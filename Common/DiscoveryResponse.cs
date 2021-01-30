using System;
using System.Diagnostics.Contracts;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization;

namespace Roniz.Networking.Common
{
    [DataContract]
    public class DiscoveryResponse
    {
        #region Members

        #endregion

        #region Constructores
        
        public DiscoveryResponse(Socket socket)
        {
            Contract.Requires<ArgumentNullException>(socket != null, "socket");

            AcceptIncomingTrafficMode acceptIncomingTcpIPv4Traffic;
            AcceptIncomingTrafficMode acceptIncomingTcpIPv6Traffic;

            TryDiscoverIncomingStatus(socket,
                                      out acceptIncomingTcpIPv4Traffic,
                                      out acceptIncomingTcpIPv6Traffic);

            if (socket.AddressFamily == AddressFamily.InterNetwork)
            {
                Initialize(((IPEndPoint)socket.RemoteEndPoint).Port,
                           ((IPEndPoint)socket.RemoteEndPoint).Address,
                           -1,
                           null,
                           acceptIncomingTcpIPv4Traffic,
                           acceptIncomingTcpIPv6Traffic);
            }
            else if (socket.AddressFamily == AddressFamily.InterNetworkV6)
            {
                Initialize(-1,
                           null,
                           ((IPEndPoint)socket.RemoteEndPoint).Port,
                           ((IPEndPoint)socket.RemoteEndPoint).Address,
                           AcceptIncomingTrafficMode.Unknown,
                           AcceptIncomingTrafficMode.Unknown);
            }
            else
                throw new ArgumentException(String.Format("socket.AddressFamily {0} is not supported , support only InterNetwork and InterNetworkV6", socket.AddressFamily));
        }

        public DiscoveryResponse(int remotePortIPv4,
                    IPAddress remoteAddressIPv4,
                    int remotePortIPv6,
                    IPAddress remoteAddressIPv6,
                    AcceptIncomingTrafficMode acceptIncomingTcpIPv4Traffic,
                    AcceptIncomingTrafficMode acceptIncomingTcpIPv6Traffic)
        {
            Initialize(remotePortIPv4,
                       remoteAddressIPv4,
                       remotePortIPv6,
                       remoteAddressIPv6,
                       acceptIncomingTcpIPv4Traffic,
                       acceptIncomingTcpIPv6Traffic);
        }
        #endregion

        #region Properties
        [DataMember]
        public int RemotePortIPv4
        {
            get;
            private set;
        }

        [DataMember]
        public IPAddress RemoteAddressIPv4
        {
            get;
            private set;
        }

        [DataMember]
        public int RemotePortIPv6
        {
            get;
            private set;
        }

        [DataMember]
        public IPAddress RemoteAddressIPv6
        {
            get;
            private set;
        }

        [DataMember]
        public AcceptIncomingTrafficMode AcceptIncomingTcpIPv4Traffic
        {
            get;
            private set;
        }

        [DataMember]
        public AcceptIncomingTrafficMode AcceptIncomingTcpIPv6Traffic
        {
            get;
            private set;
        }
        #endregion

        #region Methods

        private static bool TryDiscoverIncomingStatus(Socket socket, out AcceptIncomingTrafficMode acceptIncomingTcpIPv4Traffic, out AcceptIncomingTrafficMode acceptIncomingTcpIPv6Traffic)
        {
            //Send the socket to other server and let him try connect to it.
            //send some key that will identify the socket , if successfully - it accept incoming request
            //otherwise - no
            //meanwhile return UnKnown...
            //TODO implement
            acceptIncomingTcpIPv4Traffic = acceptIncomingTcpIPv6Traffic = AcceptIncomingTrafficMode.Unknown;
            return false;
        }

        private void Initialize(int remotePortIPv4,
                            IPAddress remoteAddressIPv4,
                            int remotePortIPv6,
                            IPAddress remoteAddressIPv6,
                            AcceptIncomingTrafficMode acceptIncomingTcpIPv4Traffic,
                            AcceptIncomingTrafficMode acceptIncomingTcpIPv6Traffic)
        {
            RemotePortIPv4 = remotePortIPv4;
            RemoteAddressIPv4 = remoteAddressIPv4;
            RemotePortIPv6 = remotePortIPv6;
            RemoteAddressIPv6 = remoteAddressIPv6;
            AcceptIncomingTcpIPv4Traffic = acceptIncomingTcpIPv4Traffic;
            AcceptIncomingTcpIPv6Traffic = acceptIncomingTcpIPv6Traffic;
        }

        /// <summary>
        /// return byte array serialized data of the instance
        /// </summary>
        /// <returns></returns>
        public static byte[] ToByteArray(DiscoveryResponse instance)
        {
            var serializer = new DataContractSerializer(typeof(DiscoveryResponse));
            var stream = new MemoryStream();
            serializer.WriteObject(stream, instance);
            stream.Flush();
            var response = stream.ToArray();
            stream.Dispose();
            return response;
        }

        public static DiscoveryResponse FromByteArray(byte[] buffer)
        {
            var serializer = new DataContractSerializer(typeof(DiscoveryResponse));
            var stream = new MemoryStream(buffer);
            var response = serializer.ReadObject(stream) as DiscoveryResponse;
            stream.Dispose();
            return response;
        }
        #endregion
    }
}