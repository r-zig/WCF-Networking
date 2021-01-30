using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Timers;
using Roniz.Diagnostics.Logging;
using Roniz.WCF.P2P.Messages.Presence;
using Roniz.WCF.P2P.Sync;
using Roniz.WCF.P2P.Sync.Enums;
using Roniz.WCF.P2P.Sync.Interfaces;
using Roniz.WCF.P2P.Sync.Messages;
using Roniz.WCF.P2P.Sync.Messages.BusinessLogic;

namespace Roniz.Networking.Common.Presence
{
    /// <summary>
    /// share the presence with other peer members
    /// </summary>
    public sealed class PresenceManager : IPresenceManager, ISynchronizationBusinessLogic, IDisposable
    {
        #region members
        private readonly ISynchronizationStateManager stateManager;
        private readonly object syncLock = new object();
        private Dictionary<Guid, ServicePresence> cachedServers = new Dictionary<Guid, ServicePresence>();
        /// <summary>
        /// store the last clean up of old entries
        /// </summary>
        private DateTime lastCleanUp = DateTime.MinValue;

        private readonly Timer hearthBitTimer;
        /// <summary>
        /// whether should check and remove expired entries in the next time the hearthBit timer elapsed
        /// </summary>
        private bool shouldCheckExpiredEntries;
        private ServicePresence ownServicePresence;
        private Guid ownId;
        private bool ownServicePublished;

        /// <summary>
        /// The default time that hearth bit should send by the instance to notify that it's alive
        /// Also this time will cause validation at interval that is bigger then it to remove expired services that didn't notified own presence in the given interval.
        /// </summary>
        private static readonly TimeSpan DefaultHearthBitInterval = TimeSpan.FromMinutes(10.0);

        private bool disposed;
        #endregion

        #region events

        /// <summary>
        /// raised when new services resolved
        /// </summary>
        public event EventHandler<ServicePresenceEventArgs> AddedServices;

        /// <summary>
        /// raised when services removed
        /// </summary>
        public event EventHandler<ServicePresenceEventArgs> RemovedServices;

        #endregion

        #region constructores
        /// <summary>
        /// initialize presence manager with it's default hearth bit time
        /// </summary>
        public PresenceManager(string endpointConfigurationName = Defaults.DefaultEndpointConfigurationName)
            : this(DefaultHearthBitInterval,endpointConfigurationName)
        {
        }

        /// <summary>
        /// initialize presence manager with it's given hearth bit time
        /// </summary>
        public PresenceManager(TimeSpan hearthBitInterval, string endpointConfigurationName = Defaults.DefaultEndpointConfigurationName)
        {
            Contract.Requires<ArgumentOutOfRangeException>(hearthBitInterval.TotalMilliseconds > 0, "The hearthBitInterval.TotalMilliseconds should be > 0");
            Contract.Requires<ArgumentOutOfRangeException>(hearthBitInterval.TotalMilliseconds * 2 < Int32.MaxValue, "The hearthBitInterval.TotalMilliseconds should not pass (Int32.MaxValue / 2)");

            stateManager = new SynchronizationStateManager(this,endpointConfigurationName);
            stateManager.PeerOnline += StateManagerPeerOnline;
            stateManager.PeerOffline += StateManagerPeerOffline;
            hearthBitTimer = new Timer(hearthBitInterval.TotalMilliseconds * 2) { AutoReset = false };
            hearthBitTimer.Elapsed += HearthBitTimerElapsed;

            Task.Factory.StartNew(() => stateManager.Open());
        }

        #endregion

        #region properties

        /// <summary>
        /// Get the current services as IPEndpoints
        /// </summary>
        public ReadOnlyCollection<IPEndPoint> Services
        {
            get
            {
                lock (syncLock)
                {
                    return new ReadOnlyCollection<IPEndPoint>(new List<IPEndPoint>(cachedServers.Values.Select(v => v.IPv4Endpoint)));
                }
            }
        }

