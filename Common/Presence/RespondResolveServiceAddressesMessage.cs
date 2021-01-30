using System.Collections.Generic;
using System.Runtime.Serialization;
using Roniz.WCF.P2P.Messages;

namespace Roniz.Networking.Common.Presence
{
    /// <summary>
    /// respond of various known service resolver addresses
    /// </summary>
    /// <see cref="RequestResolveServiceAddressesMessage"/>
    [DataContract]
    public sealed class RespondResolveServiceAddressesMessage : FloodMessageBase
    {
        #region constructors
        public RespondResolveServiceAddressesMessage()
        {

        }

        public RespondResolveServiceAddressesMessage(bool neighbourOnly)
            : base(neighbourOnly)
        {

        }

        public RespondResolveServiceAddressesMessage(int hopCount)
            : base(hopCount)
        {

        }
        #endregion

        #region properties
        /// <summary>
        /// Sorted collection of services
        /// </summary>
        [DataMember]
        public SortedSet<ServicePresence> Services
        {
            get;
            set;
        }
        #endregion
    }
}