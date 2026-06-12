using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using RedFlix.Authorization;

namespace RedFlix.Helpers
{
    public static class PermissionHelper
    {
        private const string SessionKey = "Permisos";

        public static void SetUserPermissions(HttpSessionStateBase session, IEnumerable<string> permisos)
        {
            session[SessionKey] = permisos?.Distinct().ToList() ?? new List<string>();
        }

        public static List<string> GetUserPermissions(HttpSessionStateBase session)
        {
            return session[SessionKey] as List<string> ?? new List<string>();
        }

        public static bool TienePermiso(HttpSessionStateBase session, string entidad, string accion)
        {
            if (IsExempt(session, entidad, accion))
            {
                return true;
            }

            var permiso = PermissionKeys.Build(entidad, accion);
            var permisos = GetUserPermissions(session);
            return permisos.Contains(permiso);
        }

        public static bool PuedeVerModulo(HttpSessionStateBase session, string entidad)
        {
            return TienePermiso(session, entidad, PermissionKeys.Ver);
        }

        public static bool IsExempt(HttpSessionStateBase session, string entidad, string accion)
        {
            var usuarioId = session["UsuarioID"];

            if (usuarioId == null)
            {
                if (entidad == PermissionKeys.Usuarios && accion == PermissionKeys.Crear)
                {
                    return true;
                }

                if ((entidad == PermissionKeys.Peliculas || entidad == PermissionKeys.Series)
                    && accion == PermissionKeys.Ver)
                {
                    return true;
                }

                return false;
            }

            return false;
        }

        public static string MapMvcActionToPermission(string actionName)
        {
            switch (actionName)
            {
                case "Index":
                case "Details":
                case "Detalle":
                case "Tendencias":
                case "Buscar":
                    return PermissionKeys.Ver;
                case "Create":
                    return PermissionKeys.Crear;
                case "Edit":
                    return PermissionKeys.Editar;
                case "Delete":
                case "DeleteConfirmed":
                    return PermissionKeys.Eliminar;
                default:
                    return PermissionKeys.Ver;
            }
        }

        public static string MapControllerToEntity(string controllerName)
        {
            switch (controllerName)
            {
                case "Roles": return PermissionKeys.Roles;
                case "permisos": return PermissionKeys.Permisos;
                case "perfiles": return PermissionKeys.Perfiles;
                case "usuarios": return PermissionKeys.Usuarios;
                case "listas": return PermissionKeys.Listas;
                case "listaContenidoes": return PermissionKeys.Listas;
                case "favoritos": return PermissionKeys.Favoritos;
                case "Peliculas": return PermissionKeys.Peliculas;
                case "Series": return PermissionKeys.Series;
                case "Auditoria": return PermissionKeys.Auditoria;
                default: return controllerName;
            }
        }
    }
}
