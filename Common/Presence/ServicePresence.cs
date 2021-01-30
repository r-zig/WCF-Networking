using System;
using System.Net;
using System.Runtime.Serialization;

namespace Roniz.Networking.Common.Presence
{
    /// <summary>
    /// The service presence details
    /// </summary>
    [DataContract]
    public struct ServicePresence : IEquatable<ServicePresence>
    {
        #region properties
        
        /// <summary>
        /// The service IPv4 endpoint
        /// </summary>
        [DataMember]
        public IPEndPoint IPv4Endpoint { get; set;}

        /// <summary>
        /// The last time received hearth bit notification from the service , or receive expired notification
        /// ensure that the DateTime will be UTC DateTime because the various participants can be in various time locations
        /// </summary>
        [DataMember]
        public DateTime LastNotification { get; set; }

        #endregion

        #region methods

        public bool Equals(ServicePresence other)
        {
            if (!LastNotification.Equals(other.LastNotification))
                return false;

            if (!IPv4Endpoint.Equals(other.IPv4Endpoint))
                return false;
            return true;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is ServicePresence))
                return false;

            return Equals((ServicePresence) obj);
        }

        public override int GetHashCode()
        {
            int result = LastNotification.GetHashCode();
            result ^= IPv4Endpoint.GetHashCode();
            return result;
        }
        #endregion

        #region operator overloading
        
        public static bool operator ==(ServicePresence first, ServicePresence second)
        {
            return first.Equals(second);
        }

        public static bool operator !=(ServicePresence first, ServicePresence second)
        {
            return !first.Equals(second);
        }

        #endregion
    }
}