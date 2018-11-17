using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SmartStore.Services.Payments;
using SmartStore.Web.Framework.Controllers;

namespace SmartStore.PayTabs.Controllers
{
    public class PayTabsController : PaymentControllerBase
    {
        public override ProcessPaymentRequest GetPaymentInfo(FormCollection form)
        {
            var paymentInfo = new ProcessPaymentRequest();
            return paymentInfo;
        }

        public override IList<string> ValidatePaymentForm(FormCollection form)
        {
            var warnings = new List<string>();
            return warnings;
        }


    }
}