        #region Implementation of ISynchronizationBusinessLogic
        bool ISynchronizationBusinessLogic.IsNeedFullSynchronization
        {
            get;
            set;
        }
        #endregion

        #endregion

        #region methods

        #region Implementation of ISynchronizationBusinessLogic

        void ISynchronizationBusinessLogic.OnCommunicationStateChanged(SynchronizationCommunicationState oldState, SynchronizationCommunicationState newState)
        {
        }

        /// <summary>
        /// Generate SynchronizationResponse message that will be returned back to the mesh based on given synchronizationRequest message
        /// should override to produce application specific response
        /// </summary>
        /// <remarks>this method will invoked On receiver peer side based on prior SynchronizationRequest operation</remarks>
        /// <param name="synchronizationRequest">the request received from the mesh</param>
        /// <returns>SynchronizationResponse instance or null if don't want to response</returns>
        BusinessLogicMessageBase ISynchronizationBusinessLogic.ProvideSynchronizationResponse(SynchronizationRequest synchronizationRequest)
        {
            lock (syncLock)
            {
                int keysCount = cachedServers.Keys.Count;
                List<Guid> keys;
                if (!ownServicePublished)
                {
                    //if there is nothing to send - return null
                    if (keysCount == 0)
                        return null;

                    //there is only other cached servers
                    keys = new List<Guid>(cachedServers.Keys);
                }
                else //there is own presence info
                {
                    keysCount++;
                    //only the presence info
                    if (keysCount == 1)
                        keys = new List<Guid>(keysCount) { ownId };
                    else //own presence info + other cached servers
                    {
                        keys = new List<Guid>(keysCount) { ownId };

                        //add others cached servers id's
                        keys.AddRange(cachedServers.Keys);
                    }
                }

                var synchronizationKeys = new SynchronizationKeys
                {
                    Keys = keys
                };
                return synchronizationKeys;
            }
        }

        /// <summary>
        /// Generate SynchronizationDetailsRequest message that will be send back to the mesh by the peer that want to synchronized itself based on given SynchronizationResponse message
        /// should override to produce application specific request
        /// </summary>
        /// <remarks>this method will invoked On sender peer based on prior SynchronizationResponse operation</remarks>
        /// <param name="synchronizationResponse">the response from the mesh</param>
        BusinessLogicMessageBase ISynchronizationBusinessLogic.ProvideSynchronizationDetailRequest(BusinessLogicMessageBase synchronizationResponse)
        {
            lock (syncLock)
            {
                var newKeys =
                    ((SynchronizationKeys)synchronizationResponse).Keys.Except(cachedServers.Keys);

                //filter own presence info from request
                if (ownServicePublished)
                    newKeys = newKeys.Except(new List<Guid> { ownId });

                //if there is nothing to ask to - return null
                if (newKeys.Count() == 0)
                    return null;

                return new SynchronizationKeys { Keys = newKeys.ToList() };
            }
        }

        BusinessLogicMessageBase ISynchronizationBusinessLogic.ProvideSynchronizationDetailResponse(BusinessLogicMessageBase synchronizationDetailsRequest)
        {
            lock (syncLock)
            {
                var synchronizationKeys = (SynchronizationKeys)synchronizationDetailsRequest;

                var response = new SynchronizationDetail();

                //partial synchronization with all keys
                if (synchronizationKeys != null)
                {
                    response.ServicePresenceDictionary = cachedServers.Where(item => synchronizationKeys.Keys.Contains(item.Key)).ToDictionary(t => t.Key, t => t.Value);
                }
                else //full synchronization with all keys
                {
                    response.ServicePresenceDictionary = cachedServers.ToDictionary(k => k.Key, v => v.Value);
                }

                if (ownServicePublished)
                {
                    //if the synchronization request also the own presence info - add it to the response
                    if (synchronizationKeys == null || synchronizationKeys.Keys.Contains(ownId))
                        response.ServicePresenceDictionary.Add(ownId, ownServicePresence);
                }

                //if there is nothing to response - return null to avoid sending unnecessarily message
                if (response.ServicePresenceDictionary.Count == 0)
                    return null;

                return response;
            }
        }

