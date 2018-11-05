using _3dcartSampleGatewayApp.Models.Gateway;

namespace _3dcartSampleGatewayApp.Interfaces
{
    public interface IGatewayConfigurationService
    {
        bool SetWebhook(string baseUrl, GatewayToken authToken);
    }
}
