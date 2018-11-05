using System.Collections.Generic;
using System.IO;
using System.Text;
using Moq;
using NUnit.Framework;
using _3dcartSampleGatewayApp.Interfaces;
using _3dcartSampleGatewayApp.Models.Cart;
using _3dcartSampleGatewayApp.Models.Gateway;
using _3dcartSampleGatewayApp.Services;
using System.Net;

namespace _3dcartSampleGatewayAppTest.Services
{
    [TestFixture()]
    class CompleteOrderServiceTest
    {
        private Mock<IWebAPIClient> _webApiClient;
        private Mock<IRepository> _repository;
        private CompleteOrderService _completeOrderService;

        [SetUp]
        public void SetUp()
        {
            _webApiClient = new Mock<IWebAPIClient>();
            _repository = new Mock<IRepository>();
            _completeOrderService = new CompleteOrderService(_webApiClient.Object, _repository.Object);
        }
        
        [Test]
        public void CompleteOrder_ApiCallReturnNullObject_Test()
        {
            _repository.Setup(x => x.UpdateCheckoutRequestStatus(It.IsAny<int>(), It.IsAny<Status>(),It.IsAny<int>())).Returns(true);
            _webApiClient.Setup(x => x.HTTPPostRequest(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<object>(),It.IsAny<string>())).Returns(() => null);
            var actResult = _completeOrderService.CompleteOrder(1, "", "", new Mock<CheckoutRequest>().Object, new Mock<GatewayOrderDetails>().Object);
            Assert.IsNotNull(actResult);
            Assert.IsInstanceOf<bool>(actResult);
            Assert.AreEqual(false, actResult);
        }

        [Test]
        public void CompleteOrder_ApiCallReturnWrongStatusCode_Test()
        {
            _repository.Setup(x => x.UpdateCheckoutRequestStatus(It.IsAny<int>(), It.IsAny<Status>(), It.IsAny<int>())).Returns(true);
           
            var moqHttpWebResponse = new Mock<HttpWebResponse>();

            moqHttpWebResponse.Setup(_ => _.StatusCode).Returns(HttpStatusCode.BadRequest);

            _webApiClient.Setup(x => x.HTTPPostRequest(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<object>(), It.IsAny<string>())).Returns(moqHttpWebResponse.Object);
            
            var actResult = _completeOrderService.CompleteOrder(1, "", "", new Mock<CheckoutRequest>().Object, new Mock<GatewayOrderDetails>().Object);

            Assert.IsNotNull(actResult);
            Assert.IsInstanceOf<bool>(actResult);
            Assert.AreEqual(false, actResult);
        }
        
        [Test]
        public void CompleteOrder_ThrowsJsonReaderException_Test()
        {

            _repository.Setup(x => x.UpdateCheckoutRequestStatus(It.IsAny<int>(), It.IsAny<Status>(), It.IsAny<int>())).Returns(true);

            var resultContent = "response in a wrong format";

            var resultContentBytes = Encoding.ASCII.GetBytes(resultContent);

            var moqHttpWebResponse = new Mock<HttpWebResponse>();

            moqHttpWebResponse.Setup(_ => _.StatusCode).Returns(HttpStatusCode.OK);

            moqHttpWebResponse.Setup(_ => _.GetResponseStream()).Returns(new MemoryStream(resultContentBytes));

            _webApiClient.Setup(x => x.HTTPPostRequest(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<object>(), It.IsAny<string>())).Returns(moqHttpWebResponse.Object);

            var actResult = _completeOrderService.CompleteOrder(1, "", "", new Mock<CheckoutRequest>().Object, new Mock<GatewayOrderDetails>().Object);

            Assert.IsNotNull(actResult);
            Assert.IsInstanceOf<bool>(actResult);
            Assert.AreEqual(false, actResult);
        }
        
        [Test]
        public void CompleteOrder_Ok_Test()
        {

            _repository.Setup(x => x.UpdateCheckoutRequestStatus(It.IsAny<int>(), It.IsAny<Status>(), It.IsAny<int>())).Returns(true);

            var resultContent = "{\"processed\":1, \"errorcode\":\"abc123\", \"errormessage\":\"error message\"}";

            var resultContentBytes = Encoding.ASCII.GetBytes(resultContent);

            var moqHttpWebResponse = new Mock<HttpWebResponse>();

            moqHttpWebResponse.Setup(_ => _.StatusCode).Returns(HttpStatusCode.OK);

            moqHttpWebResponse.Setup(_ => _.GetResponseStream()).Returns(new MemoryStream(resultContentBytes));

            _webApiClient.Setup(x => x.HTTPPostRequest(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<object>(), It.IsAny<string>())).Returns(moqHttpWebResponse.Object);

            var actResult = _completeOrderService.CompleteOrder(1, "", "", new Mock<CheckoutRequest>().Object, new Mock<GatewayOrderDetails>().Object);

            Assert.IsNotNull(actResult);
            Assert.IsInstanceOf<bool>(actResult);
            Assert.AreEqual(true, actResult);
        }


        //-------------------------------------------------------------------------------------
        

        [Test]
        public void GetCurrentRequest_ReturnNullObject_Test()
        {
            Mock<List<CheckoutRequest>> request = new Mock<List<CheckoutRequest>>();
            _repository.Setup(x => x.GetCheckoutRequests(It.IsAny<int>(), It.IsAny<int>())).Returns(request.Object);

            var actResult = _completeOrderService.GetCurrentRequest(5);
            Assert.IsNull(actResult);
        }

        [Test]
        public void GetCurrentRequest_ReturnWrongResult_Test()
        {
            var request = new Mock<CheckoutRequest>();
            request.Object.id = 5;

            Mock<List<CheckoutRequest>> requests = new Mock<List<CheckoutRequest>>();
            requests.Object.Add(request.Object);
           
            _repository.Setup(x => x.GetCheckoutRequests(It.IsAny<int>(), It.IsAny<int>())).Returns(requests.Object);

            var actResult = _completeOrderService.GetCurrentRequest(3);
            Assert.IsNull(actResult);
        }

        [Test]
        public void GetCurrentRequest_OK_Test()
        {
            var request = new Mock<CheckoutRequest>();
            request.Object.id = 3;

            Mock<List<CheckoutRequest>> requests = new Mock<List<CheckoutRequest>>();
            requests.Object.Add(request.Object);

            _repository.Setup(x => x.GetCheckoutRequests(It.IsAny<int>(), It.IsAny<int>())).Returns(requests.Object);

            var actResult = _completeOrderService.GetCurrentRequest(3);
           
            Assert.IsNotNull(actResult);
            Assert.IsInstanceOf<CheckoutRequest>(actResult);
            var response = actResult as CheckoutRequest;
            Assert.IsNotNull(response);
            Assert.AreEqual(3, response.id);
        }
        
    }
}
