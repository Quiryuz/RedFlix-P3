using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace RedFlix.Controllers
{
    public class AdminController : Controller
    {
        // GET: Admin
        public ActionResult Index()
        {
            if (Session["rolID"] == null || Convert.ToInt32(Session["rolID"]) != 1)
            {
                return RedirectToAction("Index", "Home");
            }
            else
            {
                return View();
            }
        }
    }
}