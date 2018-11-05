namespace _3dcartSampleGatewayApp.Models.Cart
{
    public class RefundRequest
    {
        public string type { get; set; }
        public int orderid { get; set; }
        public string invoice { get; set; }
        public string transactionid { get; set; }
        public string username { get; set; }
        public string password { get; set; }
        public string currency { get; set; }
        public int amounttotal { get; set; }
        public bool is_full_refund { get; set; }
        public string randomkey { get; set; }
        public string signature { get; set; }
    }
}