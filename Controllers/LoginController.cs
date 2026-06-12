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
        private readonly PermissionService _servicioPermisos = new PermissionService();

        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Index(string email, string contrasenia)
        {
            if (string.IsNullOrWhiteSpace(email) ||
                string.IsNullOrWhiteSpace(contrasenia))
            {
                ViewBag.Error = "Debe ingresar email y contraseña.";
                return View();
            }

            var usuario = db.usuarios
                .FirstOrDefault(x => x.Mail == email);

            if (usuario != null &&
                BCrypt.Net.BCrypt.Verify(contrasenia, usuario.Contrasena))
            {
                _servicioPermisos.AsegurarCatalogoPermisos();

                Session["UsuarioID"] = usuario.ID;
                Session["Nombre"] = usuario.Nombre;
                Session["RolID"] = usuario.RolID;

                Session.Remove("PerfilID");
                Session.Remove("PerfilNombre");

                PermissionHelper.SetUserPermissions(
                    Session,
                    _servicioPermisos.ObtenerNombresPermisosPorRol(usuario.RolID));

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
