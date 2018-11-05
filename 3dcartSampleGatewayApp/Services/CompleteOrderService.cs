using System;
using System.Configuration;
using System.IO;
using System.Net;
using System.Threading;
using Newtonsoft.Json;
using _3dcartSampleGatewayApp.Helpers;
using _3dcartSampleGatewayApp.Interfaces;
using _3dcartSampleGatewayApp.Models.Cart;
using _3dcartSampleGatewayApp.Models.Gateway;

namespace _3dcartSampleGatewayApp.Services
{
    public class CompleteOrderService : ICompleteOrderService
    {
        private IWebAPIClient _webApiClient;
        private IRepository _repository;

        public CompleteOrderService(IWebAPIClient webApiClient, IRepository repository)
        {
            _webApiClient = webApiClient;
            _repository = repository;
        }

        public CheckoutRequest GetCurrentRequest(int id)
        {
            CheckoutRequest currentRequest = null;
            var requests = _repository.GetCheckoutRequests(id);
            if (requests.Count > 0)
            {
                if(requests[0].id==id)
                    currentRequest = requests[0];
            }
           
            return currentRequest;
        }

        public bool CompleteOrder(int approved, string errorcode, string errormessage, CheckoutRequest request, GatewayOrderDetails gatewayOrderDetails)
        {
            var success = false;

            var randomKey = Guid.NewGuid().ToString("N");
            var privateKey = ConfigurationManager.AppSettings["AppTestSecretKey"];
            var transactionId = gatewayOrderDetails.reference_id;

            var completeOrderRequest = new CompleteOrderRequest()
            {
                orderid = request.orderid,
                transactionid = transactionId,
                invoice = request.invoice,
                errormessage = errormessage,
                errorcode = errorcode,
                amount = request.amounttotal,
                approved = approved,
                randomkey = randomKey,
                signature = Md5Helper.GetMd5Hash(randomKey + privateKey + request.orderid + request.invoice + transactionId)
            };

            try
            {
                var response = _webApiClient.HTTPPostRequest(request.notificationurl, "", completeOrderRequest, null);

                CompleteOrderResponse completeOrderResponse = null;

                if (response != null)
                {
                    if (response.StatusCode == HttpStatusCode.OK)
                        using (var streamReader = new StreamReader(response.GetResponseStream()))
                        {
                            var responseText = streamReader.ReadToEnd();
                            completeOrderResponse = JsonConvert.DeserializeObject<CompleteOrderResponse>(responseText);
                        }
                }

                if (completeOrderResponse != null)
                {
                    if (completeOrderResponse.processed == 1)
                    {
                        _repository.UpdateCheckoutRequestStatus(request.id, Status.Completed);
                        success = true;
                    }
                    else
                    {
                        _repository.UpdateCheckoutRequestStatus(request.id, Status.Failed);
                    }
                }

            }
            catch {
                _repository.UpdateCheckoutRequestStatus(request.id, Status.Failed);
            }
            
            return success;

        }

        public bool UpdateRequest(int id, Status newstatus)
        {
            try
            {
                _repository.UpdateCheckoutRequestStatus(id, newstatus);
            }
            catch { return false; }

            return true;
        }

        public void DelayExecution(int seconds)
        {
            Thread.Sleep(seconds*1000);
        }
    }
}