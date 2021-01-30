using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;
using System.Net.NetworkInformation;
using System.Threading;
using Roniz.Diagnostics.Logging;
using Roniz.Networking.Common.Interfaces;

namespace Roniz.Networking.Client
{
    /// <summary>
    /// Provide helper methods to resolve endpoint (addresses and port) of the current machine that is not support by the .Net framework yet
    /// </summary>
    /// <see cref="ResolveExternalEndpointCompletedEventArgs"/>
    public sealed class EndpointResolver : IEndpointResolver
    {
        #region Members
        private delegate void WorkerEventHandler(ResolveOperationContext resolveOperationContext);
        private SendOrPostCallback onCompletedDelegate;
        private readonly Dictionary<ResolveAddressingOptions, ResolveOperationContext> userStateToLifetime;
        private readonly Object syncLock;
        private readonly IServiceAddressResolver addressResolver;

        /// <summary>
        /// provide caching until MemoryCache will be in .net4 client profile
        /// </summary>
        private readonly Dictionary<string, IPEndPoint> cache;
        #endregion

        #region Properties

        /// <summary>
        /// The resolved external ipv4 end point
        /// </summary>
        public IPEndPoint ExternalV4Endpoint { get; private set; }

        /// <summary>
        /// whether the component working on resolving
        /// </summary>
        public bool IsBusy { get; private set; }
        #endregion

        #region Constructores
        private EndpointResolver()
        {
            syncLock = new object();
            cache = new Dictionary<string, IPEndPoint>(10);
            userStateToLifetime = new Dictionary<ResolveAddressingOptions, ResolveOperationContext>(2);
            InitializeDelegates();
            NetworkChange.NetworkAddressChanged += OnNetworkAddressChanged;
            NetworkChange.NetworkAvailabilityChanged += OnNetworkAvailabilityChanged;
        }

        public EndpointResolver(IServiceAddressResolver addressResolver)
            : this()
        {
            this.addressResolver = addressResolver;
        }

        #endregion

        #region Methods

        /// <summary>
        /// resolve external ipv4 in synchronized mode
        /// </summary>
        /// <returns>the resolved external ipv4 endpoint address if found , or null if failed to resolved</returns>
        public IPEndPoint ResolveExternalV4Endpoint()
        {
            IPEndPoint endPoint;
            IsBusy = true;
            TryResolveExternalEndpoint(ResolveAddressingOptions.Ipv4, out endPoint);
            IsBusy = false;
            return endPoint;
        }

        /// <summary>
        /// resolve the External endpoint for IP version 4 asynchronous.
        /// </summary>
        public void ResolveExternalV4EndpointAsync()
        {
            // Create an AsyncOperation for taskId.
            var resolveOperationContext = new ResolveOperationContext(AsyncOperationManager.CreateOperation(ResolveAddressingOptions.Ipv4));
            // Multiple threads will access the task dictionary,
            // so it must be locked to serialize access.
            lock (syncLock)
            {
                if (userStateToLifetime.ContainsKey(ResolveAddressingOptions.Ipv4))
                {
                    throw new InvalidOperationException("Cannot call ResolveExternalV4EndpointAsync when other method didn't completed yet");
                }

                userStateToLifetime[ResolveAddressingOptions.Ipv4] = resolveOperationContext;
                IsBusy = true;
            }

            // Start the asynchronous operation.
            var workerDelegate = new WorkerEventHandler(ResolveExternalEndpointWorker);
            workerDelegate.BeginInvoke(resolveOperationContext,
                null,
                null);
        }

        /// <summary>
        /// cancel prior resolve the external endpoint for IP version 4 asynchronous operation.
        /// </summary>
        public void ResolveExternalV4EndpointAsyncCancel()
        {
            ResolveOperationContext resolveOperationContext;
            lock (syncLock)
            {
                if (!userStateToLifetime.TryGetValue(ResolveAddressingOptions.Ipv4, out resolveOperationContext))
                    return;

                userStateToLifetime.Remove(ResolveAddressingOptions.Ipv4);
                if (resolveOperationContext.OperationCompletedCalled)
                    return;

                resolveOperationContext.AsyncOperation.PostOperationCompleted(CancelWorker, resolveOperationContext);
                resolveOperationContext.OperationCompletedCalled = true;
            }
        }

        /// <summary>
        /// Cancel the given Operation
        /// </summary>
        /// <param name="state">instance of ResolveOperationContext</param>
        private void CancelWorker(object state)
        {
            var resolveOperationContext = state as ResolveOperationContext;
            if (resolveOperationContext == null)
            {
                LogManager.GetCurrentClassLogger().Warn("CancelWorker called with state that is not instance of ResolveOperationContext or null");
                return;
            }

            resolveOperationContext.Cancel();
            InvokeResolveExternalV4EndpointCompleted(new ResolveExternalEndpointCompletedEventArgs(null, null, true, resolveOperationContext.AsyncOperation.UserSuppliedState));
        }

        private void InitializeDelegates()
        {
            onCompletedDelegate =
                new SendOrPostCallback(ResolveExternalV4EndpointComplete);
        }

