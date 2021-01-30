using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Net;
using Roniz.Networking.Common;
using Roniz.Networking.Common.Presence;
using Roniz.Networking.Common.ServiceAddressResolvers;

namespace Roniz.Networking.Client
{
    /// <summary>
    /// Factory to easy the creation of EndpointResolver instances
    /// </summary>
    public static class EndpointResolverFactory
    {
        #region members
        /// <summary>
        /// static presence manager
        /// </summary>
        private static IPresenceManager _presenceManager;

        /// <summary>
        /// static lock
        /// </summary>
        private static readonly object StaticSync = new object();
        #endregion

        #region methods

        /// <summary>
        /// Provide new endpoint resolver that work with static service resolver
        /// </summary>
        /// <param name="ipAddress">address of the server that should respond to the discovery request</param>
        /// <param name="port">port of the server that should respond to the discovery request</param>
        /// <returns>EndpointResolver</returns>
        public static IEndpointResolver CreateStaticEndpointResovler(IPAddress ipAddress,int port)
        {
            Contract.Requires<ArgumentNullException>(ipAddress != null);
            Contract.Requires<ArgumentNullException>(port > 0 && port < 65535);
            var serviceAddressResolver = new StaticServiceAddressResolver(ipAddress, port);
            var endpointResolver = new EndpointResolver(serviceAddressResolver);
            return endpointResolver;
        }

        /// <summary>
        /// Provide new endpoint resolver that work with static service resolver
        /// </summary>
        /// <param name="ipEndPoints">list of optional IPEndPoints that represent servers that should respond to the discovery request</param>
        /// <returns>EndpointResolver</returns>
        public static IEndpointResolver CreateStaticEndpointResovler(List<IPEndPoint> ipEndPoints)
        {
            var serviceAddressResolver = new StaticServiceAddressResolver(ipEndPoints);
            var endpointResolver = new EndpointResolver(serviceAddressResolver);
            return endpointResolver;
        }

        /// <summary>
        /// Provide new endpoint resolver that work with dynamic service resolver
        /// </summary>
        /// <returns>EndpointResolver</returns>
        public static IEndpointResolver CreateDynamicEndpointResovler(string endpointConfigurationName = Defaults.DefaultEndpointConfigurationName)
        {
            IPresenceManager presenceManager = GetOrCreateStaticPresenceManager(endpointConfigurationName);
            var serviceAddressResolver = new DynamicServiceAddressResolver(presenceManager);
            var endpointResolver = new EndpointResolver(serviceAddressResolver);
            return endpointResolver;
        }

        /// <summary>
        /// Get or create presence manager if the static instance is not initialized yet
        /// </summary>
        /// <returns>instance of IPresenceManager</returns>
        private static IPresenceManager GetOrCreateStaticPresenceManager(string endpointConfigurationName = Defaults.DefaultEndpointConfigurationName)
        {
            lock (StaticSync)
            {
                if (_presenceManager != null)
                    return _presenceManager;

                _presenceManager = new PresenceManager(Properties.Settings.Default.HearthBitInterval, endpointConfigurationName);
                return _presenceManager;
            }
        }

        /// <summary>
        /// Provide new endpoint resolver that work with dynamic service resolver
        /// </summary>
        /// <param name="presenceManager">already initialized instance that will work with other peers to update the state of it's services</param>
        /// <returns>EndpointResolver</returns>
        public static IEndpointResolver CreateDynamicEndpointResovler(IPresenceManager presenceManager)
        {
            var serviceAddressResolver = new DynamicServiceAddressResolver(presenceManager);
            var endpointResolver = new EndpointResolver(serviceAddressResolver);
            return endpointResolver;
        }
        #endregion
    }
}