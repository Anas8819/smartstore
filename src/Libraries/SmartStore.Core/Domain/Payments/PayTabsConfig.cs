using SmartStore.Core.Domain.Localization;
using SmartStore.Core.Domain.Stores;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace SmartStore.Core.Domain.Payments
{
    [DataContract]
    public partial class PayTabsConfig : BaseEntity, ILocalizedEntity
    {
        public string MerchantEmail { get; set; }
        public string SecretKey { get; set; }
        public string Currency { get; set; }
        public string SiteUrl { get; set; }
        public string RefundPeriod { get; set; }
    }
}
