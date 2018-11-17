﻿using FluentValidation;
using FluentValidation.Attributes;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Domain.Tax;
using SmartStore.Core.Localization;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Modelling;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web;
using System.Web.Mvc;

namespace SmartStore.Web.Models.Customer
{
    [Validator(typeof(RegisterValidator))]
    public partial class RegisterModel : ModelBase
    {
        public RegisterModel()
        {
            this.AvailableTimeZones = new List<SelectListItem>();
            this.AvailableCountries = new List<SelectListItem>();
            this.AvailableStates = new List<SelectListItem>();
        }

        [SmartResourceDisplayName("Account.Fields.Email")]
        [AllowHtml]
		[DataType(DataType.EmailAddress)]
		public string Email { get; set; }

        public bool UsernamesEnabled { get; set; }
        [SmartResourceDisplayName("Account.Fields.Username")]
        [AllowHtml]
        public string Username { get; set; }

        public bool CheckUsernameAvailabilityEnabled { get; set; }

        [DataType(DataType.Password)]
        [SmartResourceDisplayName("Account.Fields.Password")]
        [AllowHtml]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [SmartResourceDisplayName("Account.Fields.ConfirmPassword")]
        [AllowHtml]
        public string ConfirmPassword { get; set; }

        //form fields & properties
        public bool GenderEnabled { get; set; }
        [SmartResourceDisplayName("Account.Fields.Gender")]
        public string Gender { get; set; }

		public bool FirstNameRequired { get; set; }
		public bool LastNameRequired { get; set; }
		[SmartResourceDisplayName("Account.Fields.FirstName")]
        [AllowHtml]
        public string FirstName { get; set; }
        [SmartResourceDisplayName("Account.Fields.LastName")]
        [AllowHtml]
        public string LastName { get; set; }

		public bool DateOfBirthEnabled { get; set; }
        [SmartResourceDisplayName("Account.Fields.DateOfBirth")]
        public int? DateOfBirthDay { get; set; }
        [SmartResourceDisplayName("Account.Fields.DateOfBirth")]
        public int? DateOfBirthMonth { get; set; }
        [SmartResourceDisplayName("Account.Fields.DateOfBirth")]
        public int? DateOfBirthYear { get; set; }

        public bool CompanyEnabled { get; set; }
        public bool CompanyRequired { get; set; }
        [SmartResourceDisplayName("Account.Fields.Company")]
        [AllowHtml]
        public string Company { get; set; }

        public bool StreetAddressEnabled { get; set; }
        public bool StreetAddressRequired { get; set; }
        [SmartResourceDisplayName("Account.Fields.StreetAddress")]
        [AllowHtml]
        public string StreetAddress { get; set; }

        public bool StreetAddress2Enabled { get; set; }
        public bool StreetAddress2Required { get; set; }
        [SmartResourceDisplayName("Account.Fields.StreetAddress2")]
        [AllowHtml]
        public string StreetAddress2 { get; set; }

        public bool ZipPostalCodeEnabled { get; set; }
        public bool ZipPostalCodeRequired { get; set; }
        [SmartResourceDisplayName("Account.Fields.ZipPostalCode")]
        [AllowHtml]
        public string ZipPostalCode { get; set; }

        public bool CityEnabled { get; set; }
        public bool CityRequired { get; set; }
        [SmartResourceDisplayName("Account.Fields.City")]
        [AllowHtml]
        public string City { get; set; }

        public bool CountryEnabled { get; set; }
        [SmartResourceDisplayName("Account.Fields.Country")]
        public int CountryId { get; set; }
        public IList<SelectListItem> AvailableCountries { get; set; }

        public bool StateProvinceEnabled { get; set; }
        [SmartResourceDisplayName("Account.Fields.StateProvince")]
        public int StateProvinceId { get; set; }
        public IList<SelectListItem> AvailableStates { get; set; }

        public bool PhoneEnabled { get; set; }
        public bool PhoneRequired { get; set; }
        [SmartResourceDisplayName("Account.Fields.Phone")]
        [AllowHtml]
		[DataType(DataType.PhoneNumber)]
		public string Phone { get; set; }

