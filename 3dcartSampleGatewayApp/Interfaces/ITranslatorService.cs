using _3dcartSampleGatewayApp.Models.Cart;
using _3dcartSampleGatewayApp.Models.Gateway;

namespace _3dcartSampleGatewayApp.Interfaces
{
    public interface ITranslatorService
    {
        GatewayCheckoutRequest GetGatewayCheckoutRequest(CheckoutRequest request);

        GatewayRefundRequest GetGatewayRefundRequest(RefundRequest request);
    }
}
