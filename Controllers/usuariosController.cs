using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using RedFlix;
using RedFlix.Authorization;
using RedFlix.ViewModels;

namespace RedFlix.Controllers
{
    [AuthorizePermission(Entity = PermissionKeys.Usuarios)]
    public class usuariosController : Controller
    {
        private RedFlixIIIEntities db = new RedFlixIIIEntities();

        // GET: usuarios
        public ActionResult Index()
        {
            var usuarios = db.usuarios.Include(u => u.Roles);
            return View(usuarios.ToList());
        }

        // GET: usuarios/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            usuarios usuarios = db.usuarios.Find(id);
            if (usuarios == null)
            {
                return HttpNotFound();
            }
            return View(usuarios);
        }

        [AuthorizePermission(Entity = PermissionKeys.Usuarios, AllowAnonymous = true)]
        public ActionResult Create()
        {
            PrepararFormularioCreacionUsuario();
            return View();
        }

        // POST: usuarios/Create
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que quiere enlazarse. Para obtener 
        // más detalles, vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AuthorizePermission(Entity = PermissionKeys.Usuarios, AllowAnonymous = true)]
        public ActionResult Create([Bind(Include = "ID,Nombre,Mail,RolID,Contrasena")] usuarios usuarios)
        {
            var esAdministrador = EsAdministradorAutenticado();
            var rolUsuario = ObtenerRolUsuario();

            if (rolUsuario == null)
            {
                ModelState.AddModelError("", "No se encontro el rol base Usuario.");
                PrepararFormularioCreacionUsuario(usuarios.RolID);
                return View(usuarios);
            }

            if (!esAdministrador)
            {
                if (usuarios.RolID != 0 && usuarios.RolID != rolUsuario.ID)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.Forbidden, "No autorizado para asignar roles especiales.");
                }

