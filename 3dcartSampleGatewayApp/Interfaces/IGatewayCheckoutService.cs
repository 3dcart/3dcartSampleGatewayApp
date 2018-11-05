using _3dcartSampleGatewayApp.Models.Cart;
using _3dcartSampleGatewayApp.Models.Gateway;

namespace _3dcartSampleGatewayApp.Interfaces
{
    public interface IGatewayCheckoutService
    {
        GatewayCheckoutResponse InitiateGatewayChechout(CheckoutRequest request, GatewayToken token);
        GatewayOrderDetails GetGatewayOrderDetails(string reference_id, GatewayToken token);
    }
}
