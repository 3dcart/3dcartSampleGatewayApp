namespace _3dcartSampleGatewayApp.Models.Gateway
{
    public class GatewayRefundRequest
    {
        public string refund_id { get; set; }
        public GatewayAmount amount { get; set; }
        public string refund_reason { get; set; }
        public bool is_full_refund { get; set; }        

    }
}