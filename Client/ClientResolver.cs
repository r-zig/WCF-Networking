using System;
using System.Net;
using System.Net.Sockets;
using Roniz.Diagnostics.Logging;
using Roniz.Networking.Common;

namespace Roniz.Networking.Client
{
    /// <summary>
    /// Work against given server(s) and try to obtain the external network information
    /// </summary>
    sealed class ClientResolver : IDisposable
    {
        #region events
        public event EventHandler<ResolveCompletedEventArgs> ResolveCompleted;
        #endregion

        #region members
        private readonly EndPoint serverEndpoint;
        private readonly ResolveAddressingOptions resolveAddressingOptions;
        private Socket clientSocket;
        private bool stopped;
        private bool disposed;

        #endregion

        #region properties
        internal DiscoveryResponse DiscoveryResponse { get; private set; }

        #endregion

        #region constructor
        public ClientResolver(EndPoint serverEndpoint, ResolveAddressingOptions resolveAddressingOptions)
        {
            this.serverEndpoint = serverEndpoint;
            this.resolveAddressingOptions = resolveAddressingOptions;
        }
        #endregion

        #region methods
        /// <summary>
        /// start the resolving
        /// </summary>
        public void Start()
        {
            stopped = false;
            var addressFamily = ConvertResolveAddressingOptionsToAddressFamily(resolveAddressingOptions);
            BeginResolve(addressFamily, serverEndpoint);
        }

        /// <summary>
        /// stop the resolving
        /// </summary>
        public void Stop()
        {
            stopped = true;
            Dispose();
            LogManager.GetCurrentClassLogger().Debug("Stop...ClientResolver on socket");
        }

        private static AddressFamily ConvertResolveAddressingOptionsToAddressFamily(ResolveAddressingOptions resolveAddressingOptions)
        {
            switch (resolveAddressingOptions)
            {
                case ResolveAddressingOptions.Ipv4:
                    return AddressFamily.InterNetwork;
                case ResolveAddressingOptions.Ipv6:
                    return AddressFamily.InterNetworkV6;
                default:
                    return AddressFamily.Unknown;
            }
        }

        private void BeginResolve(AddressFamily addressFamily, EndPoint remoteEndpoint)
        {
            clientSocket = new Socket(addressFamily, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                clientSocket.BeginConnect(remoteEndpoint, EndConnectCallback, clientSocket);
            }
            catch (Exception exception)
            {
                var logger = LogManager.GetCurrentClassLogger();
                logger.Warn("Cannot BeginResolve with remoteEndpoint: {0}", remoteEndpoint);
                logger.Warn(exception);
                return;
            }
        }

        private void EndConnectCallback(IAsyncResult ar)
        {
            var client = ar.AsyncState as Socket;
            try
            {
                if (client == null)
                {
                    LogManager.GetCurrentClassLogger().Error("EndConnectCallback ar.AsyncState cannot convert to Socket");
                    return;
                }

                //avoid exception
                if (stopped)
                    return;

                client.EndConnect(ar);
                var response = ProcessResponse(client);
                EndResolve(response);
            }
            catch (Exception exception)
            {
                LogManager.GetCurrentClassLogger().Warn(exception);
                InvokeResolveCompleted(null, exception);
            }
            finally
            {
                if (client != null)
                    CloseConnection(client);
            }
        }

        /// <summary>
        /// Close the given socket
        /// </summary>
        /// <param name="connection">the socket to close</param>
        private void CloseConnection(Socket connection)
        {
            if (stopped)
                return;
            try
            {
                if (connection.Connected)
                    connection.Shutdown(SocketShutdown.Both);
                connection.Close();
            }
            catch (Exception exception)
            {
                LogManager.GetCurrentClassLogger().Warn(exception);
            }
        }

        private static DiscoveryResponse ProcessResponse(Socket client)
        {
            var stream = new NetworkStream(client, false);

            //read length
            var buffer = new byte[4];
            stream.Read(buffer, 0, buffer.Length);
            int packetlength = BitConverter.ToInt32(buffer, 0);

            //read the packet itself
            buffer = new byte[packetlength];
            stream.Read(buffer, 0, buffer.Length);
            stream.Close();

            //deserialize the byte array into object
            return DiscoveryResponse.FromByteArray(buffer);
        }

        private void EndResolve(DiscoveryResponse response)
        {
            DiscoveryResponse = response;
            InvokeResolveCompleted(response);
        }

        private void InvokeResolveCompleted(DiscoveryResponse response, Exception exception = null)
        {
            var handler = ResolveCompleted;
            if (handler == null)
                return;
            handler(this, new ResolveCompletedEventArgs(response, exception));
        }

        public void Dispose()
        {
            // If you need thread safety, use a lock around these 
            // operations, as well as in your methods that use the resource.
            if (disposed)
                return;
            if (clientSocket != null)
                clientSocket.Dispose();

            // Indicate that the instance has been disposed.
            clientSocket = null;

            // This object will be cleaned up by the Dispose method.
            // Therefore, you should call GC.SupressFinalize to
            // take this object off the finalization queue 
            // and prevent finalization code for this object
            // from executing a second time.
            GC.SuppressFinalize(this);
            disposed = true;
        }
        #endregion
    }
}
