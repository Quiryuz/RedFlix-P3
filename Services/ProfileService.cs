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
        private const string PasswordColumn = "ContrasenaPerfil";

        public static readonly string[] DefaultIcons =
        {
            "red",
            "blue",
            "green",
            "purple",
            "orange"
        };

        public void EnsureProfilePasswordColumn()
        {
            using (var connection = new SqlConnection(GetProviderConnectionString()))
            using (var command = connection.CreateCommand())
            {
                command.CommandText = @"
IF COL_LENGTH('dbo.perfiles', 'ContrasenaPerfil') IS NULL
BEGIN
    ALTER TABLE dbo.perfiles ADD ContrasenaPerfil VARCHAR(255) NULL
END";
                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        public string GetProfilePassword(int perfilId)
        {
            EnsureProfilePasswordColumn();

            using (var connection = new SqlConnection(GetProviderConnectionString()))
            using (var command = connection.CreateCommand())
            {
                command.CommandText = "SELECT ContrasenaPerfil FROM dbo.perfiles WHERE ID = @id";
                command.Parameters.AddWithValue("@id", perfilId);
                connection.Open();

                var value = command.ExecuteScalar();
                return value == null || value == System.DBNull.Value ? string.Empty : value.ToString();
            }
        }

        public void SaveProfilePassword(int perfilId, string password)
        {
            EnsureProfilePasswordColumn();

            using (var connection = new SqlConnection(GetProviderConnectionString()))
            using (var command = connection.CreateCommand())
            {
                command.CommandText = "UPDATE dbo.perfiles SET ContrasenaPerfil = @password WHERE ID = @id";
                command.Parameters.AddWithValue("@id", perfilId);
                command.Parameters.AddWithValue("@password", string.IsNullOrWhiteSpace(password) ? (object)System.DBNull.Value : password);
                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        public ProfileViewModel ToViewModel(perfiles perfil, string password = null)
        {
            return new ProfileViewModel
            {
                ID = perfil.ID,
                Nombre = perfil.Nombre,
                Icono = string.IsNullOrWhiteSpace(perfil.Icono) ? DefaultIcons[0] : perfil.Icono,
                ContrasenaPerfil = password ?? GetProfilePassword(perfil.ID),
                UsuarioID = perfil.usuarioID,
                UsuarioNombre = perfil.usuarios != null ? perfil.usuarios.Nombre : null,
                UsuarioMail = perfil.usuarios != null ? perfil.usuarios.Mail : null,
                IconosDisponibles = DefaultIcons
            };
        }

        public SelectList GetIconSelectList(string selectedIcon = null)
        {
            var items = DefaultIcons.Select(icon => new SelectListItem
            {
                Text = GetIconDisplayName(icon),
                Value = icon,
                Selected = icon == selectedIcon
            });

            return new SelectList(items, "Value", "Text", selectedIcon);
        }

        public static string GetIconDisplayName(string icon)
        {
            switch (icon)
            {
                case "red": return "Rojo";
                case "blue": return "Azul";
                case "green": return "Verde";
                case "purple": return "Violeta";
                case "orange": return "Naranja";
                default: return icon;
            }
        }

        private static string GetProviderConnectionString()
        {
            var entityConnection = ConfigurationManager.ConnectionStrings["RedFlixIIIEntities"].ConnectionString;
            var builder = new EntityConnectionStringBuilder(entityConnection);
            return builder.ProviderConnectionString;
        }
    }
}
