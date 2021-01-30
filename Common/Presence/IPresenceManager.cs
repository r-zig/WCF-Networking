using System;
using System.Collections.ObjectModel;
using System.Net;

namespace Roniz.Networking.Common.Presence
{
    public interface IPresenceManager
    {
        /// <summary>
        /// Publish to the other participant it's own presence
        /// </summary>
        /// <param name="id">it's own id</param>
        /// <param name="servicePresence">it's own presence info</param>
        void Publish(Guid id, ServicePresence servicePresence);

        /// <summary>
        /// Publish to the other participant that it's own presence not exists anymore
        /// </summary>
        /// <param name="id">it's own id</param>
        /// <param name="servicePresence">it's own presence info</param>
        void Unpublish(Guid id, ServicePresence servicePresence);

        /// <summary>
        /// raised when new services resolved
        /// </summary>
        event EventHandler<ServicePresenceEventArgs> AddedServices;

        /// <summary>
        /// raised when services removed
        /// </summary>
        event EventHandler<ServicePresenceEventArgs> RemovedServices;

        /// <summary>
        /// Get the current services as IPEndpoints
        /// </summary>
        ReadOnlyCollection<IPEndPoint> Services { get; }
    }
}