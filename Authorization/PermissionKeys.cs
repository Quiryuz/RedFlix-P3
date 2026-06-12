using System.Collections.Generic;

namespace RedFlix.Authorization
{
    public static class PermissionKeys
    {
        public const string Ver = "Ver";
        public const string Crear = "Crear";
        public const string Editar = "Editar";
        public const string Eliminar = "Eliminar";

        public const string Roles = "Roles";
        public const string Permisos = "Permisos";
        public const string Perfiles = "Perfiles";
        public const string Usuarios = "Usuarios";
        public const string Listas = "Listas";
        public const string Favoritos = "Favoritos";
        public const string Peliculas = "Peliculas";
        public const string Series = "Series";
        public const string Auditoria = "Auditoria";

        public static readonly string[] AccionesCrud = { Ver, Crear, Editar, Eliminar };

        public static readonly Dictionary<string, string[]> PermisosPorEntidad = new Dictionary<string, string[]>
        {
            { Roles, AccionesCrud },
            { Permisos, AccionesCrud },
            { Perfiles, AccionesCrud },
            { Usuarios, AccionesCrud },
            { Listas, AccionesCrud },
            { Favoritos, AccionesCrud },
            { Peliculas, new[] { Ver } },
            { Series, new[] { Ver } },
            { Auditoria, new[] { Ver } }
        };

        public static string Build(string entidad, string accion)
        {
            return entidad + "." + accion;
        }
    }
}
