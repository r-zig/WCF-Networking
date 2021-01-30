using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Roniz.WCF.P2P.Sync.Messages.BusinessLogic;

namespace Roniz.Networking.Common.Presence
{
    /// <summary>
    /// Contain dictionary of service presence instances per key , used for detailed response
    /// </summary>
    [DataContract]
    public class SynchronizationDetail : BusinessLogicMessageBase
    {
        [DataMember]
        public Dictionary<Guid, ServicePresence> ServicePresenceDictionary { get; set; }
    }
}
