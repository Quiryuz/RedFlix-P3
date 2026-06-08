using System;
using System.Web.Mvc;
using RedFlix.Authorization;
using RedFlix.Helpers;

namespace RedFlix.Controllers
{
    [AuthorizePermission(Entity = PermissionKeys.Roles, ActionKey = PermissionKeys.Ver)]
    public class AdminController : Controller
    {
        public ActionResult Index()
        {
            if (Session["UsuarioID"] == null)
            {
                return RedirectToAction("Index", "Login");
            }

            if (!PermissionHelper.PuedeVerModulo(Session, PermissionKeys.Roles)
                && !(Session["RolID"] != null && Convert.ToInt32(Session["RolID"]) == 1))
            {
                return RedirectToAction("AccessDenied", "Home");
            }

            return View();
        }
    }
}
