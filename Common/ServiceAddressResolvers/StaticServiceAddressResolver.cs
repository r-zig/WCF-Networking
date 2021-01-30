using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Net;
using System.Threading;
using Roniz.Networking.Common.Interfaces;

namespace Roniz.Networking.Common.ServiceAddressResolvers
{
    /// <summary>
    /// provide static list of addresses of service resolvers
    /// </summary>
    public sealed class StaticServiceAddressResolver : IServiceAddressResolver
    {
        #region members
        private readonly List<IPEndPoint> ipEndPoints;
        private readonly object syncLock = new object();
        #endregion

        #region constructors
        /// <summary>
        /// construct new instance based on given address & port
        /// </summary>
        /// <param name="ipAddress">address of the server that should respond to the discovery request</param>
        /// <param name="port">port of the server that should respond to the discovery request</param>
        public StaticServiceAddressResolver(IPAddress ipAddress, int port)
        {
            Contract.Requires(ipAddress != null);
            Contract.Requires(port > 0 && port <= 65535);
            ipEndPoints = new List<IPEndPoint> { new IPEndPoint(ipAddress,port) };
        }

        /// <summary>
        /// construct new instance based on given list of IPEndPoints
        /// </summary>
        /// <param name="ipEndPoints">list of optional IPEndPoints that represent servers that should respond to the discovery request</param>
        public StaticServiceAddressResolver(List<IPEndPoint> ipEndPoints)
        {
            this.ipEndPoints = ipEndPoints;
        }
        #endregion

        #region Implementation of IAddressResolver

        /// <summary>
        /// get collection of optional services that can act as resolver service
        /// </summary>
        /// <param name="waitWhenNoServices"></param>
        /// <param name="millisecondsTimeout"></param>
        /// <returns>list of IPEndPoint of optional service resolvers locations</returns>
        public IEnumerable<IPEndPoint> GetServiceResolverAddresses(bool waitWhenNoServices = true, int millisecondsTimeout = Timeout.Infinite)
        {
            return ipEndPoints;
        }

        public void RemoveServiceResolverAddress(IPEndPoint serviceResolverEndpoint)
        {
            lock (syncLock)
            {
                ipEndPoints.Remove(serviceResolverEndpoint);
            }
        }
        #endregion
    }
}