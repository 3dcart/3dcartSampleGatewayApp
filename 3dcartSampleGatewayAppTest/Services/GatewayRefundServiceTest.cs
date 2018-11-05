using System;
using System.IO;
using System.Net;
using System.Text;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;
using _3dcartSampleGatewayApp.Interfaces;
using _3dcartSampleGatewayApp.Models.Cart;
using _3dcartSampleGatewayApp.Models.Gateway;
using _3dcartSampleGatewayApp.Services;

namespace _3dcartSampleGatewayAppTest.Services
{
    [TestFixture()]
    class GatewayRefundServiceTest
    {

        private Mock<ITranslatorService> _translatorService;
        private Mock<IWebAPIClient> _webApiClient;
        private GatewayRefundService _gatewayRefundService;

        [SetUp]
        public void SetUp()
        {
            _translatorService = new Mock<ITranslatorService>();
            _webApiClient = new Mock<IWebAPIClient>();
            _translatorService.Setup(x => x.GetGatewayRefundRequest(It.IsAny<RefundRequest>())).Returns(new GatewayRefundRequest());

            _gatewayRefundService = new GatewayRefundService(_webApiClient.Object, _translatorService.Object);
        }

        [Test]
        public void InitiateGatewayRefund_NullResponse_Test()
        {
            _webApiClient.Setup(x => x.HTTPPostRequest(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<GatewayRefundRequest>(), It.IsAny<string>())).Returns(() => null);
            
            var token = new Mock<GatewayToken>().Object;
            token.token = Guid.NewGuid().ToString("N");
            token.expiration_date = DateTime.Now.AddDays(1).ToString();

            var request = new Mock<RefundRequest>().Object;
            request.orderid = 123;
            request.type = "void";
            
            var actResult = _gatewayRefundService.InitiateGatewayRefund(request, token);
            Assert.IsNull(actResult);
        }


        [Test]
        public void InitiateGatewayRefund_StatusCodeIsNotCreated_Test()
        {
            var moqHttpWebResponse = new Mock<HttpWebResponse>();

            moqHttpWebResponse.Setup(_ => _.StatusCode).Returns(HttpStatusCode.BadRequest);

            _webApiClient.Setup(x => x.HTTPPostRequest(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<GatewayRefundRequest>(), It.IsAny<string>())).Returns(moqHttpWebResponse.Object);
            
            var token = new Mock<GatewayToken>().Object;
            token.token = Guid.NewGuid().ToString("N");
            token.expiration_date = DateTime.Now.AddDays(1).ToString();

            var request = new Mock<RefundRequest>().Object;
            request.orderid = 123;
            request.type = "void";
            
            var actResult = _gatewayRefundService.InitiateGatewayRefund(request, token);
            Assert.IsNull(actResult);
        }


        [Test]
        public void InitiateGatewayRefund_ThrowsJsonReaderException_Test()
        {
            var token = new Mock<GatewayToken>().Object;
            token.token = Guid.NewGuid().ToString("N");
            token.expiration_date = DateTime.Now.AddDays(1).ToString();

            var request = new Mock<RefundRequest>().Object;
            request.orderid = 123;
            request.type = "void";
            
            var resultContent = "response in a wrong format";

            var resultContentBytes = Encoding.ASCII.GetBytes(resultContent);

            var moqHttpWebResponse = new Mock<HttpWebResponse>();

            moqHttpWebResponse.Setup(_ => _.StatusCode).Returns(HttpStatusCode.OK);

            moqHttpWebResponse.Setup(_ => _.GetResponseStream()).Returns(new MemoryStream(resultContentBytes));

            _webApiClient.Setup(x => x.HTTPPostRequest(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<GatewayRefundRequest>(), It.IsAny<string>())).Returns(moqHttpWebResponse.Object);

            var ex = Assert.Throws<JsonReaderException>(() => _gatewayRefundService.InitiateGatewayRefund(request, token));

            Assert.IsTrue(ex.Message.Contains("Unexpected character encountered while parsing value"));
        }


        [Test]
        public void InitiateGatewayRefund_Test()
        {
            var token = new Mock<GatewayToken>().Object;
            token.token = Guid.NewGuid().ToString("N");
            token.expiration_date = DateTime.Now.AddDays(1).ToString();

            var request = new Mock<RefundRequest>().Object;
            request.orderid = 123;
            request.type = "void";
            
            var resultContent = "{\"amount\":{\"amount_in_cents\":630,\"currency\":\"USD\"},\"refund_id\":\"41a2O9Lv-7\",\"is_full_refund\":true}";

            var resultContentBytes = Encoding.ASCII.GetBytes(resultContent);

            var moqHttpWebResponse = new Mock<HttpWebResponse>();

            moqHttpWebResponse.Setup(_ => _.StatusCode).Returns(HttpStatusCode.OK);

            moqHttpWebResponse.Setup(_ => _.GetResponseStream()).Returns(new MemoryStream(resultContentBytes));

            _webApiClient.Setup(x => x.HTTPPostRequest(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<GatewayRefundRequest>(), It.IsAny<string>())).Returns(moqHttpWebResponse.Object);

            var actResult = _gatewayRefundService.InitiateGatewayRefund(request, token);

            Assert.IsNotNull(actResult);
            Assert.IsInstanceOf<GatewayRefundResponse>(actResult);
            StringAssert.Contains("41a2O9Lv-7", actResult.refund_id);

        }
        
    }
}
