using System;
using System.Configuration;
using System.IO;
using System.Net;
using Newtonsoft.Json;
using _3dcartSampleGatewayApp.Helpers;
using _3dcartSampleGatewayApp.Interfaces;
using _3dcartSampleGatewayApp.Models.Cart;
using _3dcartSampleGatewayApp.Models.Gateway;

namespace _3dcartSampleGatewayApp.Services
{
    public class GatewayCheckoutService : IGatewayCheckoutService
    {

        private IWebAPIClient _webApiClient;
        private ITranslatorService _translatorService;
        private IHttpContextService _httpContextService;
        private IRepository _repository;

        public GatewayCheckoutService(IWebAPIClient webApiClient, ITranslatorService translatorService, IHttpContextService httpContextService, IRepository repository)
        {
            _webApiClient = webApiClient;
            _translatorService = translatorService;
            _httpContextService = httpContextService;
            _repository = repository;
        }

        public GatewayOrderDetails GetGatewayOrderDetails(string orderReferenceId, GatewayToken token)
        {
            GatewayOrderDetails gatewayResponse = null;

            try
            {
                var baseUrl = ConfigurationManager.AppSettings["BaseURLWebAPIService"];
                
                var response = _webApiClient.HTTPGetRequest(baseUrl, "orders/"+ orderReferenceId, "", token.token);

                if (response != null)
                {
                    if (response.StatusCode == HttpStatusCode.OK)
                        using (var streamReader = new StreamReader(response.GetResponseStream()))
                        {
                            var responseText = streamReader.ReadToEnd();
                            gatewayResponse = JsonConvert.DeserializeObject<GatewayOrderDetails>(responseText);
                        }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return gatewayResponse;
        }

        public GatewayCheckoutResponse InitiateGatewayChechout(CheckoutRequest request, GatewayToken token)
        {
            GatewayCheckoutResponse gatewayResponse = null;
            try
            {
                var gatewayCheckoutRequest = _translatorService.GetGatewayCheckoutRequest(request);

                var url = _httpContextService.Request.Url;
                var server = $"{url.Scheme}://{url.Authority}/";
               
                int requestid = _repository.SaveCheckoutRequest(request);

                if (requestid > 0)
                {
                    var randomKey = Guid.NewGuid().ToString("N");

                    var key = Md5Helper.GetMd5Hash(requestid + randomKey + randomKey + requestid);

                    var checkoutCompleteUrl = $"{server}{"checkoutresponse"}?id={requestid}&rk={randomKey}&k={key}";

                    gatewayCheckoutRequest.checkout_complete_url = checkoutCompleteUrl;

                    gatewayCheckoutRequest.order_reference_id = requestid.ToString() + "_" + request.orderid.ToString();

                    var baseUrl = ConfigurationManager.AppSettings["BaseURLWebAPIService"];

                    var response = _webApiClient.HTTPPostRequest(baseUrl, "checkouts", gatewayCheckoutRequest, token.token);

                    if (response == null)
                    {
                        _repository.UpdateCheckoutRequestStatus(request.id, Status.Failed);
                    }
                    else
                    {
                        if (response.StatusCode == HttpStatusCode.Created)
                            using (var streamReader = new StreamReader(response.GetResponseStream()))
                            {
                                var responseText = streamReader.ReadToEnd();
                                gatewayResponse = JsonConvert.DeserializeObject<GatewayCheckoutResponse>(responseText);
                            }
                    }
                }
            }
            catch (Exception ex)
            {
                _repository.UpdateCheckoutRequestStatus(request.id, Status.Failed);
                throw ex;
            }

            return gatewayResponse;
        }
       
    }
}