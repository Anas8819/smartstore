using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SmartStore.Admin.Models.Plugins
{
    public class PayTabTransactionStatusResponseModel
    {
        public string result { get; set; }
        public string response_code { get; set; }
        public string transaction_id { get; set; }
        public string amount { get; set; }
        public string currency { get; set; }
        public string reference_no { get; set; }
        public string pt_invoice_id { get; set; }
    }
}