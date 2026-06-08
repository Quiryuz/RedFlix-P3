using System.Linq;
using System.Web.Mvc;
using RedFlix;
using RedFlix.Helpers;
using RedFlix.Services;

namespace RedFlix.Controllers
{
    public class LoginController : Controller
    {
        private readonly RedFlixIIIEntities db = new RedFlixIIIEntities();
        private readonly PermissionService _permissionService = new PermissionService();

        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Index(string email, string contrasenia)
        {
            var usuario = db.usuarios.FirstOrDefault(x => x.Mail == email && x.Contrasena == contrasenia);

            if (usuario != null)
            {
                _permissionService.EnsurePermissionCatalog();
                Session["UsuarioID"] = usuario.ID;
                Session["Nombre"] = usuario.Nombre;
                Session["RolID"] = usuario.RolID;
                Session.Remove("PerfilID");
                Session.Remove("PerfilNombre");
                PermissionHelper.SetUserPermissions(
                    Session,
                    _permissionService.GetPermissionNamesForRole(usuario.RolID));
                return RedirectToAction("Index", "MiPerfil");
            }

            ViewBag.Error = "Email o contrasena incorrectos";
            return View();
        }

        public ActionResult Logout()
        {
            Session.Clear();
            return RedirectToAction("Index");
        }
    }
}
