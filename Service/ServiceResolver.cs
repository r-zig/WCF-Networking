using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Roniz.Diagnostics.Logging;
using Roniz.Networking.Client;
using Roniz.Networking.Common;
using Roniz.Networking.Common.Presence;
using Roniz.Networking.Service.Resources;

namespace Roniz.Networking.Service
{
    /// <summary>
    /// provide addressing resolver
    /// </summary>
    public sealed class ServiceResolver : IDisposable
    {
        #region members
        private Socket listenerV4;
        private IPAddress listenAddressV4;
        private int listenPortV4;
        private int backlogV4;

        private Socket listenerV6;
        private IPAddress listenAddressV6;
        private int listenPortV6;
        private int backlogV6;

        private readonly AsyncCallback endAcceptSocketCallback;
        private readonly Dictionary<AddressFamily, Socket> listenerDictionary;
        private readonly Guid ownId = Guid.NewGuid();
        private ServicePresence servicePresence;

        /// <summary>
        /// The p2p infrastructure to allow synchronized service state with other participants
        /// </summary>
        private readonly PresenceManager presenceManager;

        private bool disposed;

        #endregion

        #region Constructores
        public ServiceResolver()
        {
            endAcceptSocketCallback = new AsyncCallback(EndAcceptSocketCallback);
            listenerDictionary = new Dictionary<AddressFamily, Socket>(2);
            presenceManager = new PresenceManager(Properties.Settings.Default.HearthBitInterval);
        }
        #endregion

        #region methods
        private void InitializeSockets()
        {
            listenAddressV4 = IPAddress.Any;
            listenAddressV6 = IPAddress.IPv6Any;

            listenPortV4 = Properties.Settings.Default.ListenPortV4;
            backlogV4 = Properties.Settings.Default.BacklogV4;

            listenPortV6 = Properties.Settings.Default.ListenPortV6;
            backlogV6 = Properties.Settings.Default.BacklogV6;

            listenerV4 = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            listenerV4.Bind(new IPEndPoint(listenAddressV4, listenPortV4));

            if (Socket.OSSupportsIPv6)
            {
                listenerV6 = new Socket(AddressFamily.InterNetworkV6, SocketType.Stream, ProtocolType.Tcp);
                listenerV6.Bind(new IPEndPoint(listenAddressV6, listenPortV6));
            }
        }

        public void Open()
        {
            StartListening();

            Task.Factory.StartNew(() =>
            {
                if (!String.IsNullOrEmpty(Properties.Settings.Default.ExternalIpv4Address) &&
                    Properties.Settings.Default.ExternalIpv4Port > 0)
                {
                    servicePresence.IPv4Endpoint = new IPEndPoint(
                        IPAddress.Parse(Properties.Settings.Default.ExternalIpv4Address),
                        Properties.Settings.Default.ExternalIpv4Port);
                    servicePresence.LastNotification = DateTime.UtcNow;

                    LogManager.GetCurrentClassLogger().Info("Own External IPv4Endpoint from configuration: {0}", servicePresence.IPv4Endpoint);
                }
                else if (Properties.Settings.Default.ExternalIpv4Port == 0)
                {
                    servicePresence.IPv4Endpoint = new IPEndPoint(
                        IPAddress.Parse(Properties.Settings.Default.ExternalIpv4Address),((IPEndPoint)listenerV4.LocalEndPoint).Port);

                    LogManager.GetCurrentClassLogger().Info("Own External IPv4Endpoint.Address from configuration and the port from local socket opened port: {0}", servicePresence.IPv4Endpoint);
                }
                else
                {
                    //throw new InvalidOperationException(ErrorMessages.MissingExternalEndpoint);
                    //in case there is no information about it's own external info - will discover it from others
                    DisoveringExternalNetworkInfo();
                }
                presenceManager.Publish(ownId, servicePresence);
            }).HandleException();
        }

        /// <summary>
        /// start listen for client request
        /// </summary>
        /// <returns>true if succeeded to listen , if error occur - return false</returns>
        private void StartListening()
        {
            InitializeSockets();

            listenerV4.Listen(backlogV4);
            LogManager.GetCurrentClassLogger().Debug("listening on :{0}", listenerV4.LocalEndPoint);
            lock (listenerDictionary)
            {
                listenerDictionary.Add(AddressFamily.InterNetwork, listenerV4);
            }
            BeginAccept(AddressFamily.InterNetwork);

            if (!Socket.OSSupportsIPv6)
                return;

            listenerV6.Listen(backlogV6);
            LogManager.GetCurrentClassLogger().Debug("listening on :{0}", listenerV6.LocalEndPoint);
            lock (listenerDictionary)
            {
                listenerDictionary.Add(AddressFamily.InterNetworkV6, listenerV6);
            }
            BeginAccept(AddressFamily.InterNetworkV6);
        }

