using System.Collections.Generic;
using System.ComponentModel;

namespace Roniz.Networking.Client
{
    /// <summary>
    /// maintain context variables for AsyncOperation of resolving
    /// </summary>
    class ResolveOperationContext
    {
        #region members
        private readonly object syncLock = new object();

        /// <summary>
        /// maintain list of client resolvers
        /// </summary>
        private readonly List<ClientResolver> clientResolvers;
        #endregion

        #region properties
        /// <summary>
        /// The AsyncOperation for this instance
        /// </summary>
        internal AsyncOperation AsyncOperation
        {
            get;
            private set;
        }

        /// <summary>
        /// determine if PostOperationCompleted already called or not
        /// </summary>
        /// <remarks>To avoid call this operation again and raise exception</remarks>
        internal bool OperationCompletedCalled
        {
            get; set;
        }

        #endregion

        #region constructors

        public ResolveOperationContext(AsyncOperation asyncOperation)
        {
            AsyncOperation = asyncOperation;
            clientResolvers = new List<ClientResolver>();
        }

        #endregion

        #region methods
        /// <summary>
        /// cancel any pending or in progress work
        /// </summary>
        internal void Cancel()
        {
            lock (syncLock)
            {
                foreach (var client in clientResolvers)
                {
                    client.Stop();
                }
                clientResolvers.Clear();
            }
        }

        internal void AddResolvers(ClientResolver clientResolver)
        {
            lock (syncLock)
            {
                if(!clientResolvers.Contains(clientResolver))
                    clientResolvers.Add(clientResolver);
            }
        }

        internal void RemoveResolver(ClientResolver clientResolver)
        {
            lock (syncLock)
            {
                clientResolvers.Remove(clientResolver);
                clientResolver.Dispose();
            }
        }
        #endregion
    }
}
