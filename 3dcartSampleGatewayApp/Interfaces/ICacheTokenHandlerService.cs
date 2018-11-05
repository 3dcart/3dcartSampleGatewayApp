using _3dcartSampleGatewayApp.Models.Gateway;

namespace _3dcartSampleGatewayApp.Interfaces
{
    public interface ICacheTokenHandlerService
    {
        GatewayToken GetTokenFromCache(string cacheKey);
        bool CachingValidToken(string cacheKey, GatewayToken authToken);
    }
}
