namespace _3dcartSampleGatewayApp.Models.Cart
{
    public class CheckoutResponse
    {
        public string redirectmethod { get; set; }
        public string paymenttoken { get; set; }
        public string redirecturl { get; set; }
        public string errorcode { get; set; }
        public string errormessage { get; set; }
        public string randomkey { get; set; }
        public string signature { get; set; }
    }
}