        /// <summary>
        /// start discover the external network information using P2P
        /// </summary>
        /// <remarks>
        /// This method currently will not help in situation when
        /// service network topology does not allowed incoming traffic (such as behind firewall or NAT)
        /// because the IP address that will be discovered as the external one - probably will be ok
        /// but the port will be the temporary port that used by the socket to connect to the remote service.
        /// So , when the current service will publish this port as the "well known" one for receiving incoming requests
        /// it will be blocked for most of the sockets (for more information: see hole punching)
        /// </remarks>
        private void DisoveringExternalNetworkInfo()
        {
            var endpointResolver = EndpointResolverFactory.CreateDynamicEndpointResovler();
            servicePresence.IPv4Endpoint = endpointResolver.ResolveExternalV4Endpoint();

            if (Properties.Settings.Default.ExternalIpv4Port == 0)
            {
                if (listenerV4.LocalEndPoint != null)
                    servicePresence.IPv4Endpoint.Port = ((IPEndPoint) listenerV4.LocalEndPoint).Port;
            }
            servicePresence.LastNotification = DateTime.UtcNow;

            LogManager.GetCurrentClassLogger().Debug("Resolve external address using other service :{0}", servicePresence.IPv4Endpoint);
        }

        public void Close()
        {
            presenceManager.Dispose();
            lock (listenerDictionary)
            {
                foreach (var socket in listenerDictionary.Values)
                {
                    try
                    {
                        CloseSocket(socket);
                    }
                    catch (Exception exception)
                    {
                        LogManager.GetCurrentClassLogger().Warn(exception);
                    }
                }
            }
        }

        private void BeginAccept(AddressFamily addressFamily)
        {
            try
            {
                Socket socket;
                lock (listenerDictionary)
                {
                    socket = listenerDictionary[addressFamily];
                }
                socket.BeginAccept(endAcceptSocketCallback, socket);
            }
            catch (SocketException socketException)
            {
                LogManager.GetCurrentClassLogger().Warn(socketException);
            }
            catch (Exception exception)
            {
                LogManager.GetCurrentClassLogger().Warn(exception);
            }
        }

        private void EndAcceptSocketCallback(IAsyncResult ar)
        {
            var listenSocket = (Socket)ar.AsyncState;
            Socket client = null;
            var isExceptionThrow = false;
            try
            {
                
                client = listenSocket.EndAccept(ar);
                listenSocket.BeginAccept(endAcceptSocketCallback, listenSocket);
                if (client != null)
                {
                    Response(client);
                    CloseSocket(client);
                }
            }
            catch (ObjectDisposedException objectDisposedException)
            {
                if (disposed)
                    return;
                LogManager.GetCurrentClassLogger().Warn(objectDisposedException);
                isExceptionThrow = true;
            }
            catch (Exception exception)
            {
                LogManager.GetCurrentClassLogger().Warn(exception);
                isExceptionThrow = true;
            }
            finally
            {
                if (isExceptionThrow)
                {
                    try
                    {
                        if (client != null && client.Connected)
                        {
                            CloseSocket(client);
                        }
                    }
                    catch (Exception innerException)
                    {
                        LogManager.GetCurrentClassLogger().Error(innerException);
                    }
                }
            }
        }

        private static void CloseSocket(Socket client)
        {
            if (client.Connected)
                client.Shutdown(SocketShutdown.Both);
            client.Close();
        }

        private static void Response(Socket client)
        {
            Contract.Requires<ArgumentNullException>(client != null);
            var discoveryResponse = new DiscoveryResponse(client);

            var buffer = DiscoveryResponse.ToByteArray(discoveryResponse);

            //send length first
            client.Send(BitConverter.GetBytes(buffer.Length));

            //send buffer itself
            client.Send(buffer);

            LogManager.GetCurrentClassLogger().Debug("Client: {0} receive discovery response", discoveryResponse.RemoteAddressIPv4);
        }

        #region IDisposable Members

        public void Dispose()
        {
            Close();
            // This object will be cleaned up by the Dispose method.
            // Therefore, you should call GC.SupressFinalize to
            // take this object off the finalization queue 
            // and prevent finalization code for this object
            // from executing a second time.
            GC.SuppressFinalize(this);
            disposed = true;
        }

        #endregion

        #endregion
    }
}