        BusinessLogicMessageBase ISynchronizationBusinessLogic.ProvideFullSynchronizationDetailResponse()
        {
            lock (syncLock)
            {
                var response = new SynchronizationDetail
                                   {
                                       ServicePresenceDictionary = cachedServers
                                   };
                return response;
            }
        }

        void ISynchronizationBusinessLogic.OnSynchronizationDetailsResponseReceived(BusinessLogicMessageBase synchronizationDetailsResponse)
        {
            MergeServiceResolvers(((SynchronizationDetail)synchronizationDetailsResponse).ServicePresenceDictionary);
        }

        void ISynchronizationBusinessLogic.OnUpdateReceived(BusinessLogicMessageBase stateMessage)
        {
            var updateServiceResolverMessage = stateMessage as UpdateServiceResolverMessage;
            if (updateServiceResolverMessage != null)
            {
                MergeServiceResolvers(updateServiceResolverMessage.AddedServices, updateServiceResolverMessage.RemovedServices);
            }
        }

        #endregion

        #region Implementation of IAnnouncementBusinessLogic

        FullPresenceInfo IAnnouncementBusinessLogic.ProvideFullPresenceInfo()
        {
            lock (syncLock)
            {
                if (ownServicePublished)
                {
                    return new NetworkingFullPresenceInfo
                               {
                                   UniqueName = ownId.ToString(),
                                   Presence = ownServicePresence
                               };
                }
            }
            return null;
        }

        void IAnnouncementBusinessLogic.OnOnlineAnnouncementReceived(FullPresenceInfo fullPresenceInfo)
        {
            if (fullPresenceInfo is NetworkingFullPresenceInfo)
            {
                var networkingFullPresenceInfo = fullPresenceInfo as NetworkingFullPresenceInfo;
                MergeServiceResolvers(new Dictionary<Guid, ServicePresence>
                                          {
                                              {Guid.Parse(networkingFullPresenceInfo.UniqueName),networkingFullPresenceInfo.Presence}
                                          });
            }
        }

        CompactPresenceInfo IAnnouncementBusinessLogic.ProvideCompactPresenceInfo()
        {
            lock (syncLock)
            {
                if (ownServicePublished)
                {
                    return new CompactPresenceInfo
                    {
                        UniqueName = ownId.ToString()
                    };
                }
            }
            return null;
        }

        void IAnnouncementBusinessLogic.OnOfflineAnnouncementReceived(CompactPresenceInfo compactPresenceInfo)
        {
            if (compactPresenceInfo != null)
                RemoveServiceResolver(Guid.Parse(compactPresenceInfo.UniqueName));
        }

        void IAnnouncementBusinessLogic.OnPresenceInfoChangedReceived(FullPresenceInfo fullPresenceInfo)
        {
        }

        #endregion

        /// <summary>
        /// Publish to the other participant it's own presence
        /// </summary>
        /// <param name="id">it's own id</param>
        /// <param name="servicePresence">it's own presence info</param>
        public void Publish(Guid id, ServicePresence servicePresence)
        {
            lock (syncLock)
            {
                ownId = id;
                ownServicePresence = servicePresence;
                UpdateCore(ownId, ownServicePresence, true);
                //cause the timer to stop publish this info
                ownServicePublished = true;
            }
        }

        /// <summary>
        /// Publish to the other participant that it's own presence not exists anymore
        /// </summary>
        /// <param name="id">it's own id</param>
        /// <param name="servicePresence">it's own presence info</param>
        public void Unpublish(Guid id, ServicePresence servicePresence)
        {
            lock (syncLock)
            {
                //cause the timer to stop publish this info
                ownServicePublished = false;
                UpdateCore(id, servicePresence, false);
            }
        }

