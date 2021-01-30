using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Roniz.WCF.P2P.Sync.Messages.BusinessLogic;

namespace Roniz.Networking.Common.Presence
{
    /// <summary>
    /// The compact response of only the keys for the synchronization
    /// </summary>
    [DataContract]
    public class SynchronizationKeys : BusinessLogicMessageBase
    {
        [DataMember] 
        public IEnumerable<Guid> Keys { get; set;}
    }
}
