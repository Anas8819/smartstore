using SmartStore.Core.Logging;
using SmartStore.Core.Plugins;
using SmartStore.PayTabs.Controllers;
using SmartStore.PayTabs.Tasks;
using SmartStore.Services;
using SmartStore.Services.Authentication.External;
using SmartStore.Services.Customers;
using SmartStore.Services.Orders;
using SmartStore.Services.Payments;
using SmartStore.Services.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Routing;

namespace SmartStore.PayTabs
{
    public class PayTabsPlugin : PaymentPluginBase, IExternalAuthenticationMethod, IConfigurable
    {
        private readonly ICommonServices _services;
        private readonly IOrderTotalCalculationService _orderTotalCalculationService;
        private readonly IScheduleTaskService _scheduleTaskService;
        public ILogger Logger { get; set; }

        public PayTabsPlugin(
            ICommonServices services,
            IOrderTotalCalculationService orderTotalCalculationService,
            IScheduleTaskService scheduleTaskService)
        {
            _services = services;
            _orderTotalCalculationService = orderTotalCalculationService;
            _scheduleTaskService = scheduleTaskService;

            Logger = NullLogger.Instance;
        }

        public static string SystemName
        {
            get { return "SmartStore.PayTabs"; }
        }

        public override void Install()
        {
            int a = 0;
            _services.Settings.SaveSetting(new PayTabSettings(),a);
            _services.Localization.ImportPluginResourcesFromXml(PluginDescriptor);

            // Polling task every 30 minutes.
            _scheduleTaskService.GetOrAddTask<DataPollingTask>(x =>
            {
                x.Name = _services.Localization.GetResource("Plugins.Payments.AmazonPay.TaskName");
                x.CronExpression = "*/30 * * * *";
            });

            base.Install();
        }

        public override void Uninstall()
        {
            _scheduleTaskService.TryDeleteTask<DataPollingTask>();
            _services.Settings.DeleteSetting<PayTabSettings>();
            _services.Localization.DeleteLocaleStringResources(PluginDescriptor.ResourceRootKey);

            base.Uninstall();
        }

        









        //To be implemented or reviewed
        public void GetPublicInfoRoute(out string actionName, out string controllerName, out RouteValueDictionary routeValues)
        {
            throw new NotImplementedException();
        }

        public override ProcessPaymentResult ProcessPayment(ProcessPaymentRequest processPaymentRequest)
        {
            throw new NotImplementedException();
        }

        public override void GetConfigurationRoute(out string actionName, out string controllerName, out RouteValueDictionary routeValues)
        {
            throw new NotImplementedException();
        }

        public override void GetPaymentInfoRoute(out string actionName, out string controllerName, out RouteValueDictionary routeValues)
        {
            try
            {
                var settings = _services.Settings.LoadSetting<PayTabSettings>(_services.StoreContext.CurrentStore.Id);
                if (settings.ShowPayButtonForAdminOnly && !_services.WorkContext.CurrentCustomer.IsAdmin())
                {
                    actionName = controllerName = null;
                    routeValues = null;
                    return;
                }
            }
            catch (Exception exception)
            {
                Logger.Error(exception);
            }

            actionName = "ShoppingCart";
            controllerName = "PayTabShoppingCart";
            routeValues = new RouteValueDictionary { { "Namespaces", "SmartStore.PayTabs.Controllers" }, { "area", SystemName } };
        }

        public override Type GetControllerType()
        {
            return typeof(PayTabsController);
        }
    }
}