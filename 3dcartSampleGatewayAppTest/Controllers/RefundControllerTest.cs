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
    class RefundControllerTest
    {
        private Mock<IGatewayRefundService> _gatewayRefundService;
        private Mock<IGatewayAuthenticationService> _gatewayAuthenticationService;
        private RefundController _refundController;


        [SetUp]
        public void SetUp()
        {
            _gatewayRefundService = new Mock<IGatewayRefundService>();
            _gatewayAuthenticationService = new Mock<IGatewayAuthenticationService>();
            _refundController = new RefundController(_gatewayAuthenticationService.Object, _gatewayRefundService.Object);
        }
        
        [Test]
        public void InitiateRefundTransactionTypeValidationTest()
        {
            //Transaction type validation
            var request = new Mock<RefundRequest>().Object;
            request.type = "any wrong type";

            var actResult = _refundController.InitiateRefund(request);

            Assert.IsNotNull(actResult);
            Assert.IsInstanceOf<JsonResult>(actResult);
            
            var response = actResult.Data as RefundResponse;
            Assert.IsNotNull(response);
            Assert.IsInstanceOf<RefundResponse>(response);
            
            Assert.That("100", Is.EqualTo(response.errorcode));
            Assert.That("Bad Request. Invalid Transaction Type.", Is.EqualTo(response.errormessage));
        }
        
        [Test]
        public void InitiateRefundSignatureValidationTest()
        {
            // Signature validation
            var randomKey = Guid.NewGuid().ToString("N");
           
            var request = new Mock<RefundRequest>().Object;
            request.type = "void";
            request.randomkey = randomKey;
            request.orderid = 123;
            request.invoice = "AB-123";
            request.amounttotal = 500;
            request.transactionid = "1111111";
            request.signature = "4545A3SD22DD";

            var actResult = _refundController.InitiateRefund(request);
            Assert.IsNotNull(actResult);
            Assert.IsInstanceOf<JsonResult>(actResult);

            var response = actResult.Data as RefundResponse;
            Assert.IsNotNull(response);
            Assert.IsInstanceOf<RefundResponse>(response);

            Assert.That("100", Is.EqualTo(response.errorcode));
            Assert.That("Bad Request. Invalid Signature.", Is.EqualTo(response.errormessage));
        }
        
        [Test]
        public void InitiateRefundAuthenticationThrowsExceptionTest()
        {
           
            var randomKey = Guid.NewGuid().ToString("N");
            var privateKey = ConfigurationManager.AppSettings["AppTestSecretKey"];
            var signature = Md5Helper.GetMd5Hash(randomKey + privateKey + 123 + "AB-123" + "test123456");

            var request = new Mock<RefundRequest>().Object;
            request.type = "void";
            request.randomkey = randomKey;
            request.orderid = 123;
            request.invoice = "AB-123";
            request.amounttotal = 500;
            request.transactionid = "test123456";
            request.signature = signature;

            _gatewayAuthenticationService.Setup(m => m.GetGatewayToken(It.IsAny<string>(), It.IsAny<string>())).Throws(new Exception());

            var actResult = _refundController.InitiateRefund(request);
            Assert.IsNotNull(actResult);
            Assert.IsInstanceOf<JsonResult>(actResult);

            var response = actResult.Data as RefundResponse;
            Assert.IsNotNull(response);
            Assert.IsInstanceOf<RefundResponse>(response);

            StringAssert.AreEqualIgnoringCase("101", response.errorcode);
            StringAssert.Contains("Gateway authentication transaction failed.", response.errormessage);
            
        }

        [Test]
        public void InitiateRefundAuthenticationReturnNullObjectTest()
        {
            var randomKey = Guid.NewGuid().ToString("N");
            var privateKey = ConfigurationManager.AppSettings["AppTestSecretKey"];
            var signature = Md5Helper.GetMd5Hash(randomKey + privateKey + 123 + "AB-123" + "test123456");

            var request = new Mock<RefundRequest>().Object;
            request.type = "void";
            request.randomkey = randomKey;
            request.orderid = 123;
            request.invoice = "AB-123";
            request.amounttotal = 500;
            request.transactionid = "test123456";
            request.signature = signature;
            
            _gatewayAuthenticationService.Setup(m => m.GetGatewayToken(It.IsAny<string>(), It.IsAny<string>())).Returns(() => null);

            var actResult = _refundController.InitiateRefund(request);
            Assert.IsNotNull(actResult);
            Assert.IsInstanceOf<JsonResult>(actResult);

            var response = actResult.Data as RefundResponse;
            Assert.IsNotNull(response);
            Assert.IsInstanceOf<RefundResponse>(response);

            StringAssert.AreEqualIgnoringCase("101", response.errorcode);
            StringAssert.Contains("Gateway authentication transaction failed.", response.errormessage);
        }
        
        [Test]
        public void InitiateRefund_GatewayRefundThrowsException_Test()
        {
            var randomKey = Guid.NewGuid().ToString("N");
            var privateKey = ConfigurationManager.AppSettings["AppTestSecretKey"];

            var signature = Md5Helper.GetMd5Hash(randomKey + privateKey + 123 + "AB-123" + "test123456");
            
            var request = new Mock<RefundRequest>().Object;
            request.type = "void";
            request.randomkey = randomKey;
            request.orderid = 123;
            request.invoice = "AB-123";
            request.amounttotal = 500;
            request.transactionid = "test123456";
            request.signature = signature;
            
            var token = new Mock<GatewayToken>().Object;
            token.token = Guid.NewGuid().ToString("N");
            token.expiration_date = DateTime.Now.AddDays(1).ToString();
            
            _gatewayAuthenticationService.Setup(m => m.GetGatewayToken(It.IsAny<string>(), It.IsAny<string>())).Returns(token);

            _gatewayRefundService.Setup(x => x.InitiateGatewayRefund(request, token)).Throws(new Exception());

            var actResult = _refundController.InitiateRefund(request);
            Assert.IsNotNull(actResult);
            Assert.IsInstanceOf<JsonResult>(actResult);

            var response = actResult.Data as RefundResponse;
            Assert.IsNotNull(response);
            Assert.IsInstanceOf<RefundResponse>(response);


            StringAssert.AreEqualIgnoringCase("103", response.errorcode);
            StringAssert.Contains("Gateway refund transaction failed.", response.errormessage);

        }

        [Test]
        public void InitiateRefund_GatewayRefundReturnNullObject_Test()
        {
            var randomKey = Guid.NewGuid().ToString("N");
            var privateKey = ConfigurationManager.AppSettings["AppTestSecretKey"];

            var signature = Md5Helper.GetMd5Hash(randomKey + privateKey + 123 + "AB-123" + "test123456");

            var request = new Mock<RefundRequest>().Object;
            request.type = "void";
            request.randomkey = randomKey;
            request.orderid = 123;
            request.invoice = "AB-123";
            request.amounttotal = 500;
            request.transactionid = "test123456";
            request.signature = signature;

            var token = new Mock<GatewayToken>().Object;
            token.token = Guid.NewGuid().ToString("N");
            token.expiration_date = DateTime.Now.AddDays(1).ToString();

            _gatewayAuthenticationService.Setup(m => m.GetGatewayToken(It.IsAny<string>(), It.IsAny<string>())).Returns(token);

            _gatewayRefundService.Setup(x => x.InitiateGatewayRefund(request, token)).Returns(() => null);

            var actResult = _refundController.InitiateRefund(request);
            Assert.IsNotNull(actResult);
            Assert.IsInstanceOf<JsonResult>(actResult);

            var response = actResult.Data as RefundResponse;
            Assert.IsNotNull(response);
            Assert.IsInstanceOf<RefundResponse>(response);

            StringAssert.AreEqualIgnoringCase("103", response.errorcode);
            StringAssert.Contains("Gateway refund transaction failed.", response.errormessage);

        }
        
        [Test]
        public void InitiateRefundPassedTest()
        {
            var randomKey = Guid.NewGuid().ToString("N");
            var privateKey = ConfigurationManager.AppSettings["AppTestSecretKey"];
            var signature = Md5Helper.GetMd5Hash(randomKey + privateKey + 123 + "AB-123" + "test123456");
            
            var request = new Mock<RefundRequest>().Object;
            request.type = "void";
            request.randomkey = randomKey;
            request.orderid = 123;
            request.invoice = "AB-123";
            request.amounttotal = 500;
            request.transactionid = "test123456";
            request.signature = signature;

            var token = new Mock<GatewayToken>().Object;
            token.token = Guid.NewGuid().ToString("N");
            token.expiration_date = DateTime.Now.AddDays(1).ToString();
            
            _gatewayAuthenticationService.Setup(m => m.GetGatewayToken(It.IsAny<string>(), It.IsAny<string>())).Returns(token);
            
            var gatewayRefundResponse = new Mock<GatewayRefundResponse>().Object;
            gatewayRefundResponse.refund_id = "test0000";

            _gatewayRefundService.Setup(x => x.InitiateGatewayRefund(request, token)).Returns(gatewayRefundResponse);

            var actResult = _refundController.InitiateRefund(request);
            Assert.IsNotNull(actResult);
            Assert.IsInstanceOf<JsonResult>(actResult);

            var response = actResult.Data as RefundResponse;
            Assert.IsNotNull(response);
            Assert.IsInstanceOf<RefundResponse>(response);

            Assert.IsEmpty(response.errorcode, string.Empty);
            Assert.IsEmpty(response.errormessage, string.Empty);
            
            StringAssert.AreEqualIgnoringCase(Md5Helper.GetMd5Hash(response.randomkey + privateKey + request.orderid + request.invoice + response.transactionid), response.signature);

        }
        
    }
}
