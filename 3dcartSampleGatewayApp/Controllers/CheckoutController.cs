using _3dcartSampleGatewayApp.Helpers;
using _3dcartSampleGatewayApp.Interfaces;
using _3dcartSampleGatewayApp.Models.Gateway;
using System;
using System.Configuration;
using System.Net;
using System.Web.Mvc;
using _3dcartSampleGatewayApp.Models.Cart;

namespace _3dcartSampleGatewayApp.Controllers
{
    public class CheckoutController : Controller
    {      
        private IGatewayAuthenticationService _gatewayAuthenticationService;
        private IGatewayCheckoutService _gatewayChekoutService;
        private ICompleteOrderService _completeOrderService;
        
        public CheckoutController(IGatewayAuthenticationService gatewayAuthenticationService, IGatewayCheckoutService gatewayChekoutService, ICompleteOrderService completeOrderService)
        {
            _gatewayAuthenticationService = gatewayAuthenticationService;
            _gatewayChekoutService = gatewayChekoutService;
            _completeOrderService = completeOrderService;
        }
        
        [HttpPost]
        public JsonResult InitiateCheckout(CheckoutRequest request)
        {
            CheckoutResponse response = null;

            var randomKey = Guid.NewGuid().ToString("N");
            var privateKey = ConfigurationManager.AppSettings["AppTestSecretKey"];
            var paymenttoken = "";
            var checkouturl = "";
            var errorMsg = "";

            //validations
            if (!request.type.Equals("sale"))
            {
                errorMsg = "Bad Request. Invalid Transaction Type.";
                response = DoCheckoutResponse(randomKey, privateKey, paymenttoken, checkouturl, "100", errorMsg);
                return Json(response);
            }
            if (request.signature != Md5Helper.GetMd5Hash(request.randomkey + privateKey + request.orderid + request.invoice + request.amounttotal))
            {
                errorMsg = "Bad Request. Invalid Signature.";
                response = DoCheckoutResponse(randomKey, privateKey, paymenttoken, checkouturl, "100", errorMsg);
                return Json(response);
            }

            //authentication
            GatewayToken token = null;
            try
            {
                token = _gatewayAuthenticationService.GetGatewayToken(request.username, request.password);
            }
            catch (Exception ex)
            {
                errorMsg = "Gateway authentication transaction failed. " + ex.Message;
                response = DoCheckoutResponse(randomKey, privateKey, paymenttoken, checkouturl, "101", errorMsg);
                return Json(response);
            }

            if (token == null)
            {
                errorMsg = "Gateway authentication transaction failed.";
                response = DoCheckoutResponse(randomKey, privateKey, paymenttoken, checkouturl, "101", errorMsg);
                return Json(response);
            }

            //checkout
            GatewayCheckoutResponse gatewayCheckoutResponse = null;
            try
            {
                gatewayCheckoutResponse = _gatewayChekoutService.InitiateGatewayChechout(request, token);
            }
            catch (Exception ex)
            {
                errorMsg = "Gateway checkout transaction failed. " + ex.Message;
                response = DoCheckoutResponse(randomKey, privateKey, paymenttoken, checkouturl, "102", errorMsg);
                return Json(response);
            }

            if (gatewayCheckoutResponse == null)
            {
                errorMsg = "Gateway checkout transaction failed.";
                response = DoCheckoutResponse(randomKey, privateKey, paymenttoken, checkouturl, "102", errorMsg);
                return Json(response);
            }
            
            checkouturl = gatewayCheckoutResponse.checkout_url;
            response = DoCheckoutResponse(randomKey, privateKey, paymenttoken, checkouturl, "", "");
            return Json(response);
        }
        
        public ActionResult CompleteCheckout(int id, string rk, string k)
        {

            if (k != Md5Helper.GetMd5Hash(id + rk + rk + id))
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Key parameter is not valid.");
                
            CheckoutRequest request = null;
            try {
                request = _completeOrderService.GetCurrentRequest(id);
            }
            catch {
                return new HttpStatusCodeResult(HttpStatusCode.InternalServerError, "Error getting current request from database.");
            }
            if(request==null)
                return new HttpStatusCodeResult(HttpStatusCode.InternalServerError, "Error getting current request from database.");
                
            
            if(request.status==Status.Completed)
                return Redirect(request.returnurl);
            
            _completeOrderService.UpdateRequest(id, Status.Running);
            

            var errorMsg = "";
            GatewayToken token = null;
            try
            {
                token = _gatewayAuthenticationService.GetGatewayToken(request.username, request.password);
            }
            catch (Exception ex)
            {
                errorMsg = "Complete order failed. Gateway Authentication Failed.";
                _completeOrderService.CompleteOrder(0, "101", errorMsg, request, null);
                return Redirect(request.errorurl);

            }
            if (token == null)
            {
                errorMsg = "Complete order failed. Gateway Authentication Failed.";
                _completeOrderService.CompleteOrder(0, "101", errorMsg, request, null);
                return Redirect(request.errorurl);
            }
            
            //get order details
            GatewayOrderDetails gatewayOrderDetailsResponse = null;
            try
            {
                var orderReferenceId = request.id.ToString() + "_" + request.orderid.ToString();
                gatewayOrderDetailsResponse = _gatewayChekoutService.GetGatewayOrderDetails(orderReferenceId, token);
            }
            catch (Exception ex)
            {
                errorMsg = "Complete order failed. Error getting order details from gateway.";
                _completeOrderService.CompleteOrder(0, "104", errorMsg, request, null);
                return Redirect(request.errorurl);
            }
            if (gatewayOrderDetailsResponse == null)
            {
                errorMsg = "Complete order failed. Error getting order details from gateway.";
                _completeOrderService.CompleteOrder(0, "104", errorMsg, request, null);
                return Redirect(request.errorurl);
            }

            
            if(!_completeOrderService.CompleteOrder(1, "", "", request, gatewayOrderDetailsResponse))
                return Redirect(request.errorurl);

            return Redirect(request.returnurl);
        }


