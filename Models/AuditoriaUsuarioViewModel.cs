using System;

namespace RedFlix.Models
{
    public class AuditoriaViewModel
    {
        public System.Collections.Generic.List<AuditoriaUsuarioViewModel> Usuarios { get; set; }
        public System.Collections.Generic.List<AuditoriaPermisoRolViewModel> PermisosRoles { get; set; }
    }

    public class AuditoriaUsuarioViewModel
    {
        public int ID { get; set; }
        public int? UsuarioCreadorID { get; set; }
        public string NombreCreador { get; set; }
        public int UsuarioCreadoID { get; set; }
        public int RolAsignadoID { get; set; }
        public string RolAsignadoNombre { get; set; }
        public DateTime Fecha { get; set; }
        public string DireccionIP { get; set; }
    }

    public class AuditoriaPermisoRolViewModel
    {
        public int ID { get; set; }
        public int? UsuarioCreadorID { get; set; }
        public string NombreCreador { get; set; }
        public int RolID { get; set; }
        public string RolNombre { get; set; }
        public int PermisoID { get; set; }
        public string PermisoNombre { get; set; }
        public string Accion { get; set; }
        public DateTime Fecha { get; set; }
        public string DireccionIP { get; set; }
    }
}
