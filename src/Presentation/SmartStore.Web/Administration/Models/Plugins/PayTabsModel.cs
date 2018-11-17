using SmartStore.Web.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SmartStore.Admin.Models.Plugins
{
    public class PayTabsModel
    {
        [SmartResourceDisplayName("Admin.Plugins.Paytabs.Fields.MerchantEmail")]
        public string MerchantEmail { get; set; }

        [SmartResourceDisplayName("Admin.Plugins.Paytabs.Fields.SecretKey")]
        public string SecretKey { get; set; }

        [SmartResourceDisplayName("Admin.Plugins.Paytabs.Fields.Currency")]
        public string Currency { get; set; }

        [SmartResourceDisplayName("Admin.Plugins.Paytabs.Fields.SiteUrl")]
        public string SiteUrl { get; set; }

        [SmartResourceDisplayName("Admin.Plugins.Paytabs.Fields.RefundPeriod")]
        public string RefundPeriod { get; set; }
    }
}