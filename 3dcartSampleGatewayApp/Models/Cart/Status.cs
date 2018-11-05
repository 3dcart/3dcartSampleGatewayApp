namespace _3dcartSampleGatewayApp.Models.Cart
{
    public enum Status : byte
    {
        Pending = 0,
        Running,
        Completed,
        Failed
    }
}