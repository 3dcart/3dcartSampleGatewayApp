using System.Web.Mvc;

namespace _3dcartSampleGatewayApp.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View("Index");
        }
        public ActionResult InstallApp()
        {
            return RedirectToRoute("OAuthAuthorize");
        }
        public ActionResult Result(string error_code, string error)
        {
            ViewBag.Error = false;
            ViewBag.ErrorCode = "";
            ViewBag.ErrorMessage = "";

            if (!string.IsNullOrEmpty(error_code) || !string.IsNullOrEmpty(error))
            {
                ViewBag.Error = true;
                ViewBag.ErrorCode = error_code;
                ViewBag.ErrorMessage = error;
            }

            return View("Index");
        }
    }
}