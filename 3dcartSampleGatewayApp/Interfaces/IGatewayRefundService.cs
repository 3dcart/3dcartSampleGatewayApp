using _3dcartSampleGatewayApp.Models.Cart;
using _3dcartSampleGatewayApp.Models.Gateway;

namespace _3dcartSampleGatewayApp.Interfaces
{
    public interface IGatewayRefundService
    {
        GatewayRefundResponse InitiateGatewayRefund(RefundRequest request, GatewayToken token);
    }
}