        // This method is invoked via the AsyncOperation object,
        // so it is guaranteed to be executed on the correct thread.
        private void ResolveExternalV4EndpointComplete(object operationState)
        {
            var e = operationState as ResolveExternalEndpointCompletedEventArgs;

            InvokeResolveExternalV4EndpointCompleted(e);
        }

        private void InvokeResolveExternalV4EndpointCompleted(ResolveExternalEndpointCompletedEventArgs e)
        {
            var handler = ResolveExternalV4EndpointCompleted;
            if (handler != null) handler(this, e);
        }

        private void InvokeExternalV4EndpointChanged(ResolveExternalEndpointCompletedEventArgs e)
        {
            var handler = ExternalV4EndpointChanged;
            if (handler != null) handler(this, e);
        }

        // This is the method that the underlying, free-threaded 
        // asynchronous behavior will invoke.  This will happen on
        // an arbitrary thread.
        private void CompletionResolveExternalEndpoint(
            IPEndPoint endPoint,
            Exception exception,
            bool canceled,
            ResolveOperationContext resolveOperationContext)
        {

            // If the task was not previously canceled,
            // remove the task from the lifetime collection.
            if (!canceled)
            {
                lock (syncLock)
                {
                    userStateToLifetime.Remove((ResolveAddressingOptions)resolveOperationContext.AsyncOperation.UserSuppliedState);
                }
            }


            // Package the results of the operation in a 
            // ResolveExternalAddressCompletedEventArgs.
            var e =
                new ResolveExternalEndpointCompletedEventArgs(endPoint,
                exception,
                canceled,
                resolveOperationContext.AsyncOperation.UserSuppliedState);

            IsBusy = false;
            if (!resolveOperationContext.OperationCompletedCalled)
            {
                // End the task. The asyncOp object is responsible 
                // for marshaling the call.
                resolveOperationContext.AsyncOperation.PostOperationCompleted(onCompletedDelegate, e);
                resolveOperationContext.OperationCompletedCalled = true;
            }

            // Note that after the call to OperationCompleted, 
            // asyncOp is no longer usable, and any attempt to use it
            // will cause an exception to be thrown.
        }

        // Utility method for determining if a 
        // task has been canceled.
        private bool TaskCanceled(ResolveAddressingOptions taskId)
        {
            lock (syncLock)
            {
                return (userStateToLifetime.ContainsKey(taskId) == false);
            }
        }


        // This method performs the actual resolve.
        // It is executed on the worker thread.
        private void ResolveExternalEndpointWorker(ResolveOperationContext resolveOperationContext)
        {
            IPEndPoint endPoint = null;
            Exception e = null;

            // Check that the task is still active.
            // The operation may have been canceled before
            // the thread was scheduled.
            if (!TaskCanceled((ResolveAddressingOptions)resolveOperationContext.AsyncOperation.UserSuppliedState))
            {

                try
                {
                    ResolveAddressingOptions resolveAddressingOptions =
                        (ResolveAddressingOptions)resolveOperationContext.AsyncOperation.UserSuppliedState;
                    TryResolveExternalEndpoint(resolveAddressingOptions, out endPoint, resolveOperationContext);
                }
                catch (Exception exception)
                {
                    e = exception;
                }
            }

            CompletionResolveExternalEndpoint(endPoint, e, TaskCanceled((ResolveAddressingOptions)resolveOperationContext.AsyncOperation.UserSuppliedState), resolveOperationContext);
        }

        /// <summary>
        /// Try resolve the endpoint
        /// </summary>
        /// <param name="resolveAddressingOptions">determine the type of address to resolve</param>
        /// <param name="endpoint">The resolved endpoint</param>
        /// <param name="resolveOperationContext">The operation context , optional parameter , when using asynchronous pattern should not be null</param>
        /// <returns>true if succeeded to resolve, otherwise false</returns>
        private bool TryResolveExternalEndpoint(ResolveAddressingOptions resolveAddressingOptions, out IPEndPoint endpoint, ResolveOperationContext resolveOperationContext = null)
        {
            //only when call from sync methods
            if (resolveOperationContext == null)
            {
                resolveOperationContext = new ResolveOperationContext(null);
            }

            //Try resolve address from the cache first
            if (!TryResolveEndpointFromCache(resolveAddressingOptions.ToString(), out endpoint))
            {
                //Try resolve the address using UPNP
                if (!TryResolveEndpointUsingUPNP(resolveAddressingOptions, out endpoint))
                {
                    //Try resolve the address using remote server(s)
                    if (!TryResolveEndpointUsingRemoteServers(resolveOperationContext, resolveAddressingOptions, out endpoint))
                    {
                        return false;
                    }
                    InsertIntoCache(resolveAddressingOptions.ToString(), endpoint);
                }
            }

            switch (resolveAddressingOptions)
            {
                case ResolveAddressingOptions.Ipv4:
                    {
                        ExternalV4Endpoint = endpoint;
                        break;
                    }
                //case ResolveAddressingOptions.Ipv6:
                //    {
                //        ExternalV6Endpoint = endpoint;
                //        break;
                //    }
            }

            return true;
        }

