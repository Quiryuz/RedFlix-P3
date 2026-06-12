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
            var usuario = db.usuarios
                .FirstOrDefault(x => x.Mail == email);

            if (usuario != null &&
                BCrypt.Net.BCrypt.Verify(contrasenia, usuario.Contrasena))
            {
                _permissionService.EnsurePermissionCatalog();

                _servicioPermisos.AsegurarCatalogoPermisos();
                Session["UsuarioID"] = usuario.ID;
                Session["Nombre"] = usuario.Nombre;
                Session["RolID"] = usuario.RolID;

                Session.Remove("PerfilID");
                Session.Remove("PerfilNombre");

                PermissionHelper.SetUserPermissions(
                    Session,
                    _permissionService.GetPermissionNamesForRole(usuario.RolID));

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
        public ActionResult Registro()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Registro(
     [Bind(Include = "Nombre,Mail,Contrasena")]
    usuarios usuario)
        {
            if (ModelState.IsValid)
            {
                if (db.usuarios.Any(x => x.Mail == usuario.Mail))
                {
                    ModelState.AddModelError(
                        "Mail",
                        "Ya existe un usuario con ese correo.");

                    return View(usuario);
                }

                // Buscar el rol Usuario en la BD
                var rolUsuario = db.Roles
                    .FirstOrDefault(r => r.Nombre == "Usuario");

                if (rolUsuario == null)
                {
                    ModelState.AddModelError(
                        "",
                        "No existe el rol Usuario en la base de datos.");

                    return View(usuario);
                }

                usuario.RolID = rolUsuario.ID;

                usuario.Contrasena =
                    BCrypt.Net.BCrypt.HashPassword(
                        usuario.Contrasena);

                db.usuarios.Add(usuario);
                db.SaveChanges();

                return RedirectToAction("Index");
            }

            return View(usuario);
        }
    }
}
