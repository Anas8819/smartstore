using SmartStore.Core.Configuration;
using SmartStore.PayTabs.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SmartStore.PayTabs
{
    public class PayTabSettings : ISettings
    {
        public PayTabSettings()
        {
            Marketplace = "de";
            DataFetching = PayTabsDataFetchingType.Ipn;
            TransactionType = PayTabsTransactionType.Authorize;
            AuthorizeMethod = PayTabsAuthorizeMethod.Omnichronous;
            SaveEmailAndPhone = PayTabsSaveDataType.OnlyIfEmpty;
            AddOrderNotes = true;
            InformCustomerAboutErrors = true;
            InformCustomerAddErrors = true;
            PollingMaxOrderCreationDays = 31;

            PayButtonColor = "Gold";
            PayButtonSize = "small";
            AuthButtonType = "LwA";
            AuthButtonColor = "Gold";
            AuthButtonSize = "medium";
        }

        public bool UseSandbox { get; set; }

        public string SellerId { get; set; }
        public string AccessKey { get; set; }
        public string SecretKey { get; set; }
        public string ClientId { get; set; }
        public string Marketplace { get; set; }

        public PayTabsDataFetchingType DataFetching { get; set; }
        public PayTabsTransactionType TransactionType { get; set; }
        public PayTabsAuthorizeMethod AuthorizeMethod { get; set; }

        public PayTabsSaveDataType? SaveEmailAndPhone { get; set; }
        public bool ShowPayButtonForAdminOnly { get; set; }
        public bool ShowButtonInMiniShoppingCart { get; set; }

        public int PollingMaxOrderCreationDays { get; set; }

        public decimal AdditionalFee { get; set; }
        public bool AdditionalFeePercentage { get; set; }

        public bool AddOrderNotes { get; set; }

        public bool InformCustomerAboutErrors { get; set; }
        public bool InformCustomerAddErrors { get; set; }

        public string PayButtonColor { get; set; }
        public string PayButtonSize { get; set; }

        public string AuthButtonType { get; set; }
        public string AuthButtonColor { get; set; }
        public string AuthButtonSize { get; set; }

        public bool CanSaveEmailAndPhone(string value)
        {
            return (SaveEmailAndPhone == PayTabsSaveDataType.Always || (SaveEmailAndPhone == PayTabsSaveDataType.OnlyIfEmpty && value.IsEmpty()));
        }
    }
}