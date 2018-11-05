namespace _3dcartSampleGatewayApp.Models.Cart
{
    public class RefundResponse
    {
        public int approved { get; set; }
        public string transactionid { get; set; }
        public string errorcode { get; set; }
        public string errormessage { get; set; }
        public string randomkey { get; set; }
        public string signature { get; set; }
    }
}