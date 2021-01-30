using System.Runtime.Serialization;
using Roniz.WCF.P2P.Messages;

namespace Roniz.Networking.Common.Presence
{
    /// <summary>
    /// Request the mesh for collection of various known service resolver addresses
    /// </summary>
    [DataContract]
    public sealed class RequestResolveServiceAddressesMessage : FloodMessageBase
    {
        #region constructors
        public RequestResolveServiceAddressesMessage()
        {

        }

        public RequestResolveServiceAddressesMessage(bool neighbourOnly)
            :base(neighbourOnly)
        {

        }
        #endregion

        #region properties
        [DataMember]
        public int MaxResults
        {
            get; set;
        }
        #endregion
    }
}