        [HttpPost]
        public ActionResult Webhook(GatewayWebhook webhook)
        {
            _completeOrderService.DelayExecution(15);

            if (webhook.type != "order_update")
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Invalid Webhook Type.");

            if (webhook.@event != "order_complete")
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Invalid Webhook Event.");

            if (string.IsNullOrEmpty(webhook.object_uuid))
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Invalid Webhook Object.");

            int id = 0;
            if (webhook.object_uuid.Contains("_"))
            {
                int.TryParse(webhook.object_uuid.Split('_')[0], out id);
            }

            if (id <= 0)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Invalid Webhook Object.");
                
            CheckoutRequest request = null;
            try
            {
                request = _completeOrderService.GetCurrentRequest(id);
            }
            catch
            {
                return new HttpStatusCodeResult(HttpStatusCode.InternalServerError, "Error getting request info from database.");
            }
            if (request == null)
                return new HttpStatusCodeResult(HttpStatusCode.InternalServerError, "Error getting request info from database.");
                
            
            if (request.status == Status.Completed)
                return new HttpStatusCodeResult(HttpStatusCode.OK);
                
            var errorMsg = "";
            GatewayToken token = null;
            try
            {
                token = _gatewayAuthenticationService.GetGatewayToken(request.username, request.password);
            }
            catch (Exception ex)
            {
                errorMsg = "Complete order from Webhook failed. Gateway authentication transaction failed.";
                _completeOrderService.CompleteOrder(0, "101", errorMsg, request, null);
                return new HttpStatusCodeResult(HttpStatusCode.InternalServerError, "Gateway authentication failed.");

            }
            if (token == null)
            {
                errorMsg = "Complete order from Webhook failed. Gateway authentication transaction failed.";
                _completeOrderService.CompleteOrder(0, "101", errorMsg, request, null);
                return new HttpStatusCodeResult(HttpStatusCode.InternalServerError, "Gateway authentication failed.");
            }

            //get order details
            GatewayOrderDetails gatewayOrderDetailsResponse = null;
            try
            {
                var orderReferenceId = request.id.ToString() + "_" + request.orderid.ToString();
                gatewayOrderDetailsResponse = _gatewayChekoutService.GetGatewayOrderDetails(orderReferenceId, token);
            }
            catch (Exception ex)
            {
                errorMsg = "Complete order from Webhook failed. Error getting order details.";
                _completeOrderService.CompleteOrder(0, "104", errorMsg, request, null);
                return new HttpStatusCodeResult(HttpStatusCode.InternalServerError, "Error getting order details.");
            }
            if (gatewayOrderDetailsResponse == null)
            {
                errorMsg = "Complete order from Webhook failed. Error getting order details.";
                _completeOrderService.CompleteOrder(0, "104", errorMsg, request, null);
                return new HttpStatusCodeResult(HttpStatusCode.InternalServerError, "Error getting order details.");
            }

            if(!_completeOrderService.CompleteOrder(1, "", "", request, gatewayOrderDetailsResponse))
                return new HttpStatusCodeResult(HttpStatusCode.InternalServerError);

            return new HttpStatusCodeResult(HttpStatusCode.OK);

        }
        

        #region PrivateMethods

        private static CheckoutResponse DoCheckoutResponse(string randomKey, string privateKey, string paymenttoken, string checkouturl, string errorCode, string errorMsg)
        {
            return new CheckoutResponse()
            {
                errorcode = errorCode,
                errormessage = errorMsg,
                redirectmethod = "GET",
                paymenttoken = paymenttoken,
                randomkey = randomKey,
                redirecturl = checkouturl,
                signature = Md5Helper.GetMd5Hash(randomKey + privateKey + paymenttoken + checkouturl)
            };
        }
        
        #endregion

    }
}