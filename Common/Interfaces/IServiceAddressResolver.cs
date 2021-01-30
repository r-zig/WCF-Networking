using System.Collections.Generic;
using System.Net;
using System.Threading;

namespace Roniz.Networking.Common.Interfaces
{
    /// <summary>
    /// base interface for resolving addresses of other ServiceResolver instances
    /// </summary>
    /// <remarks>
    /// the EndpointResolver use this interface to obtain the list of servers that it can use for resolving network information
    /// </remarks>
    public interface IServiceAddressResolver
    {
        /// <summary>
        /// get collection of optional services that can act as resolver service
        /// </summary>
        /// <param name="waitWhenNoServices"></param>
        /// <param name="millisecondsTimeout"></param>
        /// <returns>list of IPEndPoint of optional service resolvers locations</returns>
        IEnumerable<IPEndPoint> GetServiceResolverAddresses(bool waitWhenNoServices = true, int millisecondsTimeout = Timeout.Infinite);

        /// <summary>
        /// notify from consumer that given serviceResolverEndpoint is not valid anymore
        /// </summary>
        /// <param name="serviceResolverEndpoint">the service resolver address that should remove</param>
        /// <remarks>
        /// When consumer try to resolve it's information and the service appear to be dead , should call this method to remove from the internal cache this service
        /// </remarks>
        void RemoveServiceResolverAddress(IPEndPoint serviceResolverEndpoint);
    }
}