using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Script.Serialization;
using AutoMapper;
using SmartStore.Admin.Models.Plugins;
using SmartStore.Core.Domain.Cms;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Domain.Payments;
using SmartStore.Core.Domain.Security;
using SmartStore.Core.Domain.Shipping;
using SmartStore.Core.Domain.Tax;
using SmartStore.Core.Html;
using SmartStore.Core.Logging;
using SmartStore.Core.Plugins;
using SmartStore.Licensing;
using SmartStore.Services;
using SmartStore.Services.Authentication.External;
using SmartStore.Services.Cms;
using SmartStore.Services.Localization;
using SmartStore.Services.Payments;
using SmartStore.Services.Security;
using SmartStore.Services.Shipping;
using SmartStore.Services.Tax;
using SmartStore.Web.Framework.Controllers;
using SmartStore.Web.Framework.Plugins;
using SmartStore.Web.Framework.Security;

namespace SmartStore.Admin.Controllers
{
    
	[AdminAuthorize]
    public partial class PluginController : AdminControllerBase
	{
		#region Fields

        private readonly IPluginFinder _pluginFinder;
        private readonly IPermissionService _permissionService;
        private readonly ILanguageService _languageService;
        private readonly PaymentSettings _paymentSettings;
        private readonly ShippingSettings _shippingSettings;
        private readonly TaxSettings _taxSettings;
        private readonly ExternalAuthenticationSettings _externalAuthenticationSettings;
        private readonly WidgetSettings _widgetSettings;
		private readonly IProviderManager _providerManager;
		private readonly PluginMediator _pluginMediator;
		private readonly ICommonServices _services;
        private readonly IPayTabsConfigService _payTabsConfigService;

        #endregion

        #region Constructors

        public PluginController(IPluginFinder pluginFinder,
            IPermissionService permissionService,
			ILanguageService languageService,
            PaymentSettings paymentSettings,
			ShippingSettings shippingSettings,
            TaxSettings taxSettings, 
			ExternalAuthenticationSettings externalAuthenticationSettings, 
            WidgetSettings widgetSettings,
			IProviderManager providerManager,
			PluginMediator pluginMediator,
			ICommonServices services,
            IPayTabsConfigService payTabsConfigService)
		{
            this._pluginFinder = pluginFinder;
            this._permissionService = permissionService;
            this._languageService = languageService;
            this._paymentSettings = paymentSettings;
            this._shippingSettings = shippingSettings;
            this._taxSettings = taxSettings;
            this._externalAuthenticationSettings = externalAuthenticationSettings;
            this._widgetSettings = widgetSettings;
			this._providerManager = providerManager;
			this._pluginMediator = pluginMediator;
			this._services = services;
            this._payTabsConfigService = payTabsConfigService;
		}

        #endregion

        #region Utilities

        private bool IsLicensable(PluginDescriptor pluginDescriptor)
        {
            var result = false;

            try
            {
                result = LicenseChecker.IsLicensablePlugin(pluginDescriptor);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }

            return result;
        }

        private LicensingData PrepareLicenseLabelModel(LicenseLabelModel model, PluginDescriptor pluginDescriptor, string url = null)
		{
            if (IsLicensable(pluginDescriptor))
			{
				// We always show license button to serve ability to delete a key.
				model.IsLicensable = true;
				model.LicenseUrl = Url.Action("LicensePlugin", new { systemName = pluginDescriptor.SystemName });

				var license = LicenseChecker.GetLicense(pluginDescriptor.SystemName, url);
				if (license == null)
				{
					// Licensed plugin has not been used yet -> Check state.
					var unused = LicenseChecker.CheckState(pluginDescriptor.SystemName, url);

					// And try to get license data again.
					license = LicenseChecker.GetLicense(pluginDescriptor.SystemName, url);
				}

				if (license != null)
				{
					// Licensed plugin has been used.
					model.LicenseState = license.State;
					model.TruncatedLicenseKey = license.TruncatedLicenseKey;
					model.RemainingDemoUsageDays = license.RemainingDemoDays;
				}
				else
				{
					// It's confusing to display a license state when there is no license data yet.
					model.HideLabel = true;
				}

				return license;
			}

			return null;
		}

