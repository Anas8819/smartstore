using SmartStore.Core.Domain.Payments;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartStore.Services.Payments
{
    public partial interface IPaymentRequestService
    {
        IEnumerable<PaymentRequest> GetAllPaymentRequests();

        PaymentRequest GetPaymentRequestsById(int id);

        void InsertPaymentRequest(PaymentRequest request);

        void DeletePaymentRequest(PaymentRequest request);

        void UpdatePaymentRequest(int id, PaymentRequest request);
    }
}
