namespace _3dcartSampleGatewayApp.Models.Gateway
{
    public class GatewayItem
    {
        public string name { get; set; }
        public string sku { get; set; }
        public int quantity { get; set; }
        public GatewayPrice price { get; set; }
    }
}