using System;
using System.Configuration;
using _3dcartSampleGatewayApp.Helpers;
using _3dcartSampleGatewayApp.Interfaces;
using _3dcartSampleGatewayApp.Models.Gateway;

namespace _3dcartSampleGatewayApp.Services
{
    public class CacheTokenHandlerService : ICacheTokenHandlerService
    {
        public bool CachingValidToken(string cacheKey, GatewayToken authToken)
        {
            var cacheExpirationTime = ConfigurationManager.AppSettings["CacheExpirationTime"];

            var result = int.TryParse(cacheExpirationTime, out var timeInMinutes);

            if (!result) timeInMinutes = 60;

            MemoryCacher.Add(cacheKey, authToken, DateTimeOffset.UtcNow.AddMinutes(timeInMinutes));

            return true;
        }

        public GatewayToken GetTokenFromCache(string cacheKey)
        {
            var token = MemoryCacher.GetValue(cacheKey);

            return token != null ? token as GatewayToken : null;
        }
    }
}