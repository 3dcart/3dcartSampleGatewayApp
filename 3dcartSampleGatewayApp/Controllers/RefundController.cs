using System;
using System.Configuration;
using System.Web.Mvc;
using _3dcartSampleGatewayApp.Helpers;
using _3dcartSampleGatewayApp.Interfaces;
using _3dcartSampleGatewayApp.Models.Cart;
using _3dcartSampleGatewayApp.Models.Gateway;

namespace _3dcartSampleGatewayApp.Controllers
{
    public class RefundController : Controller
    {
        
        private IGatewayAuthenticationService _gatewayAuthenticationService;
        private IGatewayRefundService _gatewayRefundService;

        public RefundController(IGatewayAuthenticationService gatewayAuthenticationService, IGatewayRefundService gatewayRefundService)
        {
            _gatewayAuthenticationService = gatewayAuthenticationService;
            _gatewayRefundService = gatewayRefundService;
        }

        [HttpPost]
        public JsonResult InitiateRefund(RefundRequest request)
        {
            RefundResponse response = null;

            var privateKey = ConfigurationManager.AppSettings["AppTestSecretKey"];
            var randomKey = Guid.NewGuid().ToString("N");
            var errorMsg = "";

            //validations
            if (!request.type.Equals("void"))
            {
                errorMsg = "Bad Request. Invalid Transaction Type.";
                response = doRefundResponse(0, request, privateKey, randomKey, "", errorMsg, "100");
                return Json(response);
            }
            if (request.signature != Md5Helper.GetMd5Hash(request.randomkey + privateKey + request.orderid + request.invoice + request.transactionid))
            {
                errorMsg = "Bad Request. Invalid Signature.";
                response = doRefundResponse(0, request, privateKey, randomKey, "", errorMsg, "100");
                return Json(response);
            }

            //auth token
            GatewayToken token = null;
            try
            {
                token = _gatewayAuthenticationService.GetGatewayToken(request.username, request.password);
            }
            catch (Exception ex)
            {
                errorMsg = "Gateway authentication transaction failed. " + ex.Message;
                response = doRefundResponse(0, request, privateKey, randomKey, "", errorMsg, "101");
                return Json(response);
            }

            if (token == null)
            {
                errorMsg = "Gateway authentication transaction failed.";
                response = doRefundResponse(0, request, privateKey, randomKey, "", errorMsg, "101");
                return Json(response);
            }

            GatewayRefundResponse gatewayRefundResponse = null;

            //refund
            try
            {
                gatewayRefundResponse = _gatewayRefundService.InitiateGatewayRefund(request, token);
            }
            catch (Exception ex)
            {
                errorMsg = "Gateway refund transaction failed. " + ex.Message;
                response = doRefundResponse(0, request, privateKey, randomKey, "", errorMsg, "103");
                return Json(response);
            }

            if (gatewayRefundResponse == null)
            {
                errorMsg = "Gateway refund transaction failed.";
                response = doRefundResponse(0, request, privateKey, randomKey, "",  errorMsg, "103");
                return Json(response);
            }
          
            response = doRefundResponse(1, request, privateKey, randomKey, gatewayRefundResponse.refund_id, "", "");
            return Json(response);
        }


        #region PrivateMethods

        private static RefundResponse doRefundResponse(int approved, RefundRequest request, string privateKey, string randomKey, string refundid, string errorMsg, string errorCode)
        {
            return new RefundResponse()
            {
                approved = approved,
                errorcode = errorCode,
                errormessage = errorMsg,
                randomkey = randomKey,
                transactionid = refundid,
                signature = Md5Helper.GetMd5Hash(randomKey + privateKey + request.orderid + request.invoice + refundid)
            };
        }
        
        #endregion


    }
}