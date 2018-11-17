using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace SmartStore.PayTabs
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                "SmartStore.PayTabs",
                "Plugins/SmartStore.PayTabs/{controller}/{action}",
                defaults: new { controller = "PayTabs", action = "Index", id = UrlParameter.Optional }
            )
            .DataTokens["area"] = PayTabsPlugin.SystemName;
        }
    }
}
