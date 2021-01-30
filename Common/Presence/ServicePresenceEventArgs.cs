using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Collections.Generic;

namespace Roniz.Networking.Common.Presence
{
    /// <summary>
    /// event args containing the information about changes occur in service presence information
    /// </summary>
    public sealed class ServicePresenceEventArgs : EventArgs
    {
        #region constructores
        public ServicePresenceEventArgs(IEnumerable<KeyValuePair<Guid, ServicePresence>> servicePresence)
        {
            Services = new ReadOnlyCollection<IPEndPoint>(new List<IPEndPoint>(servicePresence.Select(kvp => kvp.Value.IPv4Endpoint)));
        }
        #endregion

        #region properties
        public ReadOnlyCollection<IPEndPoint> Services { get; private set; }
        #endregion
    }
}
