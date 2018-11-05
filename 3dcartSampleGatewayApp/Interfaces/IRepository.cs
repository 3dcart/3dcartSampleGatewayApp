using System.Collections.Generic;
using _3dcartSampleGatewayApp.Models.Cart;

namespace _3dcartSampleGatewayApp.Interfaces
{
    public interface IRepository
    {
        List<CheckoutRequest> GetCheckoutRequests(int id, int retryNumber = 0);
        int SaveCheckoutRequest(CheckoutRequest request, int retryNumber = 0);
        bool UpdateCheckoutRequestStatus(int id, Status newStatus, int retryNumber = 0);
    }
}