		[NonAction]
        protected PluginModel PreparePluginModel(PluginDescriptor pluginDescriptor, bool forList = true)
        {
            var model = pluginDescriptor.ToModel();

			// Using GetResource because T could fallback to NullLocalizer here.
			model.Group = _services.Localization.GetResource("Admin.Plugins.KnownGroup." + pluginDescriptor.Group);

			if (forList)
			{
				model.FriendlyName = pluginDescriptor.GetLocalizedValue(_services.Localization, "FriendlyName");
				model.Description = pluginDescriptor.GetLocalizedValue(_services.Localization, "Description");
			}

            // Locales
            AddLocales(_languageService, model.Locales, (locale, languageId) =>
            {
				locale.FriendlyName = pluginDescriptor.GetLocalizedValue(_services.Localization, "FriendlyName", languageId, false);
				locale.Description = pluginDescriptor.GetLocalizedValue(_services.Localization, "Description", languageId, false);
            });

			// Stores
			model.SelectedStoreIds = _services.Settings.GetSettingByKey<string>(pluginDescriptor.GetSettingKey("LimitedToStores")).ToIntArray();

			// Icon
			model.IconUrl = _pluginMediator.GetIconUrl(pluginDescriptor);
            
            if (pluginDescriptor.Installed)
            {
                // specify configuration URL only when a plugin is already installed
				if (pluginDescriptor.IsConfigurable)
				{
					model.ConfigurationUrl = Url.Action("ConfigurePlugin", new { systemName = pluginDescriptor.SystemName });

					if (!forList)
					{
						var configurable = pluginDescriptor.Instance() as IConfigurable;

						string actionName;
						string controllerName;
						RouteValueDictionary routeValues;
						configurable.GetConfigurationRoute(out actionName, out controllerName, out routeValues);

						if (actionName.HasValue() && controllerName.HasValue())
						{
							model.ConfigurationRoute = new RouteInfo(actionName, controllerName, routeValues);
						}
					}
				}

				// License label
				PrepareLicenseLabelModel(model.LicenseLabel, pluginDescriptor);
            }

            return model;
        }

        [NonAction]
        protected LocalPluginsModel PrepareLocalPluginsModel()
        {
            var plugins = _pluginFinder.GetPluginDescriptors(false)
                .OrderBy(p => p.Group, PluginFileParser.KnownGroupComparer)
                .ThenBy(p => p.DisplayOrder)
                .Select(x => PreparePluginModel(x));

			var model = new LocalPluginsModel();

			model.AvailableStores = _services.StoreService
				.GetAllStores()
				.Select(s => s.ToModel())
				.ToList();

            var groupedPlugins = from p in plugins
                                 group p by p.Group into g
                                 select g;

            foreach (var group in groupedPlugins)
            {
                foreach (var plugin in group)
                {
                    model.Groups.Add(group.Key, plugin);
                }
            }

            return model;
        }

        #endregion

        #region Methods

        public ActionResult PaytabsConfig()
        {
            return View();
        }

        //public PayTabsConfig ToEntity(PayTabsModel model)
        //{
        //    return Mapper.Map<PayTabsConfig>(model);
        //}

        private PayTabsConfig ToEntity(PayTabsModel model)
        {
            return new PayTabsConfig
            {
                Currency = model.Currency,
                RefundPeriod = model.RefundPeriod,
                SecretKey = model.SecretKey,
                SiteUrl = model.SiteUrl,
                MerchantEmail = model.MerchantEmail
            };
        }

        private PayTabsModel ToModel(PayTabsConfig entity)
        {
            return new PayTabsModel
            {
                Currency = entity.Currency,
                RefundPeriod = entity.RefundPeriod,
                SecretKey = entity.SecretKey,
                SiteUrl = entity.SiteUrl,
                MerchantEmail = entity.MerchantEmail
            };
        }

