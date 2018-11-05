using _3dcartSampleGatewayApp.Models.Gateway;

namespace _3dcartSampleGatewayApp.Interfaces
{
    public interface IGatewayAuthenticationService
    {
        GatewayToken GetGatewayToken(string user, string password);
    }
}
