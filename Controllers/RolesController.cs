using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using RedFlix;
using RedFlix.Authorization;
using RedFlix.Helpers;
using RedFlix.Services;

namespace RedFlix.Controllers
{
    [AuthorizePermission(Entity = PermissionKeys.Roles)]
    public class RolesController : Controller
    {
        private readonly RedFlixIIIEntities db = new RedFlixIIIEntities();
        private readonly PermissionService _servicioPermisos = new PermissionService();

        public ActionResult Index()
        {
            return View(db.Roles.ToList());
        }

        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Roles roles = db.Roles.Find(id);
            if (roles == null)
            {
                return HttpNotFound();
            }

            _servicioPermisos.AsegurarCatalogoPermisos();
            ViewBag.PermissionGroups = _servicioPermisos.ConstruirGruposPermisos(id);
            return View(roles);
        }

        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ID,Nombre")] Roles roles)
        {
            if (ModelState.IsValid)
            {
                db.Roles.Add(roles);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(roles);
        }

        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Roles roles = db.Roles.Find(id);
            if (roles == null)
            {
                return HttpNotFound();
            }

            _servicioPermisos.AsegurarCatalogoPermisos();
            ViewBag.PermissionGroups = _servicioPermisos.ConstruirGruposPermisos(id);
            return View(roles);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ID,Nombre")] Roles roles, int[] permisoIds)
        {
            if (ModelState.IsValid)
            {
                var permisosAnteriores = ObtenerPermisosRol(roles.ID);
                var permisosNuevos = new HashSet<int>(permisoIds ?? new int[0]);
                var nombreRol = db.Roles
                    .Where(r => r.ID == roles.ID)
                    .Select(r => r.Nombre)
                    .FirstOrDefault() ?? roles.Nombre;

                db.Entry(roles).State = EntityState.Modified;
                db.SaveChanges();
                _servicioPermisos.AsignarPermisosARol(roles.ID, permisoIds ?? new int[0]);
                RegistrarAuditoriaPermisosRol(roles.ID, nombreRol, permisosAnteriores, permisosNuevos);

                if (Session["RolID"] != null && Convert.ToInt32(Session["RolID"]) == roles.ID)
                {
                    PermissionHelper.SetUserPermissions(
                        Session,
                        _servicioPermisos.ObtenerNombresPermisosPorRol(roles.ID));
                }

                return RedirectToAction("Index");
            }

            ViewBag.PermissionGroups = _servicioPermisos.ConstruirGruposPermisos(roles.ID);
            return View(roles);
        }

        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Roles roles = db.Roles.Find(id);
            if (roles == null)
            {
                return HttpNotFound();
            }

            return View(roles);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Roles roles = db.Roles.Find(id);
            db.Roles.Remove(roles);
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

        private HashSet<int> ObtenerPermisosRol(int rolId)
        {
            var permisos = db.Database.SqlQuery<int>(
                "SELECT PermisoID FROM rolesPermisos WHERE RolID = @rolId",
                new SqlParameter("@rolId", rolId)).ToList();

            return new HashSet<int>(permisos);
        }

        private void RegistrarAuditoriaPermisosRol(int rolId, string rolNombre, HashSet<int> permisosAnteriores, HashSet<int> permisosNuevos)
        {
            AsegurarTablaAuditoriaPermisosRoles();

            var agregados = permisosNuevos.Except(permisosAnteriores).ToList();
            var quitados = permisosAnteriores.Except(permisosNuevos).ToList();

            RegistrarCambiosPermisos(rolId, rolNombre, agregados, "Agregado");
            RegistrarCambiosPermisos(rolId, rolNombre, quitados, "Quitado");
        }

        private void RegistrarCambiosPermisos(int rolId, string rolNombre, IEnumerable<int> permisoIds, string accion)
        {
            var usuarioCreadorId = Session["UsuarioID"] == null ? (int?)null : Convert.ToInt32(Session["UsuarioID"]);
            var nombreCreador = Session["Nombre"] == null ? "Sin sesion" : Session["Nombre"].ToString();
            var direccionIp = Request.UserHostAddress ?? string.Empty;

            foreach (var permisoId in permisoIds)
            {
                var permiso = db.permisos.FirstOrDefault(p => p.ID == permisoId);
                if (permiso == null)
                {
                    continue;
                }

                db.Database.ExecuteSqlCommand(
                    @"INSERT INTO auditoriaPermisosRoles (UsuarioCreadorID, NombreCreador, RolID, RolNombre, PermisoID, PermisoNombre, Accion, Fecha, DireccionIP)
                      VALUES (@usuarioCreadorId, @nombreCreador, @rolId, @rolNombre, @permisoId, @permisoNombre, @accion, GETDATE(), @direccionIp)",
                    new SqlParameter("@usuarioCreadorId", (object)usuarioCreadorId ?? DBNull.Value),
                    new SqlParameter("@nombreCreador", nombreCreador),
                    new SqlParameter("@rolId", rolId),
                    new SqlParameter("@rolNombre", rolNombre),
                    new SqlParameter("@permisoId", permisoId),
                    new SqlParameter("@permisoNombre", permiso.Nombre),
                    new SqlParameter("@accion", accion),
                    new SqlParameter("@direccionIp", direccionIp));
            }
        }

        private void AsegurarTablaAuditoriaPermisosRoles()
        {
            db.Database.ExecuteSqlCommand(@"
IF OBJECT_ID('dbo.auditoriaPermisosRoles', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.auditoriaPermisosRoles (
        ID INT IDENTITY(1,1) PRIMARY KEY,
        UsuarioCreadorID INT NULL,
        NombreCreador VARCHAR(100) NOT NULL,
        RolID INT NOT NULL,
        RolNombre VARCHAR(50) NOT NULL,
        PermisoID INT NOT NULL,
        PermisoNombre VARCHAR(100) NOT NULL,
        Accion VARCHAR(20) NOT NULL,
        Fecha DATETIME NOT NULL DEFAULT GETDATE(),
        DireccionIP VARCHAR(50) NULL
    )
END");
        }
    }
}
