using System;
using System.Configuration;
using System.Web.Mvc;
using Moq;
using NUnit.Framework;
using _3dcartSampleGatewayApp.Controllers;
using _3dcartSampleGatewayApp.Helpers;
using _3dcartSampleGatewayApp.Interfaces;
using _3dcartSampleGatewayApp.Models.Cart;
using _3dcartSampleGatewayApp.Models.Gateway;

namespace _3dcartSampleGatewayAppTest.Controllers
{
    [TestFixture()]
    class CheckoutControllerTest
    {
       
        private Mock<IGatewayAuthenticationService> _gatewayAuthenticationService;
        private Mock<IGatewayCheckoutService> _gatewayChekoutService;
        private Mock<ICompleteOrderService> _completeOrderService;

        private CheckoutController _checkoutController;

        [SetUp]
        public void SetUp()
        {
            _gatewayAuthenticationService = new Mock<IGatewayAuthenticationService>();
            _gatewayChekoutService = new Mock<IGatewayCheckoutService>();
            _completeOrderService = new Mock<ICompleteOrderService>();
            _completeOrderService.Setup(x => x.DelayExecution(It.IsAny<int>()));
            _checkoutController = new CheckoutController(_gatewayAuthenticationService.Object, _gatewayChekoutService.Object, _completeOrderService.Object);
        }


        [Test]
        public void Webhook_BadRequest_InvalidType_Test()
        {
            var webhook = new Mock<GatewayWebhook>().Object;
            webhook.type = "type not expected";
            
            var actResult = _checkoutController.Webhook(webhook);
            
            Assert.IsNotNull(actResult);
            Assert.IsInstanceOf<HttpStatusCodeResult>(actResult);
            var httpResult = actResult as HttpStatusCodeResult;
            Assert.IsNotNull(httpResult);
            Assert.AreEqual(400, httpResult.StatusCode);
            Assert.AreEqual("Invalid Webhook Type.", httpResult.StatusDescription);
        }

        [Test]
        public void Webhook_BadRequest_InvalidEvent_Test()
        {
            var webhook = new Mock<GatewayWebhook>().Object;
            webhook.type = "order_update";
            webhook.@event = "event not expected";

            var actResult = _checkoutController.Webhook(webhook);

            Assert.IsNotNull(actResult);
            Assert.IsInstanceOf<HttpStatusCodeResult>(actResult);
            var httpResult = actResult as HttpStatusCodeResult;
            Assert.IsNotNull(httpResult);
            Assert.AreEqual(400, httpResult.StatusCode);
            Assert.AreEqual("Invalid Webhook Event.", httpResult.StatusDescription);
        }

        [Test]
        public void Webhook_BadRequest_ObjectIdNull_Test()
        {
            var webhook = new Mock<GatewayWebhook>().Object;
            webhook.type = "order_update";
            webhook.@event = "order_complete";
            webhook.object_uuid = null;

            var actResult = _checkoutController.Webhook(webhook);

            Assert.IsNotNull(actResult);
            Assert.IsInstanceOf<HttpStatusCodeResult>(actResult);
            var httpResult = actResult as HttpStatusCodeResult;
            Assert.IsNotNull(httpResult);
            Assert.AreEqual(400, httpResult.StatusCode);
            Assert.AreEqual("Invalid Webhook Object.", httpResult.StatusDescription);

        }

        [Test]
        public void Webhook_BadRequest_ObjectIdEmpty_Test()
        {
            var webhook = new Mock<GatewayWebhook>().Object;
            webhook.type = "order_update";
            webhook.@event = "order_complete";
            webhook.object_uuid = "";

            var actResult = _checkoutController.Webhook(webhook);

            Assert.IsNotNull(actResult);
            Assert.IsInstanceOf<HttpStatusCodeResult>(actResult);
            var httpResult = actResult as HttpStatusCodeResult;
            Assert.IsNotNull(httpResult);
            Assert.AreEqual(400, httpResult.StatusCode);
            Assert.AreEqual("Invalid Webhook Object.", httpResult.StatusDescription);
        }
        
        [Test]
        public void Webhook_BadRequest_ObjectIdNotValid_Test()
        {
            var webhook = new Mock<GatewayWebhook>().Object;
            webhook.type = "order_update";
            webhook.@event = "order_complete";
            webhook.object_uuid = "test";

            var actResult = _checkoutController.Webhook(webhook);

            Assert.IsNotNull(actResult);
            Assert.IsInstanceOf<HttpStatusCodeResult>(actResult);
            var httpResult = actResult as HttpStatusCodeResult;
            Assert.IsNotNull(httpResult);
            Assert.AreEqual(400, httpResult.StatusCode);
            Assert.AreEqual("Invalid Webhook Object.", httpResult.StatusDescription);
        }

