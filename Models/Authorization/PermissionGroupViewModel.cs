using System.Collections.Generic;

namespace RedFlix.Models.Authorization
{
    public class PermissionGroupViewModel
    {
        public string Entidad { get; set; }
        public string EntidadDisplay { get; set; }
        public List<PermissionItemViewModel> Permisos { get; set; }
    }

    public class PermissionItemViewModel
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string Accion { get; set; }
        public bool Asignado { get; set; }
    }
}