        public bool FaxEnabled { get; set; }
        public bool FaxRequired { get; set; }
        [SmartResourceDisplayName("Account.Fields.Fax")]
        [AllowHtml]
		[DataType(DataType.PhoneNumber)]
		public string Fax { get; set; }
        
        public bool NewsletterEnabled { get; set; }
        [SmartResourceDisplayName("Account.Fields.Newsletter")]
        public bool Newsletter { get; set; }

        //time zone
        [SmartResourceDisplayName("Account.Fields.TimeZone")]
        public string TimeZoneId { get; set; }
        public bool AllowCustomersToSetTimeZone { get; set; }
        public IList<SelectListItem> AvailableTimeZones { get; set; }

        //EU VAT
        [SmartResourceDisplayName("Account.Fields.VatNumber")]
        public string VatNumber { get; set; }
        public string VatNumberStatusNote { get; set; }
        public bool DisplayVatNumber { get; set; }
        public bool VatRequired { get; set; }

        public bool DisplayCaptcha { get; set; }

        public bool IsAgentEnabled { get; set; }

        [SmartResourceDisplayName("Account.Fields.IsAgent")]
        public bool IsAgent { get; set; }

        [SmartResourceDisplayName("Account.Fields.BankName")]
        public string BankName { get; set; }

        [SmartResourceDisplayName("Account.Fields.IBAN")]
        public string IBAN { get; set; }

        [SmartResourceDisplayName("Account.Fields.StoreName")]
        public string StoreName { get; set; }

        [SmartResourceDisplayName("Account.Fields.StoreLogo")]
        public HttpPostedFileBase StoreLogo { get; set; }

        [SmartResourceDisplayName("Account.Fields.MembershipPlan")]
        public string MembershipPlan { get; set; }
    }

    public class RegisterValidator : AbstractValidator<RegisterModel>
    {
        public RegisterValidator(Localizer T, CustomerSettings customerSettings, TaxSettings taxSettings)
        {
            RuleFor(x => x.Email).NotEmpty();
            RuleFor(x => x.Email).EmailAddress();

            RuleFor(x => x.Password).NotEmpty();
            RuleFor(x => x.Password).Length(customerSettings.PasswordMinLength, 999);
            RuleFor(x => x.ConfirmPassword).NotEmpty();
            RuleFor(x => x.ConfirmPassword).Equal(x => x.Password).WithMessage(T("Account.Fields.Password.EnteredPasswordsDoNotMatch"));

            //form fields
            if (customerSettings.FirstNameRequired)
            {
                RuleFor(x => x.FirstName).NotEmpty();
            }
            if (customerSettings.LastNameRequired)
            {
                RuleFor(x => x.LastName).NotEmpty();
            }
            if (customerSettings.CompanyRequired && customerSettings.CompanyEnabled)
            {
                RuleFor(x => x.Company).NotEmpty();
            }
            if (customerSettings.StreetAddressRequired && customerSettings.StreetAddressEnabled)
            {
                RuleFor(x => x.StreetAddress).NotEmpty();
            }
            if (customerSettings.StreetAddress2Required && customerSettings.StreetAddress2Enabled)
            {
                RuleFor(x => x.StreetAddress2).NotEmpty();
            }
            if (customerSettings.ZipPostalCodeRequired && customerSettings.ZipPostalCodeEnabled)
            {
                RuleFor(x => x.ZipPostalCode).NotEmpty();
            }
            if (customerSettings.CityRequired && customerSettings.CityEnabled)
            {
                RuleFor(x => x.City).NotEmpty();
            }
            if (customerSettings.PhoneRequired && customerSettings.PhoneEnabled)
            {
                RuleFor(x => x.Phone).NotEmpty();
            }
            if (customerSettings.FaxRequired && customerSettings.FaxEnabled)
            {
                RuleFor(x => x.Fax).NotEmpty();
            }
            if (taxSettings.EuVatEnabled && taxSettings.VatRequired)
            {
                RuleFor(x => x.VatNumber).NotEmpty();
            }
        }
    }
}