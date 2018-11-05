using System.Configuration;
using System.IO;
using System.Net;
using Newtonsoft.Json;
using _3dcartSampleGatewayApp.Interfaces;
using _3dcartSampleGatewayApp.Models.Cart;
using _3dcartSampleGatewayApp.Models.Gateway;

namespace _3dcartSampleGatewayApp.Services
{
    public class GatewayRefundService : IGatewayRefundService
    {
        private IWebAPIClient _webApiClient;
        private ITranslatorService _translatorService;
        
        public GatewayRefundService(IWebAPIClient webApiClient, ITranslatorService translatorService)
        {
            _webApiClient = webApiClient;
            _translatorService = translatorService;
        }

        public GatewayRefundResponse InitiateGatewayRefund(RefundRequest request, GatewayToken token)
        {
            GatewayRefundResponse gatewayResponse = null;

            var baseUrl = ConfigurationManager.AppSettings["BaseURLWebAPIService"];

            var gatewayRefundRequest = _translatorService.GetGatewayRefundRequest(request);

            var response = _webApiClient.HTTPPostRequest(baseUrl, "orders/" + request.transactionid + "/refund", gatewayRefundRequest, token.token);

            if (response != null)
            {
                if (response.StatusCode == HttpStatusCode.OK)
                    using (var streamReader = new StreamReader(response.GetResponseStream()))
                    {
                        var responseText = streamReader.ReadToEnd();
                        gatewayResponse = JsonConvert.DeserializeObject<GatewayRefundResponse>(responseText);
                    }
            }

            return gatewayResponse;
        }
        
    }
}