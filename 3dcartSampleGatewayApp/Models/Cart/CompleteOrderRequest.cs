namespace _3dcartSampleGatewayApp.Models.Cart
{
    public class CompleteOrderRequest
    {
        public int orderid { get; set; }
        public string invoice { get; set; }
        public int amount { get; set; }
        public int approved { get; set; }
        public string transactionid { get; set; }
        public string errorcode { get; set; }
        public string errormessage { get; set; }
        public string randomkey { get; set; }
        public string signature { get; set; }
    }
}