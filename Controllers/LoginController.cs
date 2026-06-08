using System.Linq;
using System.Web.Mvc;
using RedFlix;

namespace RedFlix.Controllers
{
    public class LoginController : Controller
    {
        private readonly RedFlixIIIEntities db = new RedFlixIIIEntities();

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
                Session["UsuarioID"] = usuario.ID;
                Session["Nombre"] = usuario.Nombre;
                Session["RolID"] = usuario.RolID;
                return RedirectToAction("Index", "Home");
            }

            ViewBag.Error = "Email o contraseña incorrectos";
            return View();
        }

        public ActionResult Logout()
        {
            Session.Clear();
            return RedirectToAction("Index");
        }
    }
}
