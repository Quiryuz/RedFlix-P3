using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using RedFlix.Authorization;
using RedFlix.Models;

namespace RedFlix.Controllers
{
    [AuthorizePermission(Entity = PermissionKeys.Auditoria)]
    public class AuditoriaController : Controller
    {
        private readonly RedFlixIIIEntities db = new RedFlixIIIEntities();

        public ActionResult Index()
        {
            AsegurarTablaAuditoriaUsuarios();
            AsegurarTablaAuditoriaPermisosRoles();

            var auditoriasUsuarios = db.Database.SqlQuery<AuditoriaUsuarioViewModel>(@"
SELECT ID,
       UsuarioCreadorID,
       NombreCreador,
       UsuarioCreadoID,
       RolAsignadoID,
       RolAsignadoNombre,
       Fecha,
       DireccionIP
FROM dbo.auditoriaUsuarios
ORDER BY Fecha DESC").ToList();

            var auditoriasPermisos = db.Database.SqlQuery<AuditoriaPermisoRolViewModel>(@"
SELECT ID,
       UsuarioCreadorID,
       NombreCreador,
       RolID,
       RolNombre,
       PermisoID,
       PermisoNombre,
       Accion,
       Fecha,
       DireccionIP
FROM dbo.auditoriaPermisosRoles
ORDER BY Fecha DESC").ToList();

            return View(new AuditoriaViewModel
            {
                Usuarios = auditoriasUsuarios,
                PermisosRoles = auditoriasPermisos
            });
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

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}
