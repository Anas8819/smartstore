using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SmartStore.Admin.Models.Customers;
using SmartStore.Core.Domain.Payments;
using SmartStore.Web.Framework;

namespace SmartStore.Web.Models.Customer
{
    public class PaymentRequestModel
    {
        [SmartResourceDisplayName("Agent.MyAccount.PaymentRequest.CustomerId")]
        public int CustomerId { get; set; }

        [SmartResourceDisplayName("Agent.MyAccount.PaymentRequest.Customer")]
        public virtual CustomerModel Customer { get; set; }

        [SmartResourceDisplayName("Agent.MyAccount.PaymentRequest.CreatedOnUtc")]
        public DateTime CreatedOnUtc { get; set; }

        [SmartResourceDisplayName("Agent.MyAccount.PaymentRequest.UpdatedOnUtc")]
        public DateTime UpdatedOnUtc { get; set; }

        [SmartResourceDisplayName("Agent.MyAccount.PaymentRequest.UserEmail")]
        public string UserEmail { get; set; }

        [SmartResourceDisplayName("Agent.MyAccount.PaymentRequest.UserFullName")]
        public string UserFullName { get; set; }

        [SmartResourceDisplayName("Agent.MyAccount.PaymentRequest.Amount")]
        public decimal Amount { get; set; }

        [SmartResourceDisplayName("Agent.MyAccount.PaymentRequest.Remarks")]
        public string Remarks { get; set; }

        [SmartResourceDisplayName("Agent.MyAccount.PaymentRequest.Date")]
        public DateTime Date { get; set; }

        [SmartResourceDisplayName("Agent.MyAccount.PaymentRequest.PaymentStatusId")]
        public int PaymentStatusId { get; set; }

        [SmartResourceDisplayName("Agent.MyAccount.PaymentRequest.PaymentStatus")]
        public PaymentStatus PaymentStatus
        {
            get
            {
                return (PaymentStatus)this.PaymentStatusId;
            }
            set
            {
                this.PaymentStatusId = (int)value;
            }
        }
    }
}