        [Test]
        public void Webhook_GetRequestThrowsException_Test()
        {
            var webhook = new Mock<GatewayWebhook>().Object;
            webhook.type = "order_update";
            webhook.@event = "order_complete";
            webhook.object_uuid = "123_321";
            
            _completeOrderService.Setup(x => x.GetCurrentRequest(It.IsAny<int>())).Throws(new Exception());
            
            var actResult = _checkoutController.Webhook(webhook);

            Assert.IsNotNull(actResult);
            Assert.IsInstanceOf<HttpStatusCodeResult>(actResult);
            var httpResult = actResult as HttpStatusCodeResult;
            Assert.IsNotNull(httpResult);
            Assert.AreEqual(500, httpResult.StatusCode);
            Assert.AreEqual("Error getting request info from database.", httpResult.StatusDescription);
        }
        
        [Test]
        public void Webhook_GetRequestReturnNull_Test()
        {
            var webhook = new Mock<GatewayWebhook>().Object;
            webhook.type = "order_update";
            webhook.@event = "order_complete";
            webhook.object_uuid = "123_321";

            _completeOrderService.Setup(x => x.GetCurrentRequest(It.IsAny<int>())).Returns(() => null);

            var actResult = _checkoutController.Webhook(webhook);

            Assert.IsNotNull(actResult);
            Assert.IsInstanceOf<HttpStatusCodeResult>(actResult);
            var httpResult = actResult as HttpStatusCodeResult;
            Assert.IsNotNull(httpResult);
            Assert.AreEqual(500, httpResult.StatusCode);
            Assert.AreEqual("Error getting request info from database.", httpResult.StatusDescription);
        }
        
        [Test]
        public void Webhook_RequestIsCompleted_Test()
        {
            var webhook = new Mock<GatewayWebhook>().Object;
            webhook.type = "order_update";
            webhook.@event = "order_complete";
            webhook.object_uuid = "123_321";

            var request = new Mock<CheckoutRequest>().Object;
            request.status = Status.Completed;

            _completeOrderService.Setup(x => x.GetCurrentRequest(It.IsAny<int>())).Returns(request);
            
            var actResult = _checkoutController.Webhook(webhook);

            Assert.IsNotNull(actResult);
            Assert.IsInstanceOf<HttpStatusCodeResult>(actResult);
            var httpResult = actResult as HttpStatusCodeResult;
            Assert.IsNotNull(httpResult);
            Assert.AreEqual(200, httpResult.StatusCode);
        }
        
        [Test]
        public void Webhook_GetTokenThrowsException_Test()
        {
            var webhook = new Mock<GatewayWebhook>().Object;
            webhook.type = "order_update";
            webhook.@event = "order_complete";
            webhook.object_uuid = "123_321";

            var request = new Mock<CheckoutRequest>().Object;
            request.status = Status.Pending;

            _completeOrderService.Setup(x => x.GetCurrentRequest(It.IsAny<int>())).Returns(request);
            
            _gatewayAuthenticationService.Setup(x => x.GetGatewayToken(It.IsAny<string>(), It.IsAny<string>())).Throws(new Exception());

            _completeOrderService.Setup(x => x.CompleteOrder(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(),It.IsAny<CheckoutRequest>(), It.IsAny<GatewayOrderDetails>())).Returns(true);

            var actResult = _checkoutController.Webhook(webhook);

            Assert.IsNotNull(actResult);
            Assert.IsInstanceOf<HttpStatusCodeResult>(actResult);
            var httpResult = actResult as HttpStatusCodeResult;
            Assert.IsNotNull(httpResult);
            Assert.AreEqual(500, httpResult.StatusCode);
            Assert.AreEqual("Gateway authentication failed.", httpResult.StatusDescription);
        }
        
        [Test]
        public void Webhook_GetTokenReturnNull_Test()
        {
            var webhook = new Mock<GatewayWebhook>().Object;
            webhook.type = "order_update";
            webhook.@event = "order_complete";
            webhook.object_uuid = "123_321";

            var request = new Mock<CheckoutRequest>().Object;
            request.status = Status.Pending;

            _completeOrderService.Setup(x => x.GetCurrentRequest(It.IsAny<int>())).Returns(request);

            _gatewayAuthenticationService.Setup(x => x.GetGatewayToken(It.IsAny<string>(), It.IsAny<string>())).Returns(() => null);

            _completeOrderService.Setup(x => x.CompleteOrder(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CheckoutRequest>(), It.IsAny<GatewayOrderDetails>())).Returns(true);

            var actResult = _checkoutController.Webhook(webhook);

            Assert.IsNotNull(actResult);
            Assert.IsInstanceOf<HttpStatusCodeResult>(actResult);
            var httpResult = actResult as HttpStatusCodeResult;
            Assert.IsNotNull(httpResult);
            Assert.AreEqual(500, httpResult.StatusCode);
            Assert.AreEqual("Gateway authentication failed.", httpResult.StatusDescription);
        }
        
