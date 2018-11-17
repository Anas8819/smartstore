using SmartStore.PayTabs.Services;
using SmartStore.Web.Framework.Modelling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SmartStore.PayTabs.Models
{
    public class PayTabsViewModel : ModelBase
    {
        public string SystemName
		{
			get { return PayTabsPlugin.SystemName; }
		}

        //To be discussed
        public string RedirectAction { get; set; }
        public string RedirectController { get; set; }
        public PayTabsResultType Result { get; set; }

        
        public string merchant_email;
        public string secret_key;
        public string ip_merchant;
        public string site_url;
        public string return_url;

        
        public string title;
        public string cc_first_name;
        public string cc_last_name;
        public string cc_phone_number;
        public string phone_number;
        public string email;
        public string state;
        public string city;
        public string country;
        public string postal_code;
        public string ip_customer;

        
        public string products_per_title;
        public string unit_price;

        
        public float other_charges;
        public float amount;
        public float discount;
        public string currency;
        public string reference_no;
        public string billing_address;
        public string quantity;
        public string shipping_first_name;
        public string shipping_last_name;
        public string address_shipping;
        public string city_shipping;
        public string state_shipping;
        public string postal_code_shipping;
        public string contry_shipping;

        
        public string msg_lang;
        public string cms_with_version;



    }
}