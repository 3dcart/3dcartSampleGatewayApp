namespace _3dcartSampleGatewayApp.Models.Gateway
{
    public class GatewayWebhook
    {
        public string time { get; set; }
        public string uuid { get; set; }
        public string type { get; set; }
        public string @event { get; set; }
        public string object_uuid { get; set; }
        public string refund_id { get; set; }
        public GatewayAmount refund_amount { get; set; }
    }
}