        [HttpPost]
        public ActionResult PaytabsConfig(PayTabsModel payTabsModel)
        {
            var entity = ToEntity(payTabsModel);
            _payTabsConfigService.InsertPayTabs(entity);
            return RedirectToAction("PayTabsApiRequestAsync", new { amount = 3.0 , id = entity.Id});
        }

        public async Task<ActionResult> PayTabsApiRequestAsync(float amount, int id)
        {
            PayTabsApiRequestModel payTabsApiRequestModel = new PayTabsApiRequestModel();
            PayTabsConfig payTabsConfig = _payTabsConfigService.GetPayTabsPagesById(id);
            PayTabsModel model = ToModel(payTabsConfig);
            payTabsApiRequestModel.merchant_email = model.MerchantEmail;
            payTabsApiRequestModel.secret_key = model.SecretKey;
            payTabsApiRequestModel.currency = model.Currency;
            payTabsApiRequestModel.site_url = model.SiteUrl;
            payTabsApiRequestModel.amount = amount;

            //To redirect to transection status after payment
            payTabsApiRequestModel.return_url = "http://localhost:57920/admin/Plugin/TransactionStatusAsync";

            //hard coded data
            #region data
            payTabsApiRequestModel.title = "SmartStoreNET";
            payTabsApiRequestModel.cc_first_name = "Anas";
            payTabsApiRequestModel.cc_last_name = "Ahmed";
            payTabsApiRequestModel.cc_phone_number = "00966";
            payTabsApiRequestModel.phone_number = "511484815";
            payTabsApiRequestModel.email = "anasahmed8819@hotmail.coom";
            payTabsApiRequestModel.products_per_title = "aaa";
            payTabsApiRequestModel.unit_price = "2";
            payTabsApiRequestModel.quantity = "1";
            payTabsApiRequestModel.other_charges = (float)1.00;
            payTabsApiRequestModel.discount = (float)1.0;
            payTabsApiRequestModel.reference_no = "Abc-5544";
            payTabsApiRequestModel.ip_customer = "123.123.12.2";
            payTabsApiRequestModel.ip_merchant = "11.11.22.22";
            payTabsApiRequestModel.billing_address = "Flat 11 Building 222 Block 333 Road 444 Manama Bahrain";
            payTabsApiRequestModel.state = "Manama";
            payTabsApiRequestModel.city = "Manama";
            payTabsApiRequestModel.postal_code = "12345";
            payTabsApiRequestModel.country = "BHR";
            payTabsApiRequestModel.shipping_first_name = "John";
            payTabsApiRequestModel.shipping_last_name = "Doe";
            payTabsApiRequestModel.address_shipping = "Flat abc road 123";
            payTabsApiRequestModel.city_shipping = "Manama";
            payTabsApiRequestModel.state_shipping = "Manama";
            payTabsApiRequestModel.postal_code_shipping = "403129";
            payTabsApiRequestModel.contry_shipping = "BHR";
            payTabsApiRequestModel.msg_lang = "English";
            payTabsApiRequestModel.cms_with_version = "Magento 0.1.9";
            #endregion

            //object preperation code for sending request
            #region
            List<KeyValuePair<string, string>> parameters = new List<KeyValuePair<string, string>>();
            parameters.Add(new KeyValuePair<string, string>("merchant_email", payTabsApiRequestModel.merchant_email));
            parameters.Add(new KeyValuePair<string, string>("secret_key", payTabsApiRequestModel.secret_key));
            parameters.Add(new KeyValuePair<string, string>("site_url", payTabsApiRequestModel.site_url));
            parameters.Add(new KeyValuePair<string, string>("return_url", payTabsApiRequestModel.return_url));
            parameters.Add(new KeyValuePair<string, string>("title", payTabsApiRequestModel.title));
            parameters.Add(new KeyValuePair<string, string>("cc_first_name", payTabsApiRequestModel.cc_first_name));
            parameters.Add(new KeyValuePair<string, string>("cc_last_name", payTabsApiRequestModel.cc_last_name));
            parameters.Add(new KeyValuePair<string, string>("cc_phone_number", payTabsApiRequestModel.cc_phone_number));
            parameters.Add(new KeyValuePair<string, string>("phone_number", payTabsApiRequestModel.phone_number));
            parameters.Add(new KeyValuePair<string, string>("email", payTabsApiRequestModel.email));
            parameters.Add(new KeyValuePair<string, string>("products_per_title", payTabsApiRequestModel.products_per_title));
            parameters.Add(new KeyValuePair<string, string>("unit_price", payTabsApiRequestModel.unit_price));
            parameters.Add(new KeyValuePair<string, string>("quantity", payTabsApiRequestModel.quantity));
            parameters.Add(new KeyValuePair<string, string>("other_charges", Convert.ToString(payTabsApiRequestModel.other_charges)));
            parameters.Add(new KeyValuePair<string, string>("amount", Convert.ToString(payTabsApiRequestModel.amount)));
            parameters.Add(new KeyValuePair<string, string>("discount", Convert.ToString(payTabsApiRequestModel.discount)));
            parameters.Add(new KeyValuePair<string, string>("currency", payTabsApiRequestModel.currency));
            parameters.Add(new KeyValuePair<string, string>("reference_no", payTabsApiRequestModel.reference_no));
            parameters.Add(new KeyValuePair<string, string>("ip_customer", payTabsApiRequestModel.ip_customer));
            parameters.Add(new KeyValuePair<string, string>("ip_merchant", payTabsApiRequestModel.ip_merchant));
            parameters.Add(new KeyValuePair<string, string>("billing_address", payTabsApiRequestModel.billing_address));
            parameters.Add(new KeyValuePair<string, string>("state", payTabsApiRequestModel.state));
            parameters.Add(new KeyValuePair<string, string>("city", payTabsApiRequestModel.city));
            parameters.Add(new KeyValuePair<string, string>("postal_code", payTabsApiRequestModel.postal_code));
            parameters.Add(new KeyValuePair<string, string>("country", payTabsApiRequestModel.country));
            parameters.Add(new KeyValuePair<string, string>("shipping_first_name", payTabsApiRequestModel.shipping_first_name));
            parameters.Add(new KeyValuePair<string, string>("shipping_last_name", payTabsApiRequestModel.shipping_last_name));
            parameters.Add(new KeyValuePair<string, string>("address_shipping", payTabsApiRequestModel.address_shipping));
            parameters.Add(new KeyValuePair<string, string>("city_shipping", payTabsApiRequestModel.city_shipping));
            parameters.Add(new KeyValuePair<string, string>("state_shipping", payTabsApiRequestModel.state_shipping));
            parameters.Add(new KeyValuePair<string, string>("postal_code_shipping", payTabsApiRequestModel.postal_code_shipping));
            parameters.Add(new KeyValuePair<string, string>("country_shipping", payTabsApiRequestModel.contry_shipping));
            parameters.Add(new KeyValuePair<string, string>("msg_lang", payTabsApiRequestModel.msg_lang));
            parameters.Add(new KeyValuePair<string, string>("cms_with_version", payTabsApiRequestModel.cms_with_version));
            #endregion

            var client = new HttpClient();
            string URI = "https://www.paytabs.com/apiv2/create_pay_page";
            var requestContent = new FormUrlEncodedContent(parameters);
            HttpResponseMessage r = await client.PostAsync(URI, requestContent);
            HttpContent responseContent = r.Content;
            string responseString = await r.Content.ReadAsStringAsync();

            PayTabPayPageResponse payTabPayPageResponse = new PayTabPayPageResponse();
            payTabPayPageResponse = new JavaScriptSerializer().Deserialize<PayTabPayPageResponse>(responseString);

            //TempData["merchant_email"] = payTabsApiRequestModel.merchant_email;
            //TempData["secret_key"] = payTabsApiRequestModel.secret_key;
            //TempData["payment_reference"] = payTabPayPageResponse.p_id;


            return Redirect(payTabPayPageResponse.payment_url);
        }

