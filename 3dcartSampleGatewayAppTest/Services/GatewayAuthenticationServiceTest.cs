using System;
using System.IO;
using System.Net;
using System.Text;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;
using _3dcartSampleGatewayApp.Interfaces;
using _3dcartSampleGatewayApp.Models.Gateway;
using _3dcartSampleGatewayApp.Services;

namespace _3dcartSampleGatewayAppTest.Services
{
    [TestFixture()]
    class GatewayAuthenticationServiceTest
    {
        private Mock<IGatewayConfigurationService> _gatewayConfigurationService;
        private Mock<IWebAPIClient> _webApiClient;
        private Mock<ICacheTokenHandlerService> _cacheTokenHandlerService;
        private GatewayAuthenticationService _authenticationService;

        [SetUp]
        public void SetUp()
        {
            _gatewayConfigurationService = new Mock<IGatewayConfigurationService>();
            _webApiClient = new Mock<IWebAPIClient>();
            _cacheTokenHandlerService = new Mock<ICacheTokenHandlerService>();
            _authenticationService = new GatewayAuthenticationService(_webApiClient.Object, _gatewayConfigurationService.Object, _cacheTokenHandlerService.Object);
        }

        [Test]
        public void GetGatewayToken_ValidTokenInMemory_Test()
        {
            _cacheTokenHandlerService.Setup(x => x.GetTokenFromCache(It.IsAny<string>())).Returns(()=> new GatewayToken(){ token = "abc1234456", expiration_date = DateTime.Now.AddDays(2).ToString()});
            var actResult = _authenticationService.GetGatewayToken("", "");
            Assert.IsNotNull(actResult);
            Assert.IsInstanceOf<GatewayToken>(actResult);
            Assert.AreEqual(actResult.token, "abc1234456");
        }


        [Test]
        public void GetGatewayToken_NullResponse_Test()
        {
            _cacheTokenHandlerService.Setup(x => x.GetTokenFromCache(It.IsAny<string>())).Returns(() => null);
            _webApiClient.Setup(x => x.HTTPPostRequest(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<GatewayAuthRequest>(), It.IsAny<string>())).Returns(() => null);

            var actResult = _authenticationService.GetGatewayToken("", "");
            Assert.IsNull(actResult);
        }


        [Test]
        public void GetGatewayToken_StatusCodeIsNotCreated_Test()
        {
            _cacheTokenHandlerService.Setup(x => x.GetTokenFromCache(It.IsAny<string>())).Returns(() => null);
            var moqHttpWebResponse = new Mock<HttpWebResponse>();
            moqHttpWebResponse.Setup(_ => _.StatusCode).Returns(HttpStatusCode.BadRequest);
            _webApiClient.Setup(x => x.HTTPPostRequest(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<GatewayAuthRequest>(), It.IsAny<string>())).Returns(moqHttpWebResponse.Object);
            
            var actResult = _authenticationService.GetGatewayToken("", "");
            Assert.IsNull(actResult);
        }


        [Test]
        public void GetGatewayToken_ThrowsJsonReaderException_Test()
        {
            _cacheTokenHandlerService.Setup(x => x.GetTokenFromCache(It.IsAny<string>())).Returns(() => null);
            var resultContent = "response in a wrong format";
            var resultContentBytes = Encoding.ASCII.GetBytes(resultContent);
            var moqHttpWebResponse = new Mock<HttpWebResponse>();
            moqHttpWebResponse.Setup(_ => _.StatusCode).Returns(HttpStatusCode.Created);
            moqHttpWebResponse.Setup(_ => _.GetResponseStream()).Returns(new MemoryStream(resultContentBytes));
            _webApiClient.Setup(x => x.HTTPPostRequest(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<GatewayAuthRequest>(), It.IsAny<string>())).Returns(moqHttpWebResponse.Object);
            var ex = Assert.Throws<JsonReaderException>(() => _authenticationService.GetGatewayToken("",""));

            Assert.IsTrue(ex.Message.Contains("Unexpected character encountered while parsing value"));
        }


        [Test]
        public void GetGatewayToken_Test()
        {
            _cacheTokenHandlerService.Setup(x => x.GetTokenFromCache(It.IsAny<string>())).Returns(() => null);
            var resultContent = "{\"token\":\"001a05b3-d653-4142-a744-cd43a9f5df44\",\"expiration_date\":\"2018-10-25T15:02:06.998263819Z\"}";
            var resultContentBytes = Encoding.ASCII.GetBytes(resultContent);
            var moqHttpWebResponse = new Mock<HttpWebResponse>();
            moqHttpWebResponse.Setup(_ => _.StatusCode).Returns(HttpStatusCode.Created);
            moqHttpWebResponse.Setup(_ => _.GetResponseStream()).Returns(new MemoryStream(resultContentBytes));
            _webApiClient.Setup(x => x.HTTPPostRequest(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<GatewayAuthRequest>(), It.IsAny<string>())).Returns(moqHttpWebResponse.Object);
            _cacheTokenHandlerService.Setup(x => x.CachingValidToken(It.IsAny<string>(), It.IsAny<GatewayToken>())).Returns(true);
            _gatewayConfigurationService.Setup(x => x.SetWebhook(It.IsAny<string>(), It.IsAny<GatewayToken>())).Returns(true);
            var actResult = _authenticationService.GetGatewayToken("", "");

            Assert.IsNotNull(actResult);
            Assert.IsInstanceOf<GatewayToken>(actResult);
            Assert.AreEqual(actResult.token, "001a05b3-d653-4142-a744-cd43a9f5df44");
            
            _cacheTokenHandlerService.Verify(x=>x.GetTokenFromCache(It.IsAny<string>()), Times.Once);
            _webApiClient.Verify(x=>x.HTTPPostRequest(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<GatewayAuthRequest>(), It.IsAny<string>()), Times.Once);
            _cacheTokenHandlerService.Verify(x=>x.CachingValidToken(It.IsAny<string>(), It.IsAny<GatewayToken>()),Times.Once);
            _gatewayConfigurationService.Verify(x=>x.SetWebhook(It.IsAny<string>(), It.IsAny<GatewayToken>()),Times.Once);
            
        }
        
    }
}