                usuarios.RolID = rolUsuario.ID;
            }
            else if (!db.Roles.Any(r => r.ID == usuarios.RolID))
            {
                ModelState.AddModelError("RolID", "Debe seleccionar un rol valido.");
            }

            if (ModelState.IsValid)
            {
                usuarios.Contrasena =
                    BCrypt.Net.BCrypt.HashPassword(usuarios.Contrasena);

                db.usuarios.Add(usuarios);
                db.SaveChanges();

                RegistrarAuditoriaCreacionUsuario(
                    usuarios.ID,
                    usuarios.RolID);

                return esAdministrador
                    ? RedirectToAction("Index")
                    : RedirectToAction("Index", "Login");
            }

            PrepararFormularioCreacionUsuario(usuarios.RolID);
            return View(usuarios);
        }

        public ActionResult CambiarContrasena(int? id)
        {
            if (id == null)
            {
                return HttpNotFound();
            }

            var usuario = db.usuarios.Find(id);

            if (usuario == null)
            {
                return HttpNotFound();
            }

            var model = new CambiarContrasenaViewModel
            {
                UsuarioID = usuario.ID
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CambiarContrasena(CambiarContrasenaViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var usuario = db.usuarios.Find(model.UsuarioID);

            if (usuario == null)
            {
                return HttpNotFound();
            }

            if (!BCrypt.Net.BCrypt.Verify(
                    model.ContrasenaActual,
                    usuario.Contrasena))
            {
                ModelState.AddModelError(
                    "",
                    "La contraseña actual es incorrecta.");

                return View(model);
            }

            usuario.Contrasena =
                BCrypt.Net.BCrypt.HashPassword(
                    model.NuevaContrasena);

            db.SaveChanges();

            TempData["Success"] =
                "Contraseña actualizada correctamente.";

            return RedirectToAction("Edit",
                new { id = usuario.ID });
        }

        // GET: usuarios/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            usuarios usuarios = db.usuarios.Find(id);

            if (usuarios == null)
            {
                return HttpNotFound();
            }

            ViewBag.RolID = new SelectList(
                db.Roles,
                "ID",
                "Nombre",
                usuarios.RolID);

            ViewBag.EsAdministrador = EsAdministradorAutenticado();

            return View(usuarios);
        }

        // POST: usuarios/Edit/5
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que quiere enlazarse. Para obtener 
        // más detalles, vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ID,Nombre,Mail,RolID")] usuarios usuarios)
        {
            if (ModelState.IsValid)
            {
                var usuarioBD = db.usuarios.Find(usuarios.ID);

                if (usuarioBD == null)
                    return HttpNotFound();

                usuarioBD.Nombre = usuarios.Nombre;
                usuarioBD.Mail = usuarios.Mail;
                usuarioBD.RolID = usuarios.RolID;

                db.SaveChanges();

                return RedirectToAction("Index");
            }

            ViewBag.RolID = new SelectList(db.Roles, "ID", "Nombre", usuarios.RolID);
            return View(usuarios);
        }

        // GET: usuarios/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("Index");
            }
            usuarios usuarios = db.usuarios.Find(id);
            if (usuarios == null)
            {
                return HttpNotFound();
            }
            return View(usuarios);
        }

        // POST: usuarios/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            usuarios usuarios = db.usuarios.Find(id);
            db.usuarios.Remove(usuarios);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private void PrepararFormularioCreacionUsuario(int? rolSeleccionado = null)
        {
            var puedeSeleccionarRol = EsAdministradorAutenticado();
            ViewBag.PuedeSeleccionarRol = puedeSeleccionarRol;

            if (puedeSeleccionarRol)
            {
                ViewBag.RolID = new SelectList(db.Roles.OrderBy(r => r.Nombre), "ID", "Nombre", rolSeleccionado);
            }
        }

        private bool EsAdministradorAutenticado()
        {
            if (Session["RolID"] == null)
            {
                return false;
            }

            var rolId = Convert.ToInt32(Session["RolID"]);
            return db.Roles.Any(r => r.ID == rolId && r.Nombre == "Administrador");
        }

        private Roles ObtenerRolUsuario()
        {
            return db.Roles.FirstOrDefault(r => r.Nombre == "Usuario");
        }

        private void RegistrarAuditoriaCreacionUsuario(int usuarioCreadoId, int rolAsignadoId)
        {
            AsegurarTablaAuditoriaUsuarios();

            var usuarioCreadorId = Session["UsuarioID"] == null ? (int?)null : (int)Session["UsuarioID"];
            var nombreCreador = Session["Nombre"] == null ? "Sin sesion" : Session["Nombre"].ToString();
            var rolAsignado = db.Roles.FirstOrDefault(r => r.ID == rolAsignadoId);

            db.Database.ExecuteSqlCommand(
                @"INSERT INTO auditoriaUsuarios (UsuarioCreadorID, NombreCreador, UsuarioCreadoID, RolAsignadoID, RolAsignadoNombre, Fecha, DireccionIP)
                  VALUES (@usuarioCreadorId, @nombreCreador, @usuarioCreadoId, @rolAsignadoId, @rolAsignadoNombre, GETDATE(), @direccionIp)",
                new SqlParameter("@usuarioCreadorId", (object)usuarioCreadorId ?? DBNull.Value),
                new SqlParameter("@nombreCreador", nombreCreador),
                new SqlParameter("@usuarioCreadoId", usuarioCreadoId),
                new SqlParameter("@rolAsignadoId", rolAsignadoId),
                new SqlParameter("@rolAsignadoNombre", rolAsignado != null ? rolAsignado.Nombre : string.Empty),
                new SqlParameter("@direccionIp", Request.UserHostAddress ?? string.Empty));
        }

        private void AsegurarTablaAuditoriaUsuarios()
        {
            db.Database.ExecuteSqlCommand(@"
IF OBJECT_ID('dbo.auditoriaUsuarios', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.auditoriaUsuarios (
        ID INT IDENTITY(1,1) PRIMARY KEY,
        UsuarioCreadorID INT NULL,
        NombreCreador VARCHAR(100) NOT NULL,
        UsuarioCreadoID INT NOT NULL,
        RolAsignadoID INT NOT NULL,
        RolAsignadoNombre VARCHAR(50) NOT NULL,
        Fecha DATETIME NOT NULL DEFAULT GETDATE(),
        DireccionIP VARCHAR(50) NULL
    )
END");
        }
    }
}
