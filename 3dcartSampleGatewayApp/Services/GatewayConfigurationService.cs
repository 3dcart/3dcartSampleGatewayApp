using System.Net;
using System.Web.Http.Routing;
using _3dcartSampleGatewayApp.Interfaces;
using _3dcartSampleGatewayApp.Models.Gateway;

namespace _3dcartSampleGatewayApp.Services
{
    public class GatewayConfigurationService : IGatewayConfigurationService
    {
        private IWebAPIClient _webApiClient;
        private IHttpContextService _httpContextService;

        public GatewayConfigurationService(IWebAPIClient webApiClient, IHttpContextService httpContextService)
        {
            _webApiClient = webApiClient;
            _httpContextService = httpContextService;
        }

        public bool SetWebhook(string baseUrl, GatewayToken authToken)
        {
            var success = false;

            var urlHelper = new UrlHelper();
            
            var url = _httpContextService.Request.Url;
            
            var server = $"{url.Scheme}://{url.Authority}/";

            var webooksNotifyUrl = $"{server}{"webhook"}";

            var configurationRequest = new GatewayConfigRequest() {webhook_url = webooksNotifyUrl};

            var response = _webApiClient.HTTPPostRequest(baseUrl, "configuration", configurationRequest, authToken.token);

            if (response.StatusCode == HttpStatusCode.OK) success = true;

            return success;
        }
    }
}