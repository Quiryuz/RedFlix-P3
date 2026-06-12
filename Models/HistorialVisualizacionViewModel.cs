using System;

namespace RedFlix.Models
{
    public class HistorialVisualizacionViewModel
    {
        public int ID { get; set; }
        public int PerfilID { get; set; }
        public int TmdbID { get; set; }
        public string Tipo { get; set; }
        public string Titulo { get; set; }
        public string Generos { get; set; }
        public decimal CalificacionTmdb { get; set; }
        public int? CalificacionPerfil { get; set; }
        public string PosterPath { get; set; }
        public DateTime FechaVisualizacion { get; set; }
    }
}
