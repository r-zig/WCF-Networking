using System;
using Roniz.Networking.Common;

namespace Roniz.Networking.Client
{
    /// <summary>
    /// expose the discovery response
    /// </summary>
    public class ResolveCompletedEventArgs:EventArgs
    {
        #region constructors
        public ResolveCompletedEventArgs(DiscoveryResponse response, Exception exception = null)
        {
            DiscoveryResponse = response;
            Exception = exception;
        }
        #endregion

        #region Properties
        public DiscoveryResponse DiscoveryResponse
        {
            get; set;
        }

        public Exception Exception
        {
            get;
            set;
        }
        #endregion
    }
}