        [Test]
        public void Webhook_GetOrderDetailThrowsException_Test()
        {
            var webhook = new Mock<GatewayWebhook>().Object;
            webhook.type = "order_update";
            webhook.@event = "order_complete";
            webhook.object_uuid = "123_321";

            var request = new Mock<CheckoutRequest>().Object;
            request.status = Status.Pending;
            request.id = 123;
            request.orderid = 321;

            _completeOrderService.Setup(x => x.GetCurrentRequest(It.IsAny<int>())).Returns(request);
            
            var token = new Mock<GatewayToken>().Object;
            token.token = Guid.NewGuid().ToString("N");
            token.expiration_date = DateTime.Now.AddDays(1).ToString();

            _gatewayAuthenticationService.Setup(x => x.GetGatewayToken(It.IsAny<string>(), It.IsAny<string>())).Returns(token);

            _completeOrderService.Setup(x => x.CompleteOrder(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CheckoutRequest>(), It.IsAny<GatewayOrderDetails>())).Returns(true);

            _gatewayChekoutService.Setup(x => x.GetGatewayOrderDetails(It.IsAny<string>(), It.IsAny<GatewayToken>())).Throws(new Exception());
            
            var actResult = _checkoutController.Webhook(webhook);

            Assert.IsNotNull(actResult);
            Assert.IsInstanceOf<HttpStatusCodeResult>(actResult);
            var httpResult = actResult as HttpStatusCodeResult;
            Assert.IsNotNull(httpResult);
            Assert.AreEqual(500, httpResult.StatusCode);
            Assert.AreEqual("Error getting order details.", httpResult.StatusDescription);
        }
        
        [Test]
        public void Webhook_GetOrderDetailReturnNull_Test()
        {
            var webhook = new Mock<GatewayWebhook>().Object;
            webhook.type = "order_update";
            webhook.@event = "order_complete";
            webhook.object_uuid = "123_321";

            var request = new Mock<CheckoutRequest>().Object;
            request.status = Status.Pending;
            request.id = 123;
            request.orderid = 321;

            _completeOrderService.Setup(x => x.GetCurrentRequest(It.IsAny<int>())).Returns(request);

            var token = new Mock<GatewayToken>().Object;
            token.token = Guid.NewGuid().ToString("N");
            token.expiration_date = DateTime.Now.AddDays(1).ToString();

            _gatewayAuthenticationService.Setup(x => x.GetGatewayToken(It.IsAny<string>(), It.IsAny<string>())).Returns(token);

            _completeOrderService.Setup(x => x.CompleteOrder(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CheckoutRequest>(), It.IsAny<GatewayOrderDetails>())).Returns(true);

            _gatewayChekoutService.Setup(x => x.GetGatewayOrderDetails(It.IsAny<string>(), It.IsAny<GatewayToken>())).Returns(() => null);

            var actResult = _checkoutController.Webhook(webhook);

            Assert.IsNotNull(actResult);
            Assert.IsInstanceOf<HttpStatusCodeResult>(actResult);
            var httpResult = actResult as HttpStatusCodeResult;
            Assert.IsNotNull(httpResult);
            Assert.AreEqual(500, httpResult.StatusCode);
            Assert.AreEqual("Error getting order details.", httpResult.StatusDescription);
        }
        
        [Test]
        public void Webhook_OK_Test()
        {
            var webhook = new Mock<GatewayWebhook>().Object;
            webhook.type = "order_update";
            webhook.@event = "order_complete";
            webhook.object_uuid = "123_321";

            var request = new Mock<CheckoutRequest>().Object;
            request.status = Status.Pending;
            request.id = 123;
            request.orderid = 321;

            _completeOrderService.Setup(x => x.GetCurrentRequest(It.IsAny<int>())).Returns(request);

            var token = new Mock<GatewayToken>().Object;
            token.token = Guid.NewGuid().ToString("N");
            token.expiration_date = DateTime.Now.AddDays(1).ToString();

            _gatewayAuthenticationService.Setup(x => x.GetGatewayToken(It.IsAny<string>(), It.IsAny<string>())).Returns(token);

            _completeOrderService.Setup(x => x.CompleteOrder(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CheckoutRequest>(), It.IsAny<GatewayOrderDetails>())).Returns(true);
            
            var orderDetails = new Mock<GatewayOrderDetails>().Object;

            _gatewayChekoutService.Setup(x => x.GetGatewayOrderDetails(It.IsAny<string>(), It.IsAny<GatewayToken>())).Returns(orderDetails);

            var actResult = _checkoutController.Webhook(webhook);

            Assert.IsNotNull(actResult);
            Assert.IsInstanceOf<HttpStatusCodeResult>(actResult);
            var httpResult = actResult as HttpStatusCodeResult;
            Assert.IsNotNull(httpResult);
            Assert.AreEqual(200, httpResult.StatusCode);
        }

        

        //-------------------------------------------------------------------------------------------------------------

