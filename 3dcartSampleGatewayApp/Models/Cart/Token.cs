namespace _3dcartSampleGatewayApp.Models.Cart
{
    public class Token
    {
        public string access_token { get; set; }
        public int user_id { get; set; }
        public string token_type { get; set; }
        public string expires { get; set; }
        public string refresh_token { get; set; }
        public string scope { get; set; }
        public string error_code { get; set; }
        public string error { get; set; }

    }
}