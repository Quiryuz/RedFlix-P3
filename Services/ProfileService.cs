using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Web.Mvc;
using System.Data.Entity.Core.EntityClient;
using RedFlix.Models;

namespace RedFlix.Services
{
    public class ProfileService
    {
        private const string ColumnaContrasena = "ContrasenaPerfil";

        public static readonly string[] IconosPredeterminados =
        {
            "red",
            "blue",
            "green",
            "purple",
            "orange"
        };

        public void AsegurarColumnaContrasenaPerfil()
        {
            using (var conexion = new SqlConnection(ObtenerCadenaConexionProveedor()))
            using (var comando = conexion.CreateCommand())
            {
                comando.CommandText = @"
IF COL_LENGTH('dbo.perfiles', 'ContrasenaPerfil') IS NULL
BEGIN
    ALTER TABLE dbo.perfiles ADD ContrasenaPerfil VARCHAR(255) NULL
END";
                conexion.Open();
                comando.ExecuteNonQuery();
            }
        }

        public string ObtenerContrasenaPerfil(int perfilId)
        {
            AsegurarColumnaContrasenaPerfil();

            using (var conexion = new SqlConnection(ObtenerCadenaConexionProveedor()))
            using (var comando = conexion.CreateCommand())
            {
                comando.CommandText = "SELECT ContrasenaPerfil FROM dbo.perfiles WHERE ID = @id";
                comando.Parameters.AddWithValue("@id", perfilId);
                conexion.Open();

                var valor = comando.ExecuteScalar();
                return valor == null || valor == System.DBNull.Value ? string.Empty : valor.ToString();
            }
        }

        public void GuardarContrasenaPerfil(int perfilId, string contrasena)
        {
            AsegurarColumnaContrasenaPerfil();

            using (var conexion = new SqlConnection(ObtenerCadenaConexionProveedor()))
            using (var comando = conexion.CreateCommand())
            {
                comando.CommandText = "UPDATE dbo.perfiles SET ContrasenaPerfil = @password WHERE ID = @id";
                comando.Parameters.AddWithValue("@id", perfilId);
                comando.Parameters.AddWithValue("@password", string.IsNullOrWhiteSpace(contrasena) ? (object)System.DBNull.Value : contrasena);
                conexion.Open();
                comando.ExecuteNonQuery();
            }
        }

        public ProfileViewModel ConvertirAViewModel(perfiles perfil, string contrasena = null)
        {
            return new ProfileViewModel
            {
                ID = perfil.ID,
                Nombre = perfil.Nombre,
                Icono = string.IsNullOrWhiteSpace(perfil.Icono) ? IconosPredeterminados[0] : perfil.Icono,
                ContrasenaPerfil = contrasena ?? ObtenerContrasenaPerfil(perfil.ID),
                UsuarioID = perfil.usuarioID,
                UsuarioNombre = perfil.usuarios != null ? perfil.usuarios.Nombre : null,
                UsuarioMail = perfil.usuarios != null ? perfil.usuarios.Mail : null,
                IconosDisponibles = IconosPredeterminados
            };
        }

        public SelectList ObtenerSelectListIconos(string iconoSeleccionado = null)
        {
            var items = IconosPredeterminados.Select(icono => new SelectListItem
            {
                Text = ObtenerNombreIcono(icono),
                Value = icono,
                Selected = icono == iconoSeleccionado
            });

            return new SelectList(items, "Value", "Text", iconoSeleccionado);
        }

        public static string ObtenerNombreIcono(string icono)
        {
            switch (icono)
            {
                case "red": return "Rojo";
                case "blue": return "Azul";
                case "green": return "Verde";
                case "purple": return "Violeta";
                case "orange": return "Naranja";
                default: return icono;
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