        [Test]
        public void CompleteCheckout_BadRequest_Test()
        {
            var requestid = 1;
            var randomKey = Guid.NewGuid().ToString("N");
            var key = Md5Helper.GetMd5Hash(requestid + randomKey + randomKey + requestid);
            var actResult = _checkoutController.CompleteCheckout(5, randomKey, key);
            Assert.IsNotNull(actResult);
            Assert.IsInstanceOf<HttpStatusCodeResult>(actResult);
            var httpResult = actResult as HttpStatusCodeResult;
            Assert.IsNotNull(httpResult);
            Assert.AreEqual(400, httpResult.StatusCode);
            Assert.AreEqual("Key parameter is not valid.", httpResult.StatusDescription);
        }

        [Test]
        public void CompleteCheckout_GetRequestThrowsException_Test()
        {
            var requestid = 1;
            var randomKey = Guid.NewGuid().ToString("N");
            var key = Md5Helper.GetMd5Hash(requestid + randomKey + randomKey + requestid);

            _completeOrderService.Setup(x => x.GetCurrentRequest(It.IsAny<int>())).Throws(new Exception());

            var actResult = _checkoutController.CompleteCheckout(requestid, randomKey, key);
            
            Assert.IsNotNull(actResult);
            Assert.IsInstanceOf<HttpStatusCodeResult>(actResult);
            var httpResult = actResult as HttpStatusCodeResult;
            Assert.IsNotNull(httpResult);
            Assert.AreEqual(500, httpResult.StatusCode);
            Assert.AreEqual("Error getting current request from database.", httpResult.StatusDescription);
        }

        [Test]
        public void CompleteCheckout_GetRequestReturnNull_Test()
        {
            var requestid = 1;
            var randomKey = Guid.NewGuid().ToString("N");
            var key = Md5Helper.GetMd5Hash(requestid + randomKey + randomKey + requestid);
            
            _completeOrderService.Setup(x => x.GetCurrentRequest(It.IsAny<int>())).Returns(() => null);

            var actResult = _checkoutController.CompleteCheckout(requestid, randomKey, key);

            Assert.IsNotNull(actResult);
            Assert.IsInstanceOf<HttpStatusCodeResult>(actResult);
            var httpResult = actResult as HttpStatusCodeResult;
            Assert.IsNotNull(httpResult);
            Assert.AreEqual(500, httpResult.StatusCode);
            Assert.AreEqual("Error getting current request from database.", httpResult.StatusDescription);
        }
        
        [Test]
        public void CompleteCheckout_AuthenticationThrowsException_Test()
        {
            var requestid = 1;
            var randomKey = Guid.NewGuid().ToString("N");
            var key = Md5Helper.GetMd5Hash(requestid + randomKey + randomKey + requestid);
            
            Mock<CheckoutRequest> requestMock = new Mock<CheckoutRequest>();
            requestMock.Object.errorurl = "https://www.store1.com/error.asp?error=100";
            requestMock.Object.username = "user1";
            requestMock.Object.password = "password1";

            _completeOrderService.Setup(x => x.GetCurrentRequest(It.IsAny<int>())).Returns(requestMock.Object);

            _completeOrderService.Setup(x => x.UpdateRequest(It.IsAny<int>(), It.IsAny<Status>())).Returns(true);

            _completeOrderService.Setup(x=>x.CompleteOrder(It.IsAny<int>(),It.IsAny<string>(),It.IsAny<string>(),It.IsAny<CheckoutRequest>(),It.IsAny<GatewayOrderDetails>())).Returns(true);
            
            _gatewayAuthenticationService.Setup(m => m.GetGatewayToken(It.IsAny<string>(), It.IsAny<string>())).Throws(new Exception());
            
            var actResult = _checkoutController.CompleteCheckout(requestid, randomKey, key);

            Assert.IsNotNull(actResult);
            Assert.IsInstanceOf<RedirectResult> (actResult);
            var httpResult = actResult as RedirectResult;
            Assert.IsNotNull(httpResult);
          
            Assert.AreEqual("https://www.store1.com/error.asp?error=100", httpResult.Url);
        }
        
