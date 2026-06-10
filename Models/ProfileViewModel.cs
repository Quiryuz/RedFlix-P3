using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace RedFlix.Models
{
    public class ProfileViewModel
    {
        public int ID { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio")]
        [StringLength(50)]
        public string Nombre { get; set; }

        [Display(Name = "Icono")]
        public string Icono { get; set; }

        [Display(Name = "Contrasena del perfil")]
        public string ContrasenaPerfil { get; set; }

        public int UsuarioID { get; set; }
        public string UsuarioNombre { get; set; }
        public string UsuarioMail { get; set; }

        public IEnumerable<string> IconosDisponibles { get; set; }
    }

    public class ContentItemViewModel
    {
        public int TmdbId { get; set; }
        public string Tipo { get; set; }
        public string Titulo { get; set; }
        public string PosterPath { get; set; }
        public string Fecha { get; set; }
        public double Puntaje { get; set; }
    }

    public class UserListViewModel
    {
        public int ID { get; set; }
        public string Nombre { get; set; }
        public int PerfilID { get; set; }
        public string PerfilNombre { get; set; }
        public List<ContentItemViewModel> Contenidos { get; set; }
    }

    public class CalificacionPerfilViewModel
    {
        public ContentItemViewModel Contenido { get; set; }
        public int PuntajePersonal { get; set; }
        public System.DateTime FechaCalificacion { get; set; }
    }
}