        /// <summary>
        /// Try resolve the address using remote servers that respond the address
        /// </summary>
        /// <remarks>usually this is the longest operation</remarks>
        /// <param name="resolveOperationContext">The operation context</param>
        /// <param name="resolveAddressingOptions">determine the type of address to resolve</param>
        /// <param name="endpoint">The resolved endpoint</param>
        /// <returns>true if succeeded to resolve, otherwise false</returns>
        private bool TryResolveEndpointUsingRemoteServers(ResolveOperationContext resolveOperationContext, ResolveAddressingOptions resolveAddressingOptions, out IPEndPoint endpoint)
        {
            bool resolved = false;
            IPEndPoint tmpEndPoint = null;
            var waitHandle = new AutoResetEvent(false);
            var deadEndpoints = new List<IPEndPoint>();

            foreach (var server in addressResolver.GetServiceResolverAddresses())
            {
                var clientResolver = new ClientResolver(server, resolveAddressingOptions);
                //register the clientResolver to allow stop it when using asynchronous cancel
                resolveOperationContext.AddResolvers(clientResolver);

                var tempServer = server;
                clientResolver.ResolveCompleted += (s, args) =>
                                                       {
                                                           if (args.Exception != null)
                                                           {
                                                               deadEndpoints.Add(tempServer);
                                                               LogManager.GetCurrentClassLogger().Warn(args.Exception);
                                                           }
                                                           else
                                                           {
                                                               switch (resolveAddressingOptions)
                                                               {
                                                                   case ResolveAddressingOptions.Ipv4:
                                                                       {
                                                                           tmpEndPoint = new IPEndPoint(args.DiscoveryResponse.RemoteAddressIPv4, args.DiscoveryResponse.RemotePortIPv4);
                                                                           break;
                                                                       }
                                                                   case ResolveAddressingOptions.Ipv6:
                                                                       {
                                                                           tmpEndPoint = new IPEndPoint(args.DiscoveryResponse.RemoteAddressIPv6, args.DiscoveryResponse.RemotePortIPv6);
                                                                           break;
                                                                       }
                                                               }
                                                               resolved = true;
                                                           }

                                                           resolveOperationContext.RemoveResolver(clientResolver);
                                                           waitHandle.Set();
                                                       };
                clientResolver.Start();
                
                waitHandle.WaitOne();
                if (resolved)
                    break;
            }

            //remove all dead endpoints
            deadEndpoints.ForEach(ep => addressResolver.RemoveServiceResolverAddress(ep));
            endpoint = tmpEndPoint;
            return resolved;
        }

        /// <summary>
        /// Try resolve the address using UPNP
        /// </summary>
        /// <param name="resolveAddressingOptions">determine the type of address to resolve</param>
        /// <param name="endpoint">The resolved endpoint</param>
        /// <returns>true if succeeded to resolve, otherwise false</returns>
        private bool TryResolveEndpointUsingUPNP(ResolveAddressingOptions resolveAddressingOptions, out IPEndPoint endpoint)
        {
            //TODO implement UPNP in next version
            endpoint = null;
            return false;
        }

        /// <summary>
        /// Try resolve the address from cache using it's key
        /// </summary>
        /// <param name="key">the key in the cache</param>
        /// <param name="endPoint">The resolved endPoint</param>
        /// <returns>true if succeeded to resolve, otherwise false</returns>
        private bool TryResolveEndpointFromCache(string key, out IPEndPoint endPoint)
        {
            /*
             * Don't use MemoryCache yet because it is not support in the .Net client profile , 
             * only in the full .Net
             * So implement very simple caching
            */
            lock (syncLock)
            {
                return cache.TryGetValue(key, out endPoint);
            }
        }

        /// <summary>
        /// Insert the resolved endpoint into the cache
        /// </summary>
        /// <param name="key">the key in the cache</param>
        /// <param name="endpoint">The resolved endPoint</param>
        private void InsertIntoCache(string key, IPEndPoint endpoint)
        {
            lock(syncLock)
            {
                cache[key] = endpoint;
            }
        }

        private void OnNetworkAvailabilityChanged(object sender, NetworkAvailabilityEventArgs e)
        {
            LogManager.GetCurrentClassLogger().Error(new NotImplementedException("To-do implement what happen on network change"));
        }

        private void OnNetworkAddressChanged(object sender, EventArgs e)
        {
            LogManager.GetCurrentClassLogger().Error(new NotImplementedException("To-do implement what happen on network change"));
        }
        #endregion

        #region Events

        /// <summary>
        /// raised when one of the asynchronous ResolveExternal**EndpointAsync operations completed
        /// </summary>
        public event EventHandler<ResolveExternalEndpointCompletedEventArgs> ResolveExternalV4EndpointCompleted;

        /// <summary>
        /// raised when the external V4 endpoint address changed (after the first resolve already completed)
        /// </summary>
        public event EventHandler<ResolveExternalEndpointCompletedEventArgs> ExternalV4EndpointChanged;

        #endregion
    }
}