        [Test]
        public void CompleteCheckout_AuthenticationReturnNullObject_Test()
        {
            var requestid = 1;
            var randomKey = Guid.NewGuid().ToString("N");
            var key = Md5Helper.GetMd5Hash(requestid + randomKey + randomKey + requestid);

            Mock<CheckoutRequest> requestMock = new Mock<CheckoutRequest>();
            requestMock.Object.errorurl = "https://www.store1.com/error.asp?error=100";
            requestMock.Object.username = "user1";
            requestMock.Object.password = "password1";

            _completeOrderService.Setup(x => x.GetCurrentRequest(It.IsAny<int>())).Returns(requestMock.Object);

            _completeOrderService.Setup(x => x.UpdateRequest(It.IsAny<int>(), It.IsAny<Status>())).Returns(true);

            _completeOrderService.Setup(x => x.CompleteOrder(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CheckoutRequest>(), It.IsAny<GatewayOrderDetails>())).Returns(true);

            _gatewayAuthenticationService.Setup(m => m.GetGatewayToken(It.IsAny<string>(), It.IsAny<string>())).Returns(()=>null);

            var actResult = _checkoutController.CompleteCheckout(requestid, randomKey, key);

            Assert.IsNotNull(actResult);
            Assert.IsInstanceOf<RedirectResult>(actResult);
            var httpResult = actResult as RedirectResult;
            Assert.IsNotNull(httpResult);

            Assert.AreEqual("https://www.store1.com/error.asp?error=100", httpResult.Url);
        }
        
        [Test]
        public void CompleteCheckout_GatewayOrderDetailsThrowsException_Test()
        {
            var requestid = 1;
            var randomKey = Guid.NewGuid().ToString("N");
            var key = Md5Helper.GetMd5Hash(requestid + randomKey + randomKey + requestid);

            Mock<CheckoutRequest> requestMock = new Mock<CheckoutRequest>();
            requestMock.Object.errorurl = "https://www.store1.com/error.asp?error=100";
            requestMock.Object.username = "user1";
            requestMock.Object.password = "password1";

            _completeOrderService.Setup(x => x.GetCurrentRequest(It.IsAny<int>())).Returns(requestMock.Object);

            _completeOrderService.Setup(x => x.UpdateRequest(It.IsAny<int>(), It.IsAny<Status>())).Returns(true);

            _completeOrderService.Setup(x => x.CompleteOrder(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CheckoutRequest>(), It.IsAny<GatewayOrderDetails>())).Returns(true);
            
            Mock<GatewayToken> gatewayAuthToken = new Mock<GatewayToken>();
            gatewayAuthToken.Object.token = Guid.NewGuid().ToString("N");
            gatewayAuthToken.Object.expiration_date = DateTime.Now.AddDays(1).ToString();
            
            _gatewayAuthenticationService.Setup(m => m.GetGatewayToken(It.IsAny<string>(), It.IsAny<string>())).Returns(gatewayAuthToken.Object);


            _gatewayChekoutService.Setup(x=>x.GetGatewayOrderDetails(It.IsAny<string>(), It.IsAny<GatewayToken>())).Throws(new Exception());


            var actResult = _checkoutController.CompleteCheckout(requestid, randomKey, key);

            Assert.IsNotNull(actResult);
            Assert.IsInstanceOf<RedirectResult>(actResult);
            var httpResult = actResult as RedirectResult;
            Assert.IsNotNull(httpResult);

            Assert.AreEqual("https://www.store1.com/error.asp?error=100", httpResult.Url);
        }
        
        [Test]
        public void CompleteCheckout_GatewayOrderDetailsReturnNullObject_Test()
        {
            var requestid = 1;
            var randomKey = Guid.NewGuid().ToString("N");
            var key = Md5Helper.GetMd5Hash(requestid + randomKey + randomKey + requestid);

            Mock<CheckoutRequest> requestMock = new Mock<CheckoutRequest>();
            requestMock.Object.errorurl = "https://www.store1.com/error.asp?error=100";
            requestMock.Object.username = "user1";
            requestMock.Object.password = "password1";

            _completeOrderService.Setup(x => x.GetCurrentRequest(It.IsAny<int>())).Returns(requestMock.Object);

            _completeOrderService.Setup(x => x.UpdateRequest(It.IsAny<int>(), It.IsAny<Status>())).Returns(true);

            _completeOrderService.Setup(x => x.CompleteOrder(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CheckoutRequest>(), It.IsAny<GatewayOrderDetails>())).Returns(true);

            Mock<GatewayToken> gatewayAuthToken = new Mock<GatewayToken>();
            gatewayAuthToken.Object.token = Guid.NewGuid().ToString("N");
            gatewayAuthToken.Object.expiration_date = DateTime.Now.AddDays(1).ToString();

            _gatewayAuthenticationService.Setup(m => m.GetGatewayToken(It.IsAny<string>(), It.IsAny<string>())).Returns(gatewayAuthToken.Object);

            _gatewayChekoutService.Setup(x => x.GetGatewayOrderDetails(It.IsAny<string>(), It.IsAny<GatewayToken>())).Returns(() => null);
            
            var actResult = _checkoutController.CompleteCheckout(requestid, randomKey, key);

            Assert.IsNotNull(actResult);
            Assert.IsInstanceOf<RedirectResult>(actResult);
            var httpResult = actResult as RedirectResult;
            Assert.IsNotNull(httpResult);

            Assert.AreEqual("https://www.store1.com/error.asp?error=100", httpResult.Url);
        }
        
