using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Payments;

namespace SmartStore.Services.Payments
{
    public partial class PayTabsConfigService : IPayTabsConfigService
    {
        private readonly IRepository<PayTabsConfig> _payTabsRepository;
        public PayTabsConfigService(IRepository<PayTabsConfig> payTabsRepository)
        {
            _payTabsRepository = payTabsRepository;
        }

        public void DeletePayTabs(PayTabsConfig request)
        {
            Guard.NotNull(request, nameof(request));
            _payTabsRepository.Delete(request);
        }

        public IEnumerable<PayTabsConfig> GetAllPayTabsPages()
        {
            return _payTabsRepository.Table.Select(x => x);
        }

        public PayTabsConfig GetPayTabsPagesById(int id)
        {
            return _payTabsRepository.GetById(id);
        }

        public void InsertPayTabs(PayTabsConfig request)
        {
            Guard.NotNull(request, nameof(request));
            _payTabsRepository.Insert(request);
        }

        public void UpdatePayTabs(int id, PayTabsConfig request)
        {
            var requestSaved = _payTabsRepository.GetById(id);
            _payTabsRepository.Update(request);
        }
    }
}
