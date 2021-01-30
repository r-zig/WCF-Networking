using System;
using System.ComponentModel;
using System.Net;

namespace Roniz.Networking.Client
{
    /// <summary>
    /// Provide the resolved address , or any other completed information such as Exception, cancel etc
    /// </summary>
    /// <see cref="EndpointResolver"/>
    public sealed class ResolveExternalEndpointCompletedEventArgs: AsyncCompletedEventArgs
    {
        #region Members
        private readonly EndPoint endPoint;
        #endregion

        #region Constructores
        public ResolveExternalEndpointCompletedEventArgs(EndPoint endPoint, Exception error, bool cancelled, object userState)
            : base(error, cancelled, userState)
        {
            this.endPoint = endPoint;
        }
        #endregion

        public EndPoint EndPoint
        {
            get
            {
                // Raise an exception if the operation failed or 
                // was canceled.
                RaiseExceptionIfNecessary();

                return endPoint;
            }
        }
    }
}