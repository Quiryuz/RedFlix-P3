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
        private readonly RedFlixIIIEntities _baseDatos = new RedFlixIIIEntities();

        public void AsegurarCatalogoPermisos()
        {
            var existentes = _baseDatos.permisos.Select(p => p.Nombre).ToList();
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
                _baseDatos.permisos.AddRange(faltantes);
                _baseDatos.SaveChanges();
            }

            AsegurarPermisosRolesBase();
        }

        public List<string> ObtenerNombresPermisosPorRol(int rolId)
        {
            var rol = _baseDatos.Roles.Include(r => r.permisos).FirstOrDefault(r => r.ID == rolId);
            if (rol == null)
            {
                return new List<string>();
            }

            return rol.permisos.Select(p => p.Nombre).ToList();
        }

        public List<PermissionGroupViewModel> ConstruirGruposPermisos(int? rolId = null)
        {
            var todos = _baseDatos.permisos.OrderBy(p => p.Nombre).ToList();
            var asignados = new HashSet<string>();

            if (rolId.HasValue)
            {
                asignados = new HashSet<string>(ObtenerNombresPermisosPorRol(rolId.Value));
            }

            var grupos = new List<PermissionGroupViewModel>();

            foreach (var entidad in PermissionKeys.PermisosPorEntidad)
            {
                var grupo = new PermissionGroupViewModel
                {
                    Entidad = entidad.Key,
                    EntidadDisplay = ObtenerNombreEntidad(entidad.Key),
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

        public void AsignarPermisosARol(int rolId, IEnumerable<int> permisoIds)
        {
            var rol = _baseDatos.Roles.Include(r => r.permisos).FirstOrDefault(r => r.ID == rolId);
            if (rol == null)
            {
                return;
            }

            var seleccionados = _baseDatos.permisos.Where(p => permisoIds.Contains(p.ID)).ToList();
            rol.permisos.Clear();

            foreach (var permiso in seleccionados)
            {
                rol.permisos.Add(permiso);
            }

            _baseDatos.SaveChanges();
        }

        private void AsegurarPermisosRolesBase()
        {
            var admin = _baseDatos.Roles.Include(r => r.permisos).FirstOrDefault(r => r.Nombre == "Administrador");
            if (admin == null)
            {
                admin = new Roles { Nombre = "Administrador" };
                _baseDatos.Roles.Add(admin);
                _baseDatos.SaveChanges();
                admin = _baseDatos.Roles.Include(r => r.permisos).First(r => r.ID == admin.ID);
            }

            var todos = _baseDatos.permisos.ToList();

            if (!admin.permisos.Any())
            {
                foreach (var permiso in todos)
                {
                    admin.permisos.Add(permiso);
                }
            }

            var usuario = _baseDatos.Roles.Include(r => r.permisos).FirstOrDefault(r => r.Nombre == "Usuario");
            if (usuario == null)
            {
                usuario = new Roles { Nombre = "Usuario" };
                _baseDatos.Roles.Add(usuario);
                _baseDatos.SaveChanges();
                usuario = _baseDatos.Roles.Include(r => r.permisos).First(r => r.ID == usuario.ID);
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

            _baseDatos.SaveChanges();
        }

        private static string ObtenerNombreEntidad(string entidad)
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
