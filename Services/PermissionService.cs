using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using RedFlix.Authorization;
using RedFlix.Models.Authorization;
using RedFlix.Services;

namespace RedFlix.Services
{
    public class PermissionService
    {
        private readonly RedFlixIIIEntities _db = new RedFlixIIIEntities();

        public void EnsurePermissionCatalog()
        {
            var existentes = _db.permisos.Select(p => p.Nombre).ToList();
            var faltantes = new List<permisos>();

            foreach (var entidad in PermissionKeys.PermisosPorEntidad)
            {
                foreach (var accion in entidad.Value)
                {
                    var nombre = PermissionKeys.Build(entidad.Key, accion);
                    if (!existentes.Contains(nombre))
                    {
                        faltantes.Add(new permisos { Nombre = nombre });
                    }
                }
            }

            if (faltantes.Any())
            {
                _db.permisos.AddRange(faltantes);
                _db.SaveChanges();
            }

            EnsureDefaultRoleAssignments();
        }

        public List<string> GetPermissionNamesForRole(int rolId)
        {
            var rol = _db.Roles.Include(r => r.permisos).FirstOrDefault(r => r.ID == rolId);
            if (rol == null)
            {
                return new List<string>();
            }

            return rol.permisos.Select(p => p.Nombre).ToList();
        }

        public List<PermissionGroupViewModel> BuildPermissionGroups(int? rolId = null)
        {
            var todos = _db.permisos.OrderBy(p => p.Nombre).ToList();
            var asignados = new HashSet<string>();

            if (rolId.HasValue)
            {
                asignados = new HashSet<string>(GetPermissionNamesForRole(rolId.Value));
            }

            var grupos = new List<PermissionGroupViewModel>();

            foreach (var entidad in PermissionKeys.PermisosPorEntidad)
            {
                var grupo = new PermissionGroupViewModel
                {
                    Entidad = entidad.Key,
                    EntidadDisplay = GetEntityDisplayName(entidad.Key),
                    Permisos = new List<PermissionItemViewModel>()
                };

                foreach (var accion in entidad.Value)
                {
                    var nombre = PermissionKeys.Build(entidad.Key, accion);
                    var permiso = todos.FirstOrDefault(p => p.Nombre == nombre);
                    if (permiso == null)
                    {
                        continue;
                    }

                    grupo.Permisos.Add(new PermissionItemViewModel
                    {
                        Id = permiso.ID,
                        Nombre = permiso.Nombre,
                        Accion = accion,
                        Asignado = asignados.Contains(permiso.Nombre)
                    });
                }

                if (grupo.Permisos.Any())
                {
                    grupos.Add(grupo);
                }
            }

            return grupos;
        }

        public void AssignPermissionsToRole(int rolId, IEnumerable<int> permisoIds)
        {
            var rol = _db.Roles.Include(r => r.permisos).FirstOrDefault(r => r.ID == rolId);
            if (rol == null)
            {
                return;
            }

            var seleccionados = _db.permisos.Where(p => permisoIds.Contains(p.ID)).ToList();
            rol.permisos.Clear();

            foreach (var permiso in seleccionados)
            {
                rol.permisos.Add(permiso);
            }

            _db.SaveChanges();
        }

        private void EnsureDefaultRoleAssignments()
        {
            var admin = _db.Roles.Include(r => r.permisos).FirstOrDefault(r => r.Nombre == "Administrador");
            if (admin == null)
            {
                admin = new Roles { Nombre = "Administrador" };
                _db.Roles.Add(admin);
                _db.SaveChanges();
                admin = _db.Roles.Include(r => r.permisos).First(r => r.ID == admin.ID);
            }

            var todos = _db.permisos.ToList();

            if (!admin.permisos.Any())
            {
                foreach (var permiso in todos)
                {
                    admin.permisos.Add(permiso);
                }
            }

            var usuario = _db.Roles.Include(r => r.permisos).FirstOrDefault(r => r.Nombre == "Usuario");
            if (usuario == null)
            {
                usuario = new Roles { Nombre = "Usuario" };
                _db.Roles.Add(usuario);
                _db.SaveChanges();
                usuario = _db.Roles.Include(r => r.permisos).First(r => r.ID == usuario.ID);
            }

            if (!usuario.permisos.Any())
            {
                var permisosUsuario = todos
                    .Where(p => p.Nombre == PermissionKeys.Build(PermissionKeys.Peliculas, PermissionKeys.Ver)
                             || p.Nombre == PermissionKeys.Build(PermissionKeys.Series, PermissionKeys.Ver))
                    .ToList();

                foreach (var permiso in permisosUsuario)
                {
                    usuario.permisos.Add(permiso);
                }
            }

            _db.SaveChanges();
        }

        private static string GetEntityDisplayName(string entidad)
        {
            switch (entidad)
            {
                case PermissionKeys.Roles: return "Roles";
                case PermissionKeys.Permisos: return "Permisos";
                case PermissionKeys.Perfiles: return "Perfiles";
                case PermissionKeys.Usuarios: return "Usuarios";
                case PermissionKeys.Listas: return "Listas";
                case PermissionKeys.Favoritos: return "Favoritos";
                case PermissionKeys.Peliculas: return "Peliculas (API)";
                case PermissionKeys.Series: return "Series (API)";
                default: return entidad;
            }
        }
    }
}
