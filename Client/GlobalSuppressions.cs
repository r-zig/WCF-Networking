// This file is used by Code Analysis to maintain SuppressMessage 
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given 
// a specific target and scoped to a namespace, type, member, etc.
//
// To add a suppression to this file, right-click the message in the 
// Error List, point to "Suppress Message(s)", and click 
// "In Project Suppression File".
// You do not need to add suppressions to this file manually.

[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Roniz", Scope = "namespace", Target = "Roniz.Networking.Client")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Roniz")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Scope = "member", Target = "Roniz.Networking.Client.ClientResolver.#BeginResolve(System.Net.Sockets.AddressFamily,System.Net.EndPoint)")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Scope = "member", Target = "Roniz.Networking.Client.ClientResolver.#CloseConnection(System.Net.Sockets.Socket)")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Scope = "member", Target = "Roniz.Networking.Client.ClientResolver.#EndConnectCallback(System.IAsyncResult)")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Scope = "member", Target = "Roniz.Networking.Client.ClientResolver.#DiscoveryResponse")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Scope = "member", Target = "Roniz.Networking.Client.ClientResolver.#ProcessResponse(System.Net.Sockets.Socket)")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Scope = "member", Target = "Roniz.Networking.Client.EndpointResolver.#ResolveExternalEndpointWorker(Roniz.Networking.Client.ResolveOperationContext)")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "resolveAddressingOptions", Scope = "member", Target = "Roniz.Networking.Client.EndpointResolver.#TryResolveEndpointUsingUPNP(Roniz.Networking.Client.ResolveAddressingOptions,System.Net.IPEndPoint&)")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Scope = "member", Target = "Roniz.Networking.Client.EndpointResolver.#TryResolveEndpointUsingUPNP(Roniz.Networking.Client.ResolveAddressingOptions,System.Net.IPEndPoint&)")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2208:InstantiateArgumentExceptionsCorrectly", Scope = "member", Target = "Roniz.Networking.Client.EndpointResolver.#TryResolveEndpointUsingRemoteServers(Roniz.Networking.Client.ResolveOperationContext,Roniz.Networking.Client.ResolveAddressingOptions,System.Net.IPEndPoint&)")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Resovler", Scope = "member", Target = "Roniz.Networking.Client.EndpointResolverFactory.#CreateDynamicEndpointResovler()")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Resovler", Scope = "member", Target = "Roniz.Networking.Client.EndpointResolverFactory.#CreateDynamicEndpointResovler(Roniz.Networking.Common.Presence.IPresenceManager)")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Scope = "member", Target = "Roniz.Networking.Client.EndpointResolverFactory.#GetOrCreateStaticPresenceManager()")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed", Scope = "member", Target = "Roniz.Networking.Client.ResolveCompletedEventArgs.#.ctor(Roniz.Networking.Common.DiscoveryResponse,System.Exception)")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1726:UsePreferredTerms", MessageId = "cancelled", Scope = "member", Target = "Roniz.Networking.Client.ResolveExternalEndpointCompletedEventArgs.#.ctor(System.Net.EndPoint,System.Exception,System.Boolean,System.Object)")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1014:MarkAssembliesWithClsCompliant")]
