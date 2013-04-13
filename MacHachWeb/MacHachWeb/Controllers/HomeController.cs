using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MacHachWeb.Controllers
{
    public class HomeController : BaseController
    {
        public ActionResult Index()
        {
            ViewBag.Message = "Modify this template to jump-start your ASP.NET MVC application.";

            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your app description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        public ActionResult Store()
        {
            Indicator indicator = new Indicator
                                      {
                                          IndicatorName = DateTime.Now.ToString(CultureInfo.InvariantCulture)
                                      };
            RaveSession.Store(indicator);
            RaveSession.SaveChanges();
            return RedirectToAction("Index");
        }

        public ActionResult Show()
        {
            var indicator = RaveSession.Query<Indicator>().FirstOrDefault();
            return View(indicator);
        }
    }

    public class Indicator
    {
        public string IndicatorName { get; set; }
    }
}
