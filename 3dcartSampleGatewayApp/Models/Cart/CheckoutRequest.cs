using System.Collections.Generic;

namespace _3dcartSampleGatewayApp.Models.Cart
{
    public class CheckoutRequest
    {
        public int id { get; set; }
        public Status status { get; set; }
        public string type { get; set; }
        public int orderid { get; set; }
        public string invoice { get; set; }
        public string email { get; set; }
        public Billing billing { get; set; }
        public Shipping shipping { get; set; }
        public string currency { get; set; }
        public int amounttotal { get; set; }
        public int taxtotal { get; set; }
        public int shippingtotal { get; set; }
        public int discount { get; set; }
        public List<Item> items { get; set; }
        public string username { get; set; }
        public string password { get; set; }
        public string errorurl { get; set; }
        public string cancelurl { get; set; }
        public string notificationurl { get; set; }
        public string returnurl { get; set; }
        public string randomkey { get; set; }
        public string signature { get; set; }
    }
}