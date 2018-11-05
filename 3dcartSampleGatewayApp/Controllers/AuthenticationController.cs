using _3dcartSampleGatewayApp.Models.Cart;
using System;
using System.Collections.Specialized;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Mvc;

namespace _3dcartSampleGatewayApp.Controllers
{
    public class AuthenticationController : Controller
    {
        // GET: OAuthClient
        public ActionResult Authorize()
        {
            try
            {
                var urlHelper = new UrlHelper(Request.RequestContext);

                var apiBaseUrl = ConfigurationManager.AppSettings["3dCartRestApiUrl"];
#if DEBUG
                apiBaseUrl = "http://localhost:59120/";
#endif
                var server = $"{Request.Url.Scheme}://{Request.Url.Authority}{urlHelper.Content("~")}";
                var returnPath = $"{server}{"oauth/token"}";

                var clientId = ConfigurationManager.AppSettings["AppTestPublicKey"];

                var redirectUrl =
                    $"{apiBaseUrl}oauth/authorize?client_id={urlHelper.Encode(clientId)}&redirect_uri={urlHelper.Encode(returnPath)}&state=optionalstate&response_type=code";

                return Redirect(redirectUrl);
            }
            catch (Exception ex)
            {
                var error_code = "500";
                var error = ex.Message.ToString();
                return RedirectToAction("Result", "Home", new {error_code, error});
            }
        }

        public ActionResult Token(string code, string state, string error_code, string error)
        {
            try
            {
                // TODO: Add insert logic here              
                Token tokenResponse = null;

                if (!string.IsNullOrEmpty(error_code))
                    return RedirectToAction("Result", "Home", new {error_code, error});

                if (state != "optionalstate")
                {
                    error_code = "500";
                    error = "State is not valid";
                    return RedirectToAction("Result", "Home", new {error_code, error});
                }

                var clientId = ConfigurationManager.AppSettings["AppTestPublicKey"];
                var secretKey = ConfigurationManager.AppSettings["AppTestSecretKey"];

                var apiBaseUrl = ConfigurationManager.AppSettings["3dCartRestApiUrl"];
#if DEBUG
                apiBaseUrl = "http://localhost:59120/";
#endif
                tokenResponse = RequestAccessToken(code, apiBaseUrl, clientId, secretKey);

                if (!string.IsNullOrEmpty(tokenResponse.error_code))
                    return RedirectToAction("Result", "Home", new {tokenResponse.error_code, tokenResponse.error});

                return RedirectToAction("Result", "Home");
            }
            catch (Exception ex)
            {
                error_code = "500";
                error = ex.Message.ToString();
                return RedirectToAction("Result", "Home", new {error_code, error});
            }
        }

        private Token RequestAccessToken(string code, string apiBaseUrl, string clientId, string secretKey)
        {
            var client = new HttpClient();

            client.BaseAddress = new Uri(apiBaseUrl);

            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/x-www-form-urlencoded"));

            var data = new NameValueCollection();
            data.Add("client_id", clientId);
            data.Add("client_secret", secretKey);
            data.Add("code", code);
            data.Add("grant_type", "authorization_code");

            var response = client.PostAsync("oauth/token",
                    new FormUrlEncodedContent(data.AllKeys.ToDictionary(k => k, v => data[v])))
                .Result;

            return response.Content.ReadAsAsync<Token>().Result;
        }

    }
}