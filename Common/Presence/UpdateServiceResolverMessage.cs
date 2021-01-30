using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Roniz.WCF.P2P.Sync.Messages.BusinessLogic;

namespace Roniz.Networking.Common.Presence
{
    [DataContract]
    class UpdateServiceResolverMessage : BusinessLogicMessageBase
    {
        [DataMember]
        public IEnumerable<KeyValuePair<Guid, ServicePresence>> AddedServices { get; set; }

        [DataMember]
        public IEnumerable<KeyValuePair<Guid, ServicePresence>> RemovedServices { get; set; }
    }
}
