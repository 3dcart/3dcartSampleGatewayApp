using System.Collections.Generic;

namespace _3dcartSampleGatewayApp.Models.Gateway
{    
    public class GatewayCheckoutRequest
    {
        public int amount_in_cents { get; set; }
        public string currency_code { get; set; }
        public string order_description { get; set; }
        public string order_reference_id { get; set; }
        public string checkout_cancel_url { get; set; }
        public string checkout_complete_url { get; set; }
        public GatewayCustomerDetails customer_details { get; set; }
        public GatewayBillingAddress billing_address { get; set; }
        public GatewayShippingAddress shipping_address { get; set; }
        public bool requires_shipping_info { get; set; }
        public List<GatewayItem> items { get; set; }
        public List<GatewayDiscount> discounts { get; set; }
        public GatewayTaxAmount tax_amount { get; set; }
        public GatewayShippingAmount shipping_amount { get; set; }
        public bool merchant_completes { get; set; }

    }
    
}