        public async Task<ActionResult> TransactionStatusAsync()
        {

            //control redirected from PayTabsApiRequestAsync
            //string merchant_email = (string)TempData["merchant_email"];
            //string secret_key = (string)TempData["secret_key"];
            //string payment_reference = (string)TempData["payment_reference"];

            string merchant_email = "anasahmed8819@hotmail.com";
            string payment_reference = "220139";
            string secret_key = "IQObGV4SNKxD6h6HqtWb9hFpZGqjj7d1Y2cEXhxK15o8mvPNYLZ73Fx3lXmF4AH2CTHAbfkdkJlHebvcXuk19R4vOraT2RWELEeX";
            List<KeyValuePair<string, string>> parameters = new List<KeyValuePair<string, string>>();
            parameters.Add(new KeyValuePair<string, string>("merchant_email", merchant_email));
            parameters.Add(new KeyValuePair<string, string>("secret_key", secret_key));
            parameters.Add(new KeyValuePair<string, string>("payment_reference", payment_reference));

            var client = new HttpClient();
            string URI = "https://www.paytabs.com/apiv2/verify_payment";
            var requestContent = new FormUrlEncodedContent(parameters);
            HttpResponseMessage r = await client.PostAsync(URI, requestContent);
            HttpContent responseContent = r.Content;
            string responseString = await r.Content.ReadAsStringAsync();

            PayTabTransactionStatusResponseModel payTabValidateKeyResponseModel = new PayTabTransactionStatusResponseModel();
            payTabValidateKeyResponseModel = new JavaScriptSerializer().Deserialize<PayTabTransactionStatusResponseModel>(responseString);

            if (payTabValidateKeyResponseModel.response_code == "100" || payTabValidateKeyResponseModel.response_code == "481" || payTabValidateKeyResponseModel.response_code == "482")
            {
                TempData["Status"] = "Success";
                TempData["Msg"] = "Transection with Transection Id " + payTabValidateKeyResponseModel.transaction_id +
                    " Reference No " + payTabValidateKeyResponseModel.reference_no +
                    " Invoice Id " + payTabValidateKeyResponseModel.pt_invoice_id +
                    " has the status " + payTabValidateKeyResponseModel.result;
            }
            else
            {
                TempData["Status"] = "Error";
                TempData["Msg"] = "Transection with" +
                    " Reference No: " + payTabValidateKeyResponseModel.reference_no +
                    " Invoice Id: " + payTabValidateKeyResponseModel.pt_invoice_id +
                    " has the status: " + payTabValidateKeyResponseModel.result;
            }
            return RedirectToAction("PaytabsConfig");
        }

