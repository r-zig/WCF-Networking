using System.Collections.Generic;
using System.Net;
using System.Threading;
using Roniz.Networking.Common.Interfaces;
using Roniz.Networking.Common.Presence;

namespace Roniz.Networking.Common.ServiceAddressResolvers
{
    /// <summary>
    /// Provide dynamic service address resolver that is updated all the time by given presence manager
    /// </summary>
    public sealed class DynamicServiceAddressResolver : IServiceAddressResolver
    {
        #region members
        /// <summary>
        /// work with other peers to update the state of it's services
        /// </summary>
        private readonly IPresenceManager presenceManager;
        #endregion

        #region constructores
        public DynamicServiceAddressResolver(IPresenceManager presenceManager)
        {
            this.presenceManager = presenceManager;
        }
        #endregion

        #region Implementation of IServiceAddressResolver

        /// <summary>
        /// get collection of optional services that can act as resolver service
        /// </summary>
        /// <returns>list of IPEndPoint of optional service resolvers locations</returns>
        public IEnumerable<IPEndPoint> GetServiceResolverAddresses(bool waitWhenNoServices = true,int millisecondsTimeout = Timeout.Infinite)
        {
            var services = presenceManager.Services;
            if (services.Count > 0 || waitWhenNoServices == false)
                return services;

            var waitHandle = new ManualResetEvent(false);
            presenceManager.AddedServices += ((a, b) => waitHandle.Set());

            waitHandle.WaitOne(millisecondsTimeout);
            return presenceManager.Services;
        }

        /// <summary>
        /// notify from consumer that given serviceResolverEndpoint is not valid anymore
        /// </summary>
        /// <param name="serviceResolverEndpoint">the service resolver address that should remove</param>
        /// <remarks>
        /// When consumer try to resolve it's information and the service appear to be dead , should call this method to remove from the internal cache this service
        /// </remarks>
        public void RemoveServiceResolverAddress(IPEndPoint serviceResolverEndpoint)
        {
            //TODO should notify the presence manager and it should have some logic that in some case (for example , when notify more than once) it will remove this service from it's internal cache and will notify the other participants that this service is not accessible
            //presenceManager.
        }

        #endregion
    }
}