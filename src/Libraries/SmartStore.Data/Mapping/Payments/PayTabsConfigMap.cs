using SmartStore.Core.Domain.Payments;
using System;
using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartStore.Data.Mapping.Payments
{
    class PayTabsConfigMap : EntityTypeConfiguration<PayTabsConfig>
    {
        public PayTabsConfigMap()
        {
            this.ToTable("PayTabs");
            this.HasKey(x => x.Id);

            this.Property(x => x.MerchantEmail).IsRequired();
            this.Property(x => x.SecretKey).IsRequired();
            this.Property(x => x.Currency).IsRequired();
            this.Property(x => x.SiteUrl).IsRequired();
            
        }
    }
}
