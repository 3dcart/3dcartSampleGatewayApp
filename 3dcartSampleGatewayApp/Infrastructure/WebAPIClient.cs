using _3dcartSampleGatewayApp.Interfaces;
using Newtonsoft.Json;
using System.Net;
using System.Net.Cache;
using System.Text;

namespace _3dcartSampleGatewayApp.Infrastructure
{
    public class WebAPIClient : IWebAPIClient
    {

        public HttpWebResponse HTTPGetRequest(string baseUrl, string action, object data, string accessToken)
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

#if DEBUG
            ServicePointManager.ServerCertificateValidationCallback += delegate (object sender, System.Security.Cryptography.X509Certificates.X509Certificate certificate,
                System.Security.Cryptography.X509Certificates.X509Chain chain,
                System.Net.Security.SslPolicyErrors sslPolicyErrors)
            {
                return true; // **** IGNORE ANY SSL ISSUES/VALIDATION
            };
#endif
            var request = (HttpWebRequest)WebRequest.Create(baseUrl + action);
            var noCachePolicy = new HttpRequestCachePolicy(HttpRequestCacheLevel.NoCacheNoStore);
            request.CachePolicy = noCachePolicy;

            request.Method = "GET";

            if (!string.IsNullOrEmpty(accessToken)) request.Headers.Add("Authorization", "Bearer " + accessToken);
            
            var response = (HttpWebResponse)request.GetResponse();
            return response;
        }



        public HttpWebResponse HTTPPostRequest(string baseUrl, string action, object content, string accessToken)
        {
            
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

#if DEBUG
            ServicePointManager.ServerCertificateValidationCallback += delegate (object sender, System.Security.Cryptography.X509Certificates.X509Certificate certificate,
                System.Security.Cryptography.X509Certificates.X509Chain chain,
                System.Net.Security.SslPolicyErrors sslPolicyErrors)
            {
                return true; // **** IGNORE ANY SSL ISSUES/VALIDATION
            };
#endif

            var postData = JsonConvert.SerializeObject(content);
            var encoding = new ASCIIEncoding();
            var postBytes = encoding.GetBytes(postData);

            var request = (HttpWebRequest) WebRequest.Create(baseUrl + action);
            var noCachePolicy = new HttpRequestCachePolicy(HttpRequestCacheLevel.NoCacheNoStore);
            request.CachePolicy = noCachePolicy;

            request.Method = "POST";
            request.ContentType = "application/json";
            request.Timeout = 60000;
            request.ReadWriteTimeout = 60000;
            request.ContentLength = postBytes.Length;
            request.UserAgent = "3dcart Shopping Cart";

            if (!string.IsNullOrEmpty(accessToken)) request.Headers.Add("Authorization", "Bearer " + accessToken);

            var newStream = request.GetRequestStream();
            newStream.Write(postBytes, 0, postBytes.Length);
            newStream.Close();

            var response = (HttpWebResponse) request.GetResponse();
            return response;

        }

    }
}