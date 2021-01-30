// This file is used by Code Analysis to maintain SuppressMessage 
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given 
// a specific target and scoped to a namespace, type, member, etc.
//
// To add a suppression to this file, right-click the message in the 
// Error List, point to "Suppress Message(s)", and click 
// "In Project Suppression File".
// You do not need to add suppressions to this file manually.

[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1020:AvoidNamespacesWithFewTypes", Scope = "namespace", Target = "Roniz.Networking.Common")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Roniz", Scope = "namespace", Target = "Roniz.Networking.Common")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1020:AvoidNamespacesWithFewTypes", Scope = "namespace", Target = "Roniz.Networking.Common.Interface")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Roniz", Scope = "namespace", Target = "Roniz.Networking.Common.Interface")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Roniz", Scope = "namespace", Target = "Roniz.Networking.Common.Presence")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1020:AvoidNamespacesWithFewTypes", Scope = "namespace", Target = "Roniz.Networking.Common.ServiceAddressResolvers")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Roniz", Scope = "namespace", Target = "Roniz.Networking.Common.ServiceAddressResolvers")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Roniz")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1014:MarkAssembliesWithClsCompliant")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1028:EnumStorageShouldBeInt32", Scope = "type", Target = "Roniz.Networking.Common.AcceptIncomingTrafficMode")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider", MessageId = "System.String.Format(System.String,System.Object)", Scope = "member", Target = "Roniz.Networking.Common.DiscoveryResponse.#.ctor(System.Net.Sockets.Socket)")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1804:RemoveUnusedLocals", MessageId = "discoverd", Scope = "member", Target = "Roniz.Networking.Common.DiscoveryResponse.#.ctor(System.Net.Sockets.Socket)")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Roniz", Scope = "namespace", Target = "Roniz.Networking.Common.Interfaces")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Scope = "member", Target = "Roniz.Networking.Common.DiscoveryResponse.#FromByteArray(System.Byte[])")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Scope = "member", Target = "Roniz.Networking.Common.DiscoveryResponse.#ToByteArray(Roniz.Networking.Common.DiscoveryResponse)")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "socket", Scope = "member", Target = "Roniz.Networking.Common.DiscoveryResponse.#TryDiscoverIncomingStatus(System.Net.Sockets.Socket,Roniz.Networking.Common.AcceptIncomingTrafficMode&,Roniz.Networking.Common.AcceptIncomingTrafficMode&)")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed", Scope = "member", Target = "Roniz.Networking.Common.ServiceAddressResolvers.DynamicServiceAddressResolver.#GetServiceResolverAddresses(System.Boolean,System.Int32)")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0", Scope = "member", Target = "Roniz.Networking.Common.DiscoveryResponse.#.ctor(System.Net.Sockets.Socket)")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed", Scope = "member", Target = "Roniz.Networking.Common.Interfaces.IServiceAddressResolver.#GetServiceResolverAddresses(System.Boolean,System.Int32)")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Unpublish", Scope = "member", Target = "Roniz.Networking.Common.Presence.IPresenceManager.#Unpublish(System.Guid,Roniz.Networking.Common.Presence.ServicePresence)")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "neighbour", Scope = "member", Target = "Roniz.Networking.Common.Presence.RequestResolveServiceAddressesMessage.#.ctor(System.Boolean)")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "neighbour", Scope = "member", Target = "Roniz.Networking.Common.Presence.RespondResolveServiceAddressesMessage.#.ctor(System.Boolean)")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly", Scope = "member", Target = "Roniz.Networking.Common.Presence.RespondResolveServiceAddressesMessage.#Services")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Scope = "member", Target = "Roniz.Networking.Common.Presence.ServicePresenceEventArgs.#.ctor(System.Collections.Generic.IEnumerable`1<System.Collections.Generic.KeyValuePair`2<System.Guid,Roniz.Networking.Common.Presence.ServicePresence>>)")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "EndPoints", Scope = "member", Target = "Roniz.Networking.Common.ServiceAddressResolvers.StaticServiceAddressResolver.#.ctor(System.Collections.Generic.List`1<System.Net.IPEndPoint>)")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed", Scope = "member", Target = "Roniz.Networking.Common.ServiceAddressResolvers.StaticServiceAddressResolver.#GetServiceResolverAddresses(System.Boolean,System.Int32)")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly", Scope = "member", Target = "Roniz.Networking.Common.Presence.SynchronizationDetail.#ServicePresenceDictionary")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1020:AvoidNamespacesWithFewTypes", Scope = "namespace", Target = "Roniz.Networking.Common.Interfaces")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists", Scope = "member", Target = "Roniz.Networking.Common.ServiceAddressResolvers.StaticServiceAddressResolver.#.ctor(System.Collections.Generic.List`1<System.Net.IPEndPoint>)")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Scope = "member", Target = "Roniz.Networking.Common.Presence.PresenceManager.#.ctor(System.TimeSpan)")]
