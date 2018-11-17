using SmartStore.PayTabs.Models;
using SmartStore.PayTabs.Services;
using SmartStore.Web.Framework.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SmartStore.PayTabs.Controllers
{
    public abstract partial class PayTabsControllerBase : PublicControllerBase
    {
        protected ActionResult GetActionResult(PayTabsViewModel model)
        {
            switch (model.Result)
            {
                case PayTabsResultType.None:
                    return new EmptyResult();

                case PayTabsResultType.PluginView:
                    return View(model);

                case PayTabsResultType.Unauthorized:
                    return new HttpUnauthorizedResult();

                case PayTabsResultType.Redirect:
                default:
                    return RedirectToAction(model.RedirectAction, model.RedirectController, new { area = "" });
            }
        }
    }
}