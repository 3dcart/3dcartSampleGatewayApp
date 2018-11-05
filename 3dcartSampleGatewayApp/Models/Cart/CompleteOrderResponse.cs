namespace _3dcartSampleGatewayApp.Models.Cart
{
    public class CompleteOrderResponse
    {
        public int processed { get; set; }
        public string errorcode { get; set; }
        public string errormessage { get; set; }
    }
}