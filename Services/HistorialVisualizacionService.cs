using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Entity.Core.EntityClient;
using System.Data.SqlClient;
using RedFlix.Models;

namespace RedFlix.Services
{
    public class HistorialVisualizacionService
    {
        public void AsegurarTabla()
        {
            using (var conexion = new SqlConnection(ObtenerCadenaConexionProveedor()))
            using (var comando = conexion.CreateCommand())
            {
                comando.CommandText = @"
IF OBJECT_ID('dbo.historialVisualizaciones', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.historialVisualizaciones (
        ID INT IDENTITY(1,1) PRIMARY KEY,
        PerfilID INT NOT NULL,
        TmdbID INT NOT NULL,
        Tipo VARCHAR(50) NOT NULL,
        Titulo VARCHAR(255) NOT NULL,
        Generos VARCHAR(255) NULL,
        CalificacionTmdb DECIMAL(5,2) NOT NULL,
        CalificacionPerfil INT NULL,
        PosterPath VARCHAR(255) NULL,
        FechaVisualizacion DATETIME NOT NULL DEFAULT GETDATE()
    )
END";

                conexion.Open();
                comando.ExecuteNonQuery();
            }
        }

        public void RegistrarVisualizacion(int perfilId, int tmdbId, string tipo, string titulo, string generos, double calificacionTmdb, string posterPath)
        {
            AsegurarTabla();

            using (var conexion = new SqlConnection(ObtenerCadenaConexionProveedor()))
            using (var comando = conexion.CreateCommand())
            {
                comando.CommandText = @"
INSERT INTO dbo.historialVisualizaciones
    (PerfilID, TmdbID, Tipo, Titulo, Generos, CalificacionTmdb, CalificacionPerfil, PosterPath, FechaVisualizacion)
VALUES
    (@perfilId, @tmdbId, @tipo, @titulo, @generos, @calificacionTmdb,
     (SELECT TOP 1 puntaje FROM dbo.calificaciones WHERE perfilID = @perfilId AND tmdbID = @tmdbId AND tipo = @tipo),
     @posterPath, GETDATE())";

                comando.Parameters.Add("@perfilId", SqlDbType.Int).Value = perfilId;
                comando.Parameters.Add("@tmdbId", SqlDbType.Int).Value = tmdbId;
                comando.Parameters.Add("@tipo", SqlDbType.VarChar, 50).Value = tipo;
                comando.Parameters.Add("@titulo", SqlDbType.VarChar, 255).Value = titulo ?? string.Empty;
                comando.Parameters.Add("@generos", SqlDbType.VarChar, 255).Value = string.IsNullOrWhiteSpace(generos) ? (object)DBNull.Value : generos;
                comando.Parameters.Add("@calificacionTmdb", SqlDbType.Decimal).Value = Convert.ToDecimal(calificacionTmdb);
                comando.Parameters["@calificacionTmdb"].Precision = 5;
                comando.Parameters["@calificacionTmdb"].Scale = 2;
                comando.Parameters.Add("@posterPath", SqlDbType.VarChar, 255).Value = string.IsNullOrWhiteSpace(posterPath) ? (object)DBNull.Value : posterPath;

                conexion.Open();
                comando.ExecuteNonQuery();
            }
        }

        public List<HistorialVisualizacionViewModel> ObtenerHistorialPerfil(int perfilId)
        {
            AsegurarTabla();

            var historial = new List<HistorialVisualizacionViewModel>();

            using (var conexion = new SqlConnection(ObtenerCadenaConexionProveedor()))
            using (var comando = conexion.CreateCommand())
            {
                comando.CommandText = @"
SELECT ID, PerfilID, TmdbID, Tipo, Titulo, Generos, CalificacionTmdb, CalificacionPerfil, PosterPath, FechaVisualizacion
FROM dbo.historialVisualizaciones
WHERE PerfilID = @perfilId
ORDER BY FechaVisualizacion DESC";
                comando.Parameters.Add("@perfilId", SqlDbType.Int).Value = perfilId;

                conexion.Open();
                using (var lector = comando.ExecuteReader())
                {
                    while (lector.Read())
                    {
                        historial.Add(new HistorialVisualizacionViewModel
                        {
                            ID = Convert.ToInt32(lector["ID"]),
                            PerfilID = Convert.ToInt32(lector["PerfilID"]),
                            TmdbID = Convert.ToInt32(lector["TmdbID"]),
                            Tipo = lector["Tipo"].ToString(),
                            Titulo = lector["Titulo"].ToString(),
                            Generos = lector["Generos"] == DBNull.Value ? string.Empty : lector["Generos"].ToString(),
                            CalificacionTmdb = Convert.ToDecimal(lector["CalificacionTmdb"]),
                            CalificacionPerfil = lector["CalificacionPerfil"] == DBNull.Value ? (int?)null : Convert.ToInt32(lector["CalificacionPerfil"]),
                            PosterPath = lector["PosterPath"] == DBNull.Value ? null : lector["PosterPath"].ToString(),
                            FechaVisualizacion = Convert.ToDateTime(lector["FechaVisualizacion"])
                        });
                    }
                }
            }

            return historial;
        }

        public int ContarHistorialPerfil(int perfilId)
        {
            AsegurarTabla();

            using (var conexion = new SqlConnection(ObtenerCadenaConexionProveedor()))
            using (var comando = conexion.CreateCommand())
            {
                comando.CommandText = "SELECT COUNT(1) FROM dbo.historialVisualizaciones WHERE PerfilID = @perfilId";
                comando.Parameters.Add("@perfilId", SqlDbType.Int).Value = perfilId;

                conexion.Open();
                return Convert.ToInt32(comando.ExecuteScalar());
            }
        }

        public void LimpiarHistorialPerfil(int perfilId)
        {
            AsegurarTabla();

            using (var conexion = new SqlConnection(ObtenerCadenaConexionProveedor()))
            using (var comando = conexion.CreateCommand())
            {
                comando.CommandText = "DELETE FROM dbo.historialVisualizaciones WHERE PerfilID = @perfilId";
                comando.Parameters.Add("@perfilId", SqlDbType.Int).Value = perfilId;

                conexion.Open();
                comando.ExecuteNonQuery();
            }
        }

        private static string ObtenerCadenaConexionProveedor()
        {
            var conexionEntity = ConfigurationManager.ConnectionStrings["RedFlixIIIEntities"].ConnectionString;
            var constructor = new EntityConnectionStringBuilder(conexionEntity);
            return constructor.ProviderConnectionString;
        }
    }
}
