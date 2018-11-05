using System;
using System.Configuration;
using System.Net;
using System.Web;
using Moq;
using NUnit.Framework;
using _3dcartSampleGatewayApp.Interfaces;
using _3dcartSampleGatewayApp.Models.Gateway;
using _3dcartSampleGatewayApp.Services;

namespace _3dcartSampleGatewayAppTest.Services
{
    [TestFixture()]
    class WebhookServiceTest
    {

        private Mock<IWebAPIClient> _webApiClient;
        private Mock<IHttpContextService> _contextService;
        private GatewayConfigurationService _webhookService;

        [SetUp]
        public void SetUp()
        {
            _contextService = new Mock<IHttpContextService>();
            _webApiClient = new Mock<IWebAPIClient>();
            _webhookService = new GatewayConfigurationService(_webApiClient.Object, _contextService.Object);
        }


        [Test]
        public void SetWebhook_StatusCodeIsNotCreated_Test()
        {
            var moqHttpWebResponse = new Mock<HttpWebResponse>();

            moqHttpWebResponse.Setup(_ => _.StatusCode).Returns(HttpStatusCode.BadRequest);

            _webApiClient.Setup(x => x.HTTPPostRequest(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<GatewayConfigRequest>(), It.IsAny<string>())).Returns(moqHttpWebResponse.Object);
            
            var token = new Mock<GatewayToken>().Object;
            token.token = Guid.NewGuid().ToString("N");
            token.expiration_date = DateTime.Now.AddDays(1).ToString();
            
            var baseUrl = ConfigurationManager.AppSettings["BaseURLWebAPIService"];
            
            var requestBase = new Mock<HttpRequestBase>();
            var Uri = new Uri("http://localhost:49798/refund");
            requestBase.Setup(_ => _.Url).Returns(Uri);
            _contextService.Setup(x => x.Request).Returns(requestBase.Object);
            
            var actResult = _webhookService.SetWebhook(baseUrl, token);
            Assert.IsFalse(actResult);
        }

        [Test]
        public void SetWebhook_Test()
        {
            var moqHttpWebResponse = new Mock<HttpWebResponse>();

            moqHttpWebResponse.Setup(_ => _.StatusCode).Returns(HttpStatusCode.OK);

            _webApiClient.Setup(x => x.HTTPPostRequest(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<GatewayConfigRequest>(), It.IsAny<string>())).Returns(moqHttpWebResponse.Object);
            
            var token = new Mock<GatewayToken>().Object;
            token.token = Guid.NewGuid().ToString("N");
            token.expiration_date = DateTime.Now.AddDays(1).ToString();
            
            var baseUrl = ConfigurationManager.AppSettings["BaseURLWebAPIService"];

            var requestBase = new Mock<HttpRequestBase>();
            var Uri = new Uri("http://localhost:49798/refund");
            requestBase.Setup(_ => _.Url).Returns(Uri);
            _contextService.Setup(x => x.Request).Returns(requestBase.Object);

            var actResult = _webhookService.SetWebhook(baseUrl, token);
            Assert.IsTrue(actResult);
        }


    }
}