        [Test]
        public void CompleteCheckout_CompleteOrderFailed_Test()
        {
            var requestid = 1;
            var randomKey = Guid.NewGuid().ToString("N");
            var key = Md5Helper.GetMd5Hash(requestid + randomKey + randomKey + requestid);

            Mock<CheckoutRequest> requestMock = new Mock<CheckoutRequest>();
            requestMock.Object.errorurl = "https://www.store1.com/error.asp?error=100";
            requestMock.Object.returnurl = "https://www.store1.com/paymentreceive.asp?gw=900001&rk=8nv6s5vyf862E1ED9AD0B1784284A149&k=a7a1bc5d2e538f41cd224012c4051eaa";
            requestMock.Object.username = "user1";
            requestMock.Object.password = "password1";

            _completeOrderService.Setup(x => x.GetCurrentRequest(It.IsAny<int>())).Returns(requestMock.Object);

            _completeOrderService.Setup(x => x.UpdateRequest(It.IsAny<int>(), It.IsAny<Status>())).Returns(true);

            Mock<GatewayToken> gatewayAuthToken = new Mock<GatewayToken>();
            gatewayAuthToken.Object.token = Guid.NewGuid().ToString("N");
            gatewayAuthToken.Object.expiration_date = DateTime.Now.AddDays(1).ToString();

            _gatewayAuthenticationService.Setup(m => m.GetGatewayToken(It.IsAny<string>(), It.IsAny<string>())).Returns(gatewayAuthToken.Object);


            Mock<GatewayOrderDetails> gatewayOrderDetails = new Mock<GatewayOrderDetails>();
            _gatewayChekoutService.Setup(x => x.GetGatewayOrderDetails(It.IsAny<string>(), It.IsAny<GatewayToken>())).Returns(gatewayOrderDetails.Object);


            _completeOrderService.Setup(x => x.CompleteOrder(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CheckoutRequest>(), It.IsAny<GatewayOrderDetails>())).Returns(false);
            
            var actResult = _checkoutController.CompleteCheckout(requestid, randomKey, key);

            Assert.IsNotNull(actResult);
            Assert.IsInstanceOf<RedirectResult>(actResult);
            var httpResult = actResult as RedirectResult;
            Assert.IsNotNull(httpResult);

            Assert.AreEqual("https://www.store1.com/error.asp?error=100", httpResult.Url);
        }
        
        [Test]
        public void CompleteCheckout_CompleteOrderOK_Test()
        {
            var requestid = 1;
            var randomKey = Guid.NewGuid().ToString("N");
            var key = Md5Helper.GetMd5Hash(requestid + randomKey + randomKey + requestid);

            Mock<CheckoutRequest> requestMock = new Mock<CheckoutRequest>();
            requestMock.Object.errorurl = "https://www.store1.com/error.asp?error=100";
            requestMock.Object.returnurl = "https://www.store1.com/paymentreceive.asp?gw=900001&rk=8nv6s5vyf862E1ED9AD0B1784284A149&k=a7a1bc5d2e538f41cd224012c4051eaa";
            requestMock.Object.username = "user1";
            requestMock.Object.password = "password1";

            _completeOrderService.Setup(x => x.GetCurrentRequest(It.IsAny<int>())).Returns(requestMock.Object);

            Mock<GatewayToken> gatewayAuthToken = new Mock<GatewayToken>();
            gatewayAuthToken.Object.token = Guid.NewGuid().ToString("N");
            gatewayAuthToken.Object.expiration_date = DateTime.Now.AddDays(1).ToString();

            _gatewayAuthenticationService.Setup(m => m.GetGatewayToken(It.IsAny<string>(), It.IsAny<string>())).Returns(gatewayAuthToken.Object);

            _completeOrderService.Setup(x => x.UpdateRequest(It.IsAny<int>(), It.IsAny<Status>())).Returns(true);

            Mock<GatewayOrderDetails> gatewayOrderDetails = new Mock<GatewayOrderDetails>();
            _gatewayChekoutService.Setup(x => x.GetGatewayOrderDetails(It.IsAny<string>(), It.IsAny<GatewayToken>())).Returns(gatewayOrderDetails.Object);


            _completeOrderService.Setup(x => x.CompleteOrder(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CheckoutRequest>(), It.IsAny<GatewayOrderDetails>())).Returns(true);

            var actResult = _checkoutController.CompleteCheckout(requestid, randomKey, key);

            Assert.IsNotNull(actResult);
            Assert.IsInstanceOf<RedirectResult>(actResult);
            var httpResult = actResult as RedirectResult;
            Assert.IsNotNull(httpResult);

            Assert.AreEqual("https://www.store1.com/paymentreceive.asp?gw=900001&rk=8nv6s5vyf862E1ED9AD0B1784284A149&k=a7a1bc5d2e538f41cd224012c4051eaa", httpResult.Url);
        }
        

        //-----------------------------------------------------------------------------------------------------------------

