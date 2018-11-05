using System.Web.Mvc;
using NUnit.Framework;
using _3dcartSampleGatewayApp.Controllers;

namespace _3dcartSampleGatewayAppTest.Controllers
{
    [TestFixture]
    public class HomeControllerTest
    {
        private HomeController _homeController;

        [SetUp]
        public void SetUp()
        {
            _homeController = new HomeController();
        }

        [Test]
        public void HomeControllerIndexTest()
        {
            var actResult = _homeController.Index() as ViewResult;
            Assert.IsNotNull(actResult);
            Assert.IsInstanceOf<ViewResult>(actResult);
            Assert.That(actResult.ViewName, Is.EqualTo("Index"));
        }

        [Test]
        public void HomeControllerInstallAppTest()
        {
            var actResult = _homeController.InstallApp() as RedirectToRouteResult;
            Assert.IsNotNull(actResult);
            Assert.IsInstanceOf<RedirectToRouteResult>(actResult);
            Assert.That(actResult.RouteName, Is.EqualTo("OAuthAuthorize"));
        }
        
        [Test]
        public void HomeControllerResultTest()
        {
            ViewResult actResult = null;

            var errorCode = "100";
            var errorMessage = "Error message";

            actResult = _homeController.Result(errorCode, errorMessage) as ViewResult;

            Assert.IsNotNull(actResult);
            Assert.IsInstanceOf<ViewResult>(actResult);
            Assert.That(actResult.ViewBag.Error, Is.EqualTo(true));
            Assert.That(actResult.ViewBag.ErrorCode, Is.EqualTo(errorCode));
            Assert.That(actResult.ViewBag.ErrorMessage, Is.EqualTo(errorMessage));
            Assert.That(actResult.ViewName, Is.EqualTo("Index"));

            errorCode = "";
            errorMessage = "";

            actResult = _homeController.Result(errorCode, errorMessage) as ViewResult;

            Assert.IsNotNull(actResult);
            Assert.IsInstanceOf<ViewResult>(actResult);
            Assert.That(actResult.ViewBag.Error, Is.EqualTo(false));
            Assert.That(actResult.ViewBag.ErrorCode, Is.EqualTo(errorCode));
            Assert.That(actResult.ViewBag.ErrorMessage, Is.EqualTo(errorMessage));
            Assert.That(actResult.ViewName, Is.EqualTo("Index"));

            errorCode = "";
            errorMessage = "Some error message";

            actResult = _homeController.Result(errorCode, errorMessage) as ViewResult;
            Assert.IsNotNull(actResult);
            Assert.IsInstanceOf<ViewResult>(actResult);
            Assert.That(actResult.ViewBag.Error, Is.EqualTo(true));
            Assert.That(actResult.ViewBag.ErrorCode, Is.EqualTo(errorCode));
            Assert.That(actResult.ViewBag.ErrorMessage, Is.EqualTo(errorMessage));
            Assert.That(actResult.ViewName, Is.EqualTo("Index"));
        }
        
    }
}
