using System.Net;
namespace _3dcartSampleGatewayApp.Interfaces
{
    public interface IWebAPIClient
    {
        HttpWebResponse HTTPGetRequest(string baseUrl, string action, object data, string accessToken);
        HttpWebResponse HTTPPostRequest(string baseUrl, string action, object content, string accessToken);
    }
}