        /// <summary>
        /// Check if anyone register to AddedServices event and then raised it
        /// </summary>
        /// <param name="services">The new services</param>
        private void InvokeAddedServices(IEnumerable<KeyValuePair<Guid, ServicePresence>> services)
        {
            var handler = AddedServices;
            if (handler != null)
                handler(this, new ServicePresenceEventArgs(services));
        }

        /// <summary>
        /// Check if anyone register to RemovedServices event and then raised it
        /// </summary>
        /// <param name="services">The removed services</param>
        private void InvokeRemovedServices(IEnumerable<KeyValuePair<Guid, ServicePresence>> services)
        {
            var handler = RemovedServices;
            if (handler != null)
                handler(this, new ServicePresenceEventArgs(services));
        }

        /// <summary>
        /// raised every time the state manager become online - have one or more participants
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void StateManagerPeerOnline(object sender, EventArgs e)
        {
            hearthBitTimer.Start();
        }

        /// <summary>
        /// raised every time the state manager become offline - does not have any participants
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void StateManagerPeerOffline(object sender, EventArgs e)
        {
            hearthBitTimer.Stop();
        }

        /// <summary>
        /// clean up old entries that didn't receive alive heart bit since the last time the timer passed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void HearthBitTimerElapsed(object sender, ElapsedEventArgs e)
        {
            if (shouldCheckExpiredEntries)
            {
                lock (syncLock)
                {
                    var removedServices = cachedServers.Where(kvp => IsLastNotificationBeforeDate(kvp.Value, lastCleanUp)).ToDictionary(t => t.Key, t => t.Value);
                    cachedServers = cachedServers.Except(removedServices).ToDictionary(k => k.Key, v => v.Value);
                    InvokeRemovedServices(removedServices);
                    lastCleanUp = e.SignalTime;
                }
            }

            lock (syncLock)
            {
                if (ownServicePublished)
                    UpdateCore(ownId, ownServicePresence, true);
            }

            shouldCheckExpiredEntries = !shouldCheckExpiredEntries;
            //start the timer again at the end of the work , avoid reentrance
            if (stateManager.State == SynchronizationCommunicationState.Online)
                hearthBitTimer.Start();
        }

        /// <summary>
        /// Add or update the current cached service resolvers dictionary with new data
        /// </summary>
        /// <param name="serviceKey">The key for the new data</param>
        /// <param name="servicePresence">The value of the new data</param>
        private void AddToServiceResolvers(Guid serviceKey, ServicePresence servicePresence)
        {
            lock (syncLock)
            {
                //filter own information
                if (ownServicePublished)
                {
                    if (serviceKey == ownId)
                        return;
                }
                ServicePresence oldServicePresence;
                if (cachedServers.TryGetValue(serviceKey, out oldServicePresence))
                {
                    //if there is already more updated information about this service ,should not update.
                    if (servicePresence.LastNotification <= oldServicePresence.LastNotification)
                        return;
                }

                cachedServers[serviceKey] = servicePresence;
            }
        }

        /// <summary>
        /// Remove service presence from the cache services by the given service key
        /// </summary>
        /// <param name="serviceKey"></param>
        private void RemoveServiceResolver(Guid serviceKey)
        {
            lock (syncLock)
            {
                cachedServers.Remove(serviceKey);
            }
        }

