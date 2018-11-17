using SmartStore.Core.Data;
using SmartStore.Core.Domain.Payments;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartStore.Services.Payments
{
    public partial class PaymentRequestService : IPaymentRequestService
    {
        private readonly IRepository<PaymentRequest> _paymentRequestRepository;

        public PaymentRequestService(IRepository<PaymentRequest> paymentRequestRepository)
        {
            _paymentRequestRepository = paymentRequestRepository;
        }

        public void DeletePaymentRequest(PaymentRequest request)
        {
            Guard.NotNull(request, nameof(request));
            _paymentRequestRepository.Delete(request);
        }

        public IEnumerable<PaymentRequest> GetAllPaymentRequests()
        {
            return _paymentRequestRepository.Table.Select(x => x);
        }

        public PaymentRequest GetPaymentRequestsById(int id)
        {
            return _paymentRequestRepository.GetById(id);
        }

        public void InsertPaymentRequest(PaymentRequest request)
        {
            Guard.NotNull(request, nameof(request));
            _paymentRequestRepository.Insert(request);
        }

        public void UpdatePaymentRequest(int id, PaymentRequest request)
        {
            var requestSaved = _paymentRequestRepository.GetById(id);
            _paymentRequestRepository.Update(request);
        }
    }
}
