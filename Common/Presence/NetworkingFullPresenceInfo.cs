using System.Runtime.Serialization;
using Roniz.WCF.P2P.Messages.Presence;

namespace Roniz.Networking.Common.Presence
{
    /// <summary>
    /// The full networking presence information that will send upon online to other peers
    /// </summary>
    [DataContract]
    public sealed class NetworkingFullPresenceInfo : FullPresenceInfo
    {
        [DataMember]
        public ServicePresence Presence { get; set; }
    }
}