        public ActionResult Index()
        {
            return RedirectToAction("List");
        }

        public ActionResult List()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePlugins))
                return AccessDeniedView();

            var model = PrepareLocalPluginsModel();
            return View(model);
        }

        [HttpPost]
        public ActionResult ExecuteTasks(IEnumerable<string> pluginsToInstall, IEnumerable<string> pluginsToUninstall)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePlugins))
                return AccessDeniedView();

            try
            {
                int tasksCount = 0;
                IEnumerable<PluginDescriptor> descriptors = null;

                // Uninstall first
                if (pluginsToUninstall != null && pluginsToUninstall.Any())
                {
                    descriptors = _pluginFinder.GetPluginDescriptors(false).Where(x => pluginsToUninstall.Contains(x.SystemName));
                    foreach (var d in descriptors)
                    {
                        if (d.Installed)
                        {
                            d.Instance().Uninstall();
                            tasksCount++;
                        }
                    }
                }

                // now execute installations
                if (pluginsToInstall != null && pluginsToInstall.Any())
                {
                    descriptors = _pluginFinder.GetPluginDescriptors(false).Where(x => pluginsToInstall.Contains(x.SystemName));
                    foreach (var d in descriptors)
                    {
                        if (!d.Installed)
                        {
                            d.Instance().Install();
                            tasksCount++;
                        }
                    }
                }

                // restart application
                if (tasksCount > 0)
                {
					_services.WebHelper.RestartAppDomain(aggressive: true);
                }
            }
            catch (Exception exc)
            {
                NotifyError(exc);
            }

            return RedirectToAction("List");
        }

        public ActionResult ReloadList()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePlugins))
                return AccessDeniedView();

            // restart application
			_services.WebHelper.RestartAppDomain(aggressive: true);

            return RedirectToAction("List");
        }
        
        public ActionResult ConfigurePlugin(string systemName)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePlugins))
                return AccessDeniedView();

            var descriptor = _pluginFinder.GetPluginDescriptorBySystemName(systemName);
            if (descriptor == null || !descriptor.Installed || !descriptor.IsConfigurable)
				return HttpNotFound();

			var model = PreparePluginModel(descriptor, false);
			model.FriendlyName = descriptor.GetLocalizedValue(_services.Localization, "FriendlyName");
            
            return View(model);
        }

		public ActionResult ConfigureProvider(string systemName, string listUrl)
		{
			var provider = _providerManager.GetProvider(systemName);
			if (provider == null || !provider.Metadata.IsConfigurable)
			{
				return HttpNotFound();
			}

			PermissionRecord requiredPermission = StandardPermissionProvider.AccessAdminPanel;
			var listUrl2 = Url.Action("List");

			var metadata = provider.Metadata;

			if (metadata.ProviderType == typeof(IPaymentMethod))
			{
				requiredPermission = StandardPermissionProvider.ManagePaymentMethods;
				listUrl2 = Url.Action("Providers", "Payment");
			}
			if (metadata.ProviderType == typeof(ITaxProvider))
			{
				requiredPermission = StandardPermissionProvider.ManageTaxSettings;
				listUrl2 = Url.Action("Providers", "Tax");
			}
			else if (metadata.ProviderType == typeof(IShippingRateComputationMethod))
			{
				requiredPermission = StandardPermissionProvider.ManageShippingSettings;
				listUrl2 = Url.Action("Providers", "Shipping");
			}
			else if (metadata.ProviderType == typeof(IWidget))
			{
				requiredPermission = StandardPermissionProvider.ManageWidgets;
				listUrl2 = Url.Action("Providers", "Widget");
			}
			else if (metadata.ProviderType == typeof(IExternalAuthenticationMethod))
			{
				requiredPermission = StandardPermissionProvider.ManageExternalAuthenticationMethods;
				listUrl2 = Url.Action("Providers", "ExternalAuthentication");
			}

			if (!_permissionService.Authorize(requiredPermission))
			{
				return AccessDeniedView();
			}

			var model = _pluginMediator.ToProviderModel(provider);

			ViewBag.ListUrl = listUrl.NullEmpty() ?? listUrl2;

			return View(model);
		}

		public ActionResult LicensePlugin(string systemName, string licenseKey)
		{
			if (!_permissionService.Authorize(StandardPermissionProvider.ManagePlugins))
				return AccessDeniedPartialView();

			var descriptor = _pluginFinder.GetPluginDescriptorBySystemName(systemName);

			if (descriptor == null || !descriptor.Installed)
				return Content(T("Admin.Common.ResourceNotFound"));

			bool singleLicenseForAllStores = false;
			bool isLicensable = LicenseChecker.IsLicensablePlugin(descriptor, out singleLicenseForAllStores);

			if (!isLicensable)
				return Content(T("Admin.Common.ResourceNotFound"));

			var stores = _services.StoreService.GetAllStores();
			var model = new LicensePluginModel
			{
				SystemName = systemName,
				StoreLicenses = new List<LicensePluginModel.StoreLicenseModel>()
			};

			// Validate store url
			foreach (var store in stores)
			{
				if (!_services.StoreService.IsStoreDataValid(store))
				{
					model.InvalidDataStoreId = store.Id;
					return View(model);
				}
			}

			if (singleLicenseForAllStores)
			{
				var licenseModel = new LicensePluginModel.StoreLicenseModel();

				// License label
				var license = PrepareLicenseLabelModel(licenseModel.LicenseLabel, descriptor);

				if (license != null)
					licenseModel.LicenseKey = license.TruncatedLicenseKey;

				model.StoreLicenses.Add(licenseModel);
			}
			else
			{
				foreach (var store in stores)
				{
					var licenseModel = new LicensePluginModel.StoreLicenseModel
					{
						StoreId = store.Id,
						StoreName = store.Name,
						StoreUrl = store.Url
					};

					// License label
					var license = PrepareLicenseLabelModel(licenseModel.LicenseLabel, descriptor, store.Url);

					if (license != null)
						licenseModel.LicenseKey = license.TruncatedLicenseKey;

					model.StoreLicenses.Add(licenseModel);
				}
			}

			return View(model);
		}

		[HttpPost]
		public ActionResult LicensePlugin(string systemName, LicensePluginModel model)
		{
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePlugins))
            {
                return AccessDeniedView();
            }

			var descriptor = _pluginFinder.GetPluginDescriptorBySystemName(systemName);
            if (descriptor == null || !descriptor.Installed)
            {
                return HttpNotFound();
            }

			var isLicensable = IsLicensable(descriptor);
            if (!isLicensable)
            {
                return HttpNotFound();
            }

			if (model.StoreLicenses != null)
			{
				foreach (var item in model.StoreLicenses)
				{
					var result = LicenseChecker.Activate(item.LicenseKey, descriptor.SystemName, item.StoreUrl);

					if (result == null)
					{
						// do nothing, skiped
					}
					else if (result.Success)
					{
						NotifySuccess(T("Admin.Configuration.Plugins.LicenseActivated"));
					}
					else
					{
						if (result.IsFailureWarning)
							NotifyWarning(result.ToString());
						else
							NotifyError(result.ToString());

						return RedirectToAction("List");
					}
				}
			}

			return RedirectToAction("List");
		}

		[HttpPost]
		public ActionResult LicenseResetStatusCheck(string systemName)
		{
			// Reset state for current store.
			var result = LicenseChecker.ResetState(systemName);
			LicenseCheckerResult subShopResult = null;

			var model = new LicenseLabelModel
			{
				IsLicensable = true,
				LicenseUrl = Url.Action("LicensePlugin", new { systemName = systemName }),
				LicenseState = result.State,
				TruncatedLicenseKey = result.TruncatedLicenseKey,
				RemainingDemoUsageDays = result.RemainingDemoDays
			};

			// Reset state for all other stores.
			if (result.Success)
			{
				var currentStoreId = Services.StoreContext.CurrentStore.Id;
				var allStores = Services.StoreService.GetAllStores();

				foreach (var store in allStores.Where(x => x.Id != currentStoreId && x.Url.HasValue()))
				{
					subShopResult = LicenseChecker.ResetState(systemName, store.Url);
					if (!subShopResult.Success)
					{
						result = subShopResult;
						break;
					}
				}
			}

			// Notify about result.
			if (result.Success)
			{
				NotifySuccess(T("Admin.Common.TaskSuccessfullyProcessed"));
			}
			else
			{
				var message = HtmlUtils.ConvertPlainTextToHtml(result.ToString());
				if (result.IsFailureWarning)
				{
					NotifyWarning(message);
				}
				else
				{
					NotifyError(message);
				}
			}

			return PartialView("Partials/LicenseLabel", model);
		}

		public ActionResult EditProviderPopup(string systemName)
		{
			if (!_permissionService.Authorize(StandardPermissionProvider.ManagePlugins))
				return AccessDeniedView();

			var provider = _providerManager.GetProvider(systemName);
			if (provider == null)
				return HttpNotFound();

			var model = _pluginMediator.ToProviderModel(provider, true);
			var pageTitle = model.FriendlyName;

			AddLocales(_languageService, model.Locales, (locale, languageId) =>
			{
				locale.FriendlyName = _pluginMediator.GetLocalizedFriendlyName(provider.Metadata, languageId, false);
				locale.Description = _pluginMediator.GetLocalizedDescription(provider.Metadata, languageId, false);

				if (pageTitle.IsEmpty() && languageId == _services.WorkContext.WorkingLanguage.Id)
				{
					pageTitle = locale.FriendlyName;
				}
			});

			ViewBag.Title = pageTitle;

			return View(model);
		}

		[HttpPost]
		public ActionResult EditProviderPopup(string btnId, ProviderModel model)
		{
			if (!_permissionService.Authorize(StandardPermissionProvider.ManagePlugins))
				return AccessDeniedView();

			var provider = _providerManager.GetProvider(model.SystemName);
			if (provider == null)
				return HttpNotFound();

			var metadata = provider.Metadata;

			_pluginMediator.SetSetting(metadata, "FriendlyName", model.FriendlyName);
			_pluginMediator.SetSetting(metadata, "Description", model.Description);

			foreach (var localized in model.Locales)
			{
				_pluginMediator.SaveLocalizedValue(metadata, localized.LanguageId, "FriendlyName", localized.FriendlyName);
				_pluginMediator.SaveLocalizedValue(metadata, localized.LanguageId, "Description", localized.Description);
			}

			ViewBag.RefreshPage = true;
			ViewBag.btnId = btnId;
			return View(model);
		}

		[HttpPost]
		public ActionResult SetSelectedStores(string pk /* SystemName */, string name, FormCollection form)
		{
			// gets called from x-editable 
			try 
			{
				var pluginDescriptor = _pluginFinder.GetPluginDescriptorBySystemName(pk, false);
				if (pluginDescriptor == null)
				{
					return HttpNotFound("The plugin does not exist");
				}
				
				string settingKey = pluginDescriptor.GetSettingKey("LimitedToStores");
				var storeIds = (form["value[]"] ?? "0").Split(',').Select(x => x.ToInt()).Where(x => x > 0).ToList();
				if (storeIds.Count > 0)
				{
					_services.Settings.SetSetting<string>(settingKey, string.Join(",", storeIds));
				}
				else
				{
					_services.Settings.DeleteSetting(settingKey);
				}
			}
			catch (Exception ex)
			{
				return new HttpStatusCodeResult(501, ex.Message);
			}

			NotifySuccess(T("Admin.Common.DataSuccessfullySaved"));
			return new HttpStatusCodeResult(200);
		}

		[HttpPost]
		public ActionResult SortProviders(string providers)
		{
			try
			{
				var arr = providers.Split(',');
				int ordinal = 5;
				foreach (var systemName in arr)
				{
					var provider = _providerManager.GetProvider(systemName);
					if (provider != null)
					{
						_pluginMediator.SetUserDisplayOrder(provider.Metadata, ordinal);
					}
					ordinal += 5;
				}
			}
			catch (Exception ex)
			{
				NotifyError(ex.Message);
				return new HttpStatusCodeResult(501, ex.Message);
			}


			NotifySuccess(T("Admin.Common.DataSuccessfullySaved"));
			return new HttpStatusCodeResult(200);
		}

		public ActionResult UpdateStringResources(string systemName, string returnUrl = null)
		{
			if (!_permissionService.Authorize(StandardPermissionProvider.ManagePlugins))
				return AccessDeniedView();

			var pluginDescriptor = _pluginFinder.GetPluginDescriptors()
				.FirstOrDefault(x => x.SystemName.Equals(systemName, StringComparison.InvariantCultureIgnoreCase));

			if (pluginDescriptor == null)
			{
				NotifyError(T("Admin.Configuration.Plugins.Resources.UpdateFailure"));
			}
			else
			{
				_services.Localization.ImportPluginResourcesFromXml(pluginDescriptor, null, false);

				NotifySuccess(T("Admin.Configuration.Plugins.Resources.UpdateSuccess"));
			}

			return RedirectToReferrer(returnUrl, () => RedirectToAction("List"));
		}

		public ActionResult UpdateAllStringResources()
		{
			if (!_permissionService.Authorize(StandardPermissionProvider.ManagePlugins))
				return AccessDeniedView();

			var pluginDescriptors = _pluginFinder.GetPluginDescriptors(false);

			foreach (var plugin in pluginDescriptors)
			{
				if (plugin.Installed)
				{
					_services.Localization.ImportPluginResourcesFromXml(plugin, null, false);
				}
				else
				{
					_services.Localization.DeleteLocaleStringResources(plugin.ResourceRootKey);
				}
			}

			NotifySuccess(T("Admin.Configuration.Plugins.Resources.UpdateSuccess"));

			return RedirectToAction("List");
		}

        #endregion
    }
}