        [Test]
        public void InitiateCheckoutTransactionTypeValidationTest()
        {
            CheckoutResponse response = null;
            JsonResult actResult = null;
            //Transaction type validation
            var request = new Mock<CheckoutRequest>();
            request.Object.type = "any wrong type";

            actResult = _checkoutController.InitiateCheckout(request.Object);
            Assert.IsNotNull(actResult);
            Assert.IsInstanceOf<JsonResult>(actResult);
            
            response = actResult.Data as CheckoutResponse;
            Assert.IsNotNull(response);
            Assert.IsInstanceOf<CheckoutResponse>(response);

            Assert.That("100", Is.EqualTo(response.errorcode));
            Assert.That("Bad Request. Invalid Transaction Type.", Is.EqualTo(response.errormessage));
        }

        [Test]
        public void InitiateCheckoutSignatureValidationTest()
        {
           
            CheckoutResponse response = null;
            JsonResult actResult = null;
            
            // Signature validation
            var randomKey = Guid.NewGuid().ToString("N");
            var request = new Mock<CheckoutRequest>().Object;
            request.type = "sale";
            request.randomkey = randomKey;
            request.orderid = 123;
            request.invoice = "AB-123";
            request.amounttotal = 500;
            request.signature = "4545A3SD22DD";
            
            actResult = _checkoutController.InitiateCheckout(request);
            Assert.IsNotNull(actResult);
            Assert.IsInstanceOf<JsonResult>(actResult);

            response = actResult.Data as CheckoutResponse;
            Assert.IsNotNull(response);
            Assert.IsInstanceOf<CheckoutResponse>(response);

            Assert.That("100", Is.EqualTo(response.errorcode));
            Assert.That("Bad Request. Invalid Signature.", Is.EqualTo(response.errormessage));
        }
        
        [Test]
        public void InitiateCheckoutAuthenticationThrowsExceptionTest()
        {
           
            CheckoutResponse response = null;
            JsonResult actResult = null;
            
            var randomKey = Guid.NewGuid().ToString("N");
            var privateKey = ConfigurationManager.AppSettings["AppTestSecretKey"];
            var signature = Md5Helper.GetMd5Hash(randomKey + privateKey + 123 + "AB-123" + 500);
           
            var request = new Mock<CheckoutRequest>().Object;
            request.type = "sale";
            request.randomkey = randomKey;
            request.orderid = 123;
            request.invoice = "AB-123";
            request.amounttotal = 500;
            request.signature = signature;
         
            _gatewayAuthenticationService.Setup(m => m.GetGatewayToken(It.IsAny<string>(), It.IsAny<string>())).Throws(new Exception());

            actResult = _checkoutController.InitiateCheckout(request);
            Assert.IsNotNull(actResult);
            Assert.IsInstanceOf<JsonResult>(actResult);
            
            response = actResult.Data as CheckoutResponse;
            Assert.IsNotNull(response);
            Assert.IsInstanceOf<CheckoutResponse>(response);

            StringAssert.AreEqualIgnoringCase("101", response.errorcode);
            StringAssert.Contains("Gateway authentication transaction failed.", response.errormessage);
            
        }

        [Test]
        public void InitiateCheckoutAuthenticationReturnNullTest()
        {

            CheckoutResponse response = null;
            JsonResult actResult = null;

            var randomKey = Guid.NewGuid().ToString("N");
            var privateKey = ConfigurationManager.AppSettings["AppTestSecretKey"];
            var signature = Md5Helper.GetMd5Hash(randomKey + privateKey + 123 + "AB-123" + 500);

            var request = new Mock<CheckoutRequest>().Object;
            request.type = "sale";
            request.randomkey = randomKey;
            request.orderid = 123;
            request.invoice = "AB-123";
            request.amounttotal = 500;
            request.signature = signature;
            
            _gatewayAuthenticationService.Setup(m => m.GetGatewayToken(It.IsAny<string>(), It.IsAny<string>())).Returns(() => null);

            actResult = _checkoutController.InitiateCheckout(request);
            Assert.IsNotNull(actResult);
            Assert.IsInstanceOf<JsonResult>(actResult);

            response = actResult.Data as CheckoutResponse;
            Assert.IsNotNull(response);
            Assert.IsInstanceOf<CheckoutResponse>(response);

            StringAssert.AreEqualIgnoringCase("101", response.errorcode);
            StringAssert.Contains("Gateway authentication transaction failed.", response.errormessage);

        }
        
