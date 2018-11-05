using _3dcartSampleGatewayApp.Infrastructure;
using _3dcartSampleGatewayApp.Interfaces;
using _3dcartSampleGatewayApp.Services;
using System.Web.Mvc;
using Unity;
using Unity.Mvc5;

namespace _3dcartSampleGatewayApp
{
    public static class UnityConfig
    {
        public static void RegisterComponents()
        {
			var container = new UnityContainer();

            // register all your components with the container here
            // it is NOT necessary to register your controllers

            // e.g. container.RegisterType<ITestService, TestService>();

            container.RegisterType<IWebAPIClient, WebAPIClient>();
            container.RegisterType<IGatewayConfigurationService, GatewayConfigurationService>();
            container.RegisterType<IGatewayAuthenticationService, GatewayAuthenticationService>();
            container.RegisterType<IHttpContextService, HttpContextService>();
            container.RegisterType<IGatewayCheckoutService, GatewayCheckoutService>();
            container.RegisterType<IGatewayRefundService, GatewayRefundService>();
            container.RegisterType<ITranslatorService, TranslatorService>();
            container.RegisterType<ICacheTokenHandlerService, CacheTokenHandlerService>();
            container.RegisterType<ICompleteOrderService, CompleteOrderService>();
            container.RegisterType<IRepository, Repository>();

            DependencyResolver.SetResolver(new UnityDependencyResolver(container));
        }
    }
}