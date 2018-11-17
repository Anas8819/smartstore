using SmartStore.Core.Domain.Payments;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartStore.Services.Payments
{
    public partial interface IPayTabsConfigService
    {
        IEnumerable<PayTabsConfig> GetAllPayTabsPages();

        PayTabsConfig GetPayTabsPagesById(int id);

        void InsertPayTabs(PayTabsConfig request);

        void DeletePayTabs(PayTabsConfig request);

        void UpdatePayTabs(int id, PayTabsConfig request);
    }
}