        [Test]
        public void InitiateCheckout_GatewayCheckoutThrowExceptionTest()
        {
           
            CheckoutResponse response = null;
            JsonResult actResult = null;

            var randomKey = Guid.NewGuid().ToString("N");
            var privateKey = ConfigurationManager.AppSettings["AppTestSecretKey"];
            var signature = Md5Helper.GetMd5Hash(randomKey + privateKey + 123 + "AB-123" + 500);
            
            var request = new Mock<CheckoutRequest>().Object;
            request.type = "sale";
            request.randomkey = randomKey;
            request.orderid = 123;
            request.invoice = "AB-123";
            request.amounttotal = 500;
            request.signature = signature;
            
            var token = new Mock<GatewayToken>().Object;
            token.token = Guid.NewGuid().ToString("N");
            token.expiration_date = DateTime.Now.AddDays(1).ToString();
            
            _gatewayAuthenticationService.Setup(m => m.GetGatewayToken(It.IsAny<string>(), It.IsAny<string>())).Returns(token);

            _gatewayChekoutService.Setup(x => x.InitiateGatewayChechout(request, token)).Throws(new Exception());
            
            actResult = _checkoutController.InitiateCheckout(request);
            Assert.IsNotNull(actResult);
            Assert.IsInstanceOf<JsonResult>(actResult);

            response = actResult.Data as CheckoutResponse;
            Assert.IsNotNull(response);
            Assert.IsInstanceOf<CheckoutResponse>(response);
            
            
            StringAssert.AreEqualIgnoringCase("102", response.errorcode);
            StringAssert.Contains("Gateway checkout transaction failed.", response.errormessage);
        }
        
        [Test]
        public void InitiateCheckout_GatewayCheckoutReturnNullObjectTest()
        {

            CheckoutResponse response = null;
            JsonResult actResult = null;

            var randomKey = Guid.NewGuid().ToString("N");
            var privateKey = ConfigurationManager.AppSettings["AppTestSecretKey"];
            var signature = Md5Helper.GetMd5Hash(randomKey + privateKey + 123 + "AB-123" + 500);

            var request = new Mock<CheckoutRequest>().Object;
            request.type = "sale";
            request.randomkey = randomKey;
            request.orderid = 123;
            request.invoice = "AB-123";
            request.amounttotal = 500;
            request.signature = signature;

            var token = new Mock<GatewayToken>().Object;
            token.token = Guid.NewGuid().ToString("N");
            token.expiration_date = DateTime.Now.AddDays(1).ToString();

            _gatewayAuthenticationService.Setup(m => m.GetGatewayToken(It.IsAny<string>(), It.IsAny<string>())).Returns(token);

            _gatewayChekoutService.Setup(x => x.InitiateGatewayChechout(request, token)).Returns(() => null);

            actResult = _checkoutController.InitiateCheckout(request);
            Assert.IsNotNull(actResult);
            Assert.IsInstanceOf<JsonResult>(actResult);

            response = actResult.Data as CheckoutResponse;
            Assert.IsNotNull(response);
            Assert.IsInstanceOf<CheckoutResponse>(response);

            StringAssert.AreEqualIgnoringCase("102", response.errorcode);
            StringAssert.Contains("Gateway checkout transaction failed.", response.errormessage);

        }
        
        [Test]
        public void InitiateCheckoutPassedTest()
        {
            CheckoutResponse response = null;
            JsonResult actResult = null;

            var randomKey = Guid.NewGuid().ToString("N");
            var privateKey = ConfigurationManager.AppSettings["AppTestSecretKey"];
            var signature = Md5Helper.GetMd5Hash(randomKey + privateKey + 123 + "AB-123" + 500);
            
            var request = new Mock<CheckoutRequest>().Object;
            request.type = "sale";
            request.randomkey = randomKey;
            request.orderid = 123;
            request.invoice = "AB-123";
            request.amounttotal = 500;
            request.signature = signature;

            var token = new Mock<GatewayToken>().Object;
            token.token = Guid.NewGuid().ToString("N");
            token.expiration_date = DateTime.Now.AddDays(1).ToString();
            
            _gatewayAuthenticationService.Setup(m => m.GetGatewayToken(It.IsAny<string>(), It.IsAny<string>())).Returns(token);
            
            var gatewayCheckoutResponse = new Mock<GatewayCheckoutResponse>().Object;
            gatewayCheckoutResponse.checkout_url = "www.test.com";

            _gatewayChekoutService.Setup(x => x.InitiateGatewayChechout(request, token)).Returns(gatewayCheckoutResponse);

            actResult = _checkoutController.InitiateCheckout(request);
            Assert.IsNotNull(actResult);
            Assert.IsInstanceOf<JsonResult>(actResult);

            response = actResult.Data as CheckoutResponse;
            Assert.IsNotNull(response);
            Assert.IsInstanceOf<CheckoutResponse>(response);
            
            Assert.IsEmpty(response.errorcode, string.Empty);
            Assert.IsEmpty(response.errormessage, string.Empty);

            Assert.IsNotEmpty(response.redirecturl);
            StringAssert.AreEqualIgnoringCase("www.test.com", response.redirecturl);
            
            StringAssert.AreEqualIgnoringCase(Md5Helper.GetMd5Hash(response.randomkey + privateKey + "" + response.redirecturl), response.signature);
                
        }
        
    }
}
