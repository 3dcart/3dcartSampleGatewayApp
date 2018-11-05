using System.Collections.Generic;
using System.Linq;
using _3dcartSampleGatewayApp.Interfaces;
using _3dcartSampleGatewayApp.Models.Cart;
using _3dcartSampleGatewayApp.Models.Gateway;

namespace _3dcartSampleGatewayApp.Services
{
    public class TranslatorService : ITranslatorService
    {
        public GatewayRefundRequest GetGatewayRefundRequest(RefundRequest request)
        {
            var gatewayRefundRequest = new GatewayRefundRequest()
            {
                amount = new GatewayAmount() { amount_in_cents = request.amounttotal, currency = request.currency },
                refund_id = request.transactionid,
                is_full_refund = request.is_full_refund
            };
            return gatewayRefundRequest;
        }

        public GatewayCheckoutRequest GetGatewayCheckoutRequest(CheckoutRequest request)
        {
            var query = from item in request.items
                        select new GatewayItem()
                        {
                            name = item.itemname,
                            sku = item.itemid,
                            price = new GatewayPrice() { amount_in_cents = item.unitprice, currency = request.currency },
                            quantity = item.quantity
                        };
            var items = query.ToList<GatewayItem>();
            
            var discounts = new List<GatewayDiscount>();
            discounts.Add(new GatewayDiscount()
            {
                name = "discount",
                amount = new GatewayAmount() { amount_in_cents = request.discount, currency = request.currency }
            });
            
            var gatewayCheckoutRequest = new GatewayCheckoutRequest()
            {
                amount_in_cents = request.amounttotal,
                currency_code = request.currency,
                order_description = request.invoice,
                order_reference_id = "",
                checkout_cancel_url = request.cancelurl,
                checkout_complete_url = "",
                customer_details = new GatewayCustomerDetails()
                {
                    first_name = request.billing.firstname,
                    last_name = request.billing.lastname,
                    email = request.email,
                    phone = request.billing.phone
                },
                billing_address = new GatewayBillingAddress()
                {
                    name = $"{request.billing.firstname} {request.billing.lastname}",
                    street = request.billing.address,
                    street2 = request.billing.address2,
                    city = request.billing.city,
                    state = request.billing.state,
                    postal_code = request.billing.zip,
                    country_code = request.billing.country,
                    phone_number = request.billing.phone
                },
                shipping_address = new GatewayShippingAddress()
                {
                    name = $"{request.shipping.firstname} {request.shipping.lastname}",
                    street = request.shipping.address,
                    street2 = request.shipping.address2,
                    city = request.shipping.city,
                    state = request.shipping.state,
                    postal_code = request.shipping.zip,
                    country_code = request.shipping.country,
                    phone_number = request.shipping.phone
                },
                requires_shipping_info = false,
                items = items,
                discounts = discounts,
                tax_amount = new GatewayTaxAmount()
                {
                    amount_in_cents = request.taxtotal,
                    currency = request.currency
                },
                shipping_amount = new GatewayShippingAmount()
                {
                    amount_in_cents = request.shippingtotal,
                    currency = request.currency
                },
                merchant_completes = false
            };

            return gatewayCheckoutRequest;
        }
        

    }
}