        /// <summary>
        /// Merge the current cached service resolvers dictionary with new entries of the given servicePresenceDictionary
        /// </summary>
        /// <param name="addedServices">contain the new entities</param>
        /// <param name="removedServices">contain the removed entities</param>
        private void MergeServiceResolvers(IEnumerable<KeyValuePair<Guid, ServicePresence>> addedServices, IEnumerable<KeyValuePair<Guid, ServicePresence>> removedServices = null)
        {
            lock (syncLock)
            {
                //filter own information
                if (ownServicePublished)
                {
                    addedServices = addedServices.SkipWhile(item => item.Key == ownId);
                    if (removedServices != null)
                        removedServices = removedServices.SkipWhile(item => item.Key == ownId);
                }

                //filter from new services the services that already in the cached services
                addedServices = addedServices.SkipWhile(item => cachedServers.ContainsKey(item.Key));

                ////filter the new services if they already represent as "dead" services in the cache
                //addedServices = addedServices.
                //    Union(cachedServers.SkipWhile(p => IsLastNotificationAfterDate(p.Value, lastCleanUp)));

                if (addedServices.Count() > 0)
                {
                    cachedServers = cachedServers.Concat(addedServices).ToDictionary(e => e.Key, e => e.Value);
                    InvokeAddedServices(addedServices);
                }
                //remove all expiration services that notify as expired in other participant after notified as "alive" here
                if (removedServices != null)
                {
                    var shouldRemove =
                        from removedService in removedServices
                        where (cachedServers.ContainsKey(removedService.Key) &&
                               IsLastNotificationAfterDate(removedService.Value, lastCleanUp))
                        select removedService;

                    if (shouldRemove.Count() > 0)
                    {
                        cachedServers = cachedServers.SkipWhile(p => shouldRemove.Contains(p)).ToDictionary(k => k.Key,
                                                                                                            v => v.Value);
                        InvokeRemovedServices(shouldRemove);
                    }
                }
            }
        }

        /// <summary>
        /// Determine if the given servicePresence last notified before the given date
        /// </summary>
        /// <param name="servicePresence">The instance to check</param>
        /// <param name="comparedDateTime">the compared date</param>
        /// <returns>true if the given servicePresence notified before the given date , otherwise false</returns>
        private static bool IsLastNotificationBeforeDate(ServicePresence servicePresence,
                                                        DateTime comparedDateTime)
        {
            return servicePresence.LastNotification.ToUniversalTime() < comparedDateTime.ToUniversalTime();
        }

        /// <summary>
        /// Determine if the given servicePresence last notified after the given date
        /// </summary>
        /// <param name="servicePresence">The instance to check</param>
        /// <param name="comparedDateTime">the compared date</param>
        /// <returns>true if the given servicePresence notified after the given date , otherwise false</returns>
        private static bool IsLastNotificationAfterDate(ServicePresence servicePresence,
                                                        DateTime comparedDateTime)
        {
            return servicePresence.LastNotification.ToUniversalTime() > comparedDateTime.ToUniversalTime();
        }

        /// <summary>
        /// update the mesh with the presence of your service
        /// </summary>
        /// <param name="id">The id of the service presence</param>
        /// <param name="servicePresence">the information of the service info</param>
        /// <param name="addOrRemove">flag that determine if to publish - when true , or un publish - when false</param>
        private void UpdateCore(Guid id, ServicePresence servicePresence, bool addOrRemove)
        {
            servicePresence.LastNotification = DateTime.UtcNow;
            if (stateManager.State == SynchronizationCommunicationState.Online)
            {
                var message = new UpdateServiceResolverMessage();
                var presenceDictionary = new Dictionary<Guid, ServicePresence> { { id, servicePresence } };
                if (addOrRemove)
                    message.AddedServices = presenceDictionary;
                else
                    message.RemovedServices = presenceDictionary;

                stateManager.Update(message);
            }
        }

        public void Dispose()
        {
            Dispose(true);
            // This object will be cleaned up by the Dispose method.
            // Therefore, you should call GC.SupressFinalize to
            // take this object off the finalization queue 
            // and prevent finalization code for this object
            // from executing a second time.
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            // If you need thread safety, use a lock around these 
            // operations, as well as in your methods that use the resource.
            if (disposed)
                return;

            if (disposing)
            {
                try
                {
                    hearthBitTimer.Dispose();
                    stateManager.Close();
                }
                catch (Exception exception)
                {
                    LogManager.GetCurrentClassLogger().Fatal(exception);
                }
            }
            disposed = true;
        }
        #endregion
    }
}
