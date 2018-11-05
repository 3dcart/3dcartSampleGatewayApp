namespace _3dcartSampleGatewayApp.Models.Gateway
{
    public class GatewayOrderDetails
    {
        public string description { get; set; }
        public int amount_in_cents { get; set; }
        public int usd_amount_in_cents { get; set; }
        public string currency_code { get; set; }
        public string reference_id { get; set; }
        public GatewayCustomerDetails customer { get; set; }
        public GatewayShippingAddress shipping_address { get; set; }
    }
}