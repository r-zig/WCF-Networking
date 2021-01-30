using System.ServiceModel;

namespace Roniz.Networking.Common.Presence
{
    [ServiceContract(CallbackContract = typeof(IServicePresence))]
    public interface IServicePresence
    {
        [OperationContract(IsOneWay = true)]
        void RequestServiceAddresses(RequestResolveServiceAddressesMessage requestResolveServiceAddressesMessage);

        [OperationContract(IsOneWay = true)]
        void RespondServiceAddresses(RespondResolveServiceAddressesMessage requestResolveServiceAddressesMessage);
    }
}
