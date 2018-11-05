using System;
using System.Configuration;
using System.Net;
using NUnit.Framework;
using _3dcartSampleGatewayApp.Infrastructure;
using _3dcartSampleGatewayApp.Models.Gateway;

namespace _3dcartSampleGatewayAppTest
{
    [TestFixture()]
    class WebAPIClientTest
    {
        private WebAPIClient _webAPIClient;

        [SetUp]
        public void SetUp()
        {
            _webAPIClient = new WebAPIClient();
        }

        [Test]
        public void HTTPPostRequest_UriFormatException_Test()
        {
            var endpointurl = "any wrong url";
            var action = "checkouts";
            var request = new GatewayCheckoutRequest();
            var ex = Assert.Throws<UriFormatException>(() => _webAPIClient.HTTPPostRequest(endpointurl, action, request, ""));
            Assert.IsTrue(ex.Message.Contains("Invalid URI: The format of the URI could not be determined"));
        }

    }
}
