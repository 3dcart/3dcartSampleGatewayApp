using System;
using System.IO;
using System.Net;
using System.Text;
using System.Web;
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
    class GatewayCheckoutServiceTest
    {
        private Mock<ITranslatorService> _translatorService;
        private Mock<IWebAPIClient> _webApiClient;
        private Mock<IHttpContextService> _contextService;
        private Mock<IRepository> _repository;
        
        private GatewayCheckoutService _checkoutService;

        [SetUp]
        public void SetUp()
        {
            _translatorService = new Mock<ITranslatorService>();
            _webApiClient = new Mock<IWebAPIClient>();
            _contextService = new Mock<IHttpContextService>();
            _repository = new Mock<IRepository>();

            _translatorService.Setup(x => x.GetGatewayCheckoutRequest(It.IsAny<CheckoutRequest>())).Returns(new GatewayCheckoutRequest());

            var requestBase = new Mock<HttpRequestBase>();
            var Uri = new Uri("http://localhost:49798");
            requestBase.Setup(_ => _.Url).Returns(Uri);
            _contextService.Setup(x => x.Request).Returns(requestBase.Object);

            _repository.Setup(x => x.SaveCheckoutRequest(It.IsAny<CheckoutRequest>(), It.IsAny<int>())).Returns(4);

            _repository.Setup(y => y.UpdateCheckoutRequestStatus(It.IsAny<int>(), It.IsAny<Status>(), It.IsAny<int>())).Returns(true);


            _checkoutService = new GatewayCheckoutService(_webApiClient.Object, _translatorService.Object, _contextService.Object, _repository.Object);
        }
        
        [Test]
        public void InitiateGatewayChechout_NullResponse_Test()
        {
            _webApiClient.Setup(x => x.HTTPPostRequest(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<GatewayCheckoutRequest>(), It.IsAny<string>())).Returns(() => null);

            var token = new Mock<GatewayToken>().Object;
            token.token = Guid.NewGuid().ToString("N");
            token.expiration_date = DateTime.Now.AddDays(1).ToString();

            var request = new Mock<CheckoutRequest>().Object;
            request.id = 4;

            var actResult = _checkoutService.InitiateGatewayChechout(request, token);
            Assert.IsNull(actResult);
        }


        [Test]
        public void InitiateGatewayChechout_SaveRequestFailed_Test()
        {
            _webApiClient.Setup(x => x.HTTPPostRequest(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<GatewayCheckoutRequest>(), It.IsAny<string>())).Returns(() => null);
            
            var token = new Mock<GatewayToken>().Object;
            token.token = Guid.NewGuid().ToString("N");
            token.expiration_date = DateTime.Now.AddDays(1).ToString();

            var request = new Mock<CheckoutRequest>().Object;
            request.id = 4;
            
            _repository.Setup(x => x.SaveCheckoutRequest(It.IsAny<CheckoutRequest>(), It.IsAny<int>())).Returns(0);

            var actResult = _checkoutService.InitiateGatewayChechout(request, token);
            Assert.IsNull(actResult);
        }


        [Test]
        public void InitiateGatewayChechout_StatusCodeIsNotCreated_Test()
        {
            var moqHttpWebResponse = new Mock<HttpWebResponse>();

            moqHttpWebResponse.Setup(_ => _.StatusCode).Returns(HttpStatusCode.BadRequest);

            _webApiClient.Setup(x => x.HTTPPostRequest(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<GatewayCheckoutRequest>(), It.IsAny<string>())).Returns(moqHttpWebResponse.Object);

            var token = new Mock<GatewayToken>().Object;
            token.token = Guid.NewGuid().ToString("N");
            token.expiration_date = DateTime.Now.AddDays(1).ToString();

            var request = new Mock<CheckoutRequest>().Object;
            request.id = 4;

            var actResult = _checkoutService.InitiateGatewayChechout(request, token);

            Assert.IsNull(actResult);
        }

        
        [Test]
        public void InitiateGatewayChechout_ThrowsJsonReaderException_Test()
        {
            var token = new Mock<GatewayToken>().Object;
            token.token = Guid.NewGuid().ToString("N");
            token.expiration_date = DateTime.Now.AddDays(1).ToString();

            var request = new Mock<CheckoutRequest>().Object;
            request.id = 4;

            var resultContent = "response in a wrong format";

            var resultContentBytes = Encoding.ASCII.GetBytes(resultContent);

            var moqHttpWebResponse = new Mock<HttpWebResponse>();

            moqHttpWebResponse.Setup(_ => _.StatusCode).Returns(HttpStatusCode.Created);

            moqHttpWebResponse.Setup(_ => _.GetResponseStream()).Returns(new MemoryStream(resultContentBytes));

            _webApiClient.Setup(x => x.HTTPPostRequest(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<GatewayCheckoutRequest>(), It.IsAny<string>())).Returns(moqHttpWebResponse.Object);

            var ex = Assert.Throws<JsonReaderException>(() => _checkoutService.InitiateGatewayChechout(request, token));

            Assert.IsTrue(ex.Message.Contains("Unexpected character encountered while parsing value"));
        }
        

        [Test]
        public void InitiateGatewayChechout_Test()
        {
            var token = new Mock<GatewayToken>().Object;
            token.token = Guid.NewGuid().ToString("N");
            token.expiration_date = DateTime.Now.AddDays(1).ToString();

            var request = new Mock<CheckoutRequest>().Object;
            request.id = 4;

            var resultContent = "{\"checkout_url\":\"https://sandbox.checkout.com?id=a9a24a14-0e2d-4b46-bab5-c9b1f06f48f5\"}";

            var resultContentBytes = Encoding.ASCII.GetBytes(resultContent);

            var moqHttpWebResponse = new Mock<HttpWebResponse>();

            moqHttpWebResponse.Setup(_ => _.StatusCode).Returns(HttpStatusCode.Created);

            moqHttpWebResponse.Setup(_ => _.GetResponseStream()).Returns(new MemoryStream(resultContentBytes));

            _webApiClient.Setup(x => x.HTTPPostRequest(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<GatewayCheckoutRequest>(), It.IsAny<string>())).Returns(moqHttpWebResponse.Object);
            
            var actResult = _checkoutService.InitiateGatewayChechout(request, token);
            
            Assert.IsNotNull(actResult);
            Assert.IsInstanceOf<GatewayCheckoutResponse>(actResult);
            StringAssert.Contains("https://sandbox.checkout.com", actResult.checkout_url);
            
        }


    }
}
