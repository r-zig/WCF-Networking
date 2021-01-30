using System;

namespace Roniz.Networking.Client
{
    /// <summary>
    /// represent the various resolve options , such as resolve only ipv4 address , or ipv6 , or maybe both
    /// </summary>
    [Flags]
    public enum ResolveAddressingOptions
    {
        /// <summary>
        /// IPV4 address
        /// </summary>
        Ipv4 = 1,

        /// <summary>
        /// IPV6 address
        /// </summary>
        Ipv6 = 2,
    }
}