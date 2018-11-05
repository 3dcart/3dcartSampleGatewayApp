using System.Web.Mvc;
using System.Web.Routing;

namespace _3dcartSampleGatewayApp
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                name: "OAuthAuthorize",
                url: "oauth/code",
                defaults: new { controller = "Authentication", action = "Authorize" }
            );

            routes.MapRoute(
                name: "OAuthToken",
                url: "oauth/token",
                defaults: new { controller = "Authentication", action = "Token" }
            );

            routes.MapRoute(
               name: "Checkout",
               url: "checkout",
               defaults: new { controller = "Checkout", action = "InitiateCheckout" }
            );

            routes.MapRoute(
               name: "CheckoutResponse",
               url: "checkoutresponse",
               defaults: new { controller = "Checkout", action = "CompleteCheckout" }
            );

            routes.MapRoute(
               name: "Webhook",
               url: "webhook",
               defaults: new { controller = "Checkout", action = "Webhook" }
            );

            routes.MapRoute(
                name: "Refund",
                url: "refund",
                defaults: new { controller = "Refund", action = "InitiateRefund" }
            );

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
            );

        }
    }
}
