using System.Collections.Specialized;
using System.Web;
using _3dcartSampleGatewayApp.Interfaces;

namespace _3dcartSampleGatewayApp.Services
{
    public class HttpContextService : IHttpContextService
    {
        public HttpContextBase Context
        {
            get { return new HttpContextWrapper(HttpContext.Current); }
        }

        public HttpRequestBase Request
        {
            get { return Context.Request; }
        }

        public HttpResponseBase Response
        {
            get { return Context.Response; }
        }

        public NameValueCollection FormOrQuerystring
        {
            get
            {
                if (Request.RequestType == "POST")
                {
                    return Request.Form;
                }

                return Request.QueryString;
            }
        }
    }
}