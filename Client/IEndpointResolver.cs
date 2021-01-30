using System;
using System.Net;

namespace Roniz.Networking.Client
{
    public interface IEndpointResolver
    {
        #region Properties
        /// <summary>
        /// The resolved external ipv4 end point
        /// </summary>
        IPEndPoint ExternalV4Endpoint { get; }

        /// <summary>
        /// whether the component working on resolving
        /// </summary>
        bool IsBusy { get; }
        
        #endregion

        #region Methods

        /// <summary>
        /// resolve external ipv4 in synchronized mode
        /// </summary>
        /// <returns>the resolved external ipv4 endpoint address if found , or null if failed to resolved</returns>
        IPEndPoint ResolveExternalV4Endpoint();

        /// <summary>
        /// resolve the external endpoint for IP version 4 asynchronous.
        /// </summary>
        void ResolveExternalV4EndpointAsync();

        /// <summary>
        /// cancel prior resolve the external endpoint for IP version 4 asynchronous operation.
        /// </summary>
        void ResolveExternalV4EndpointAsyncCancel();

        #endregion

        #region events

        /// <summary>
        /// raised when one of the asynchronous ResolveExternal**EndpointAsync operations completed
        /// </summary>
        event EventHandler<ResolveExternalEndpointCompletedEventArgs> ResolveExternalV4EndpointCompleted;

        /// <summary>
        /// raised when the external V4 endpoint address changed (after the first resolve already completed)
        /// </summary>
        event EventHandler<ResolveExternalEndpointCompletedEventArgs> ExternalV4EndpointChanged;
        #endregion
    }
}