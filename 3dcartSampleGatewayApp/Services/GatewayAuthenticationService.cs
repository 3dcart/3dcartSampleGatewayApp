using _3dcartSampleGatewayApp.Interfaces;
using _3dcartSampleGatewayApp.Models.Gateway;
using System.Configuration;
using System.IO;
using System.Net;
using Newtonsoft.Json;

namespace _3dcartSampleGatewayApp.Services
{
    public class GatewayAuthenticationService : IGatewayAuthenticationService
    {
        private IWebAPIClient _webApiClient;
        private IGatewayConfigurationService _gatewayConfigurationService;
        private ICacheTokenHandlerService _cacheTokenHandlerService;

        public GatewayAuthenticationService(IWebAPIClient webApiClient, IGatewayConfigurationService gatewayConfigurationService, ICacheTokenHandlerService cacheTokenHandlerService)
        {
            _webApiClient = webApiClient;
            _gatewayConfigurationService = gatewayConfigurationService;
            _cacheTokenHandlerService = cacheTokenHandlerService;
        }

        public GatewayToken GetGatewayToken(string user, string password)
        {
            GatewayToken authToken = null;

            var cacheKey = string.Concat("##", user, "##");

            authToken = _cacheTokenHandlerService.GetTokenFromCache(cacheKey);

            if (authToken == null)
            {
                var baseUrl = ConfigurationManager.AppSettings["BaseURLWebAPIService"];

                var gatewayAuthRequest = new GatewayAuthRequest() {private_key = password, public_key = user};

                var response = _webApiClient.HTTPPostRequest(baseUrl, "authentication", gatewayAuthRequest, null);

                if (response != null)
                {
                    if (response.StatusCode == HttpStatusCode.Created)
                    {
                        using (var streamReader = new StreamReader(response.GetResponseStream()))
                        {
                            var responseText = streamReader.ReadToEnd();
                            authToken = JsonConvert.DeserializeObject<GatewayToken>(responseText);
                        }

                        //caching gateway auth token
                        _cacheTokenHandlerService.CachingValidToken(cacheKey, authToken);

                        //sending webhoook url to Gateway
                        _gatewayConfigurationService.SetWebhook(baseUrl, authToken);
                    }
                }
            }

            return authToken;
        }
    }
}