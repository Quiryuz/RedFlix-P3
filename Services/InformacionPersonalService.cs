using System;
using System.Configuration;
using System.Data;
using System.Data.Entity.Core.EntityClient;
using System.Data.SqlClient;
using RedFlix.Models;

namespace RedFlix.Services
{
    public class InformacionPersonalService
    {
        public void AsegurarDatosCuenta()
        {
            using (var conexion = new SqlConnection(ObtenerCadenaConexionProveedor()))
            using (var comando = conexion.CreateCommand())
            {
                comando.CommandText = @"
IF COL_LENGTH('dbo.usuarios', 'NombreUsuario') IS NULL
BEGIN
    ALTER TABLE dbo.usuarios ADD NombreUsuario VARCHAR(50) NULL
END

IF COL_LENGTH('dbo.usuarios', 'FechaRegistro') IS NULL
BEGIN
    ALTER TABLE dbo.usuarios ADD FechaRegistro DATETIME NULL
END

IF COL_LENGTH('dbo.usuarios', 'EstadoCuenta') IS NULL
BEGIN
    ALTER TABLE dbo.usuarios ADD EstadoCuenta VARCHAR(20) NULL
END

IF COL_LENGTH('dbo.usuarios', 'FotoPerfil') IS NULL
BEGIN
    ALTER TABLE dbo.usuarios ADD FotoPerfil VARCHAR(255) NULL
END

EXEC('UPDATE dbo.usuarios
      SET NombreUsuario = COALESCE(NombreUsuario, LEFT(Mail, CHARINDEX(''@'', Mail + ''@'') - 1)),
          FechaRegistro = COALESCE(FechaRegistro, GETDATE()),
          EstadoCuenta = COALESCE(EstadoCuenta, ''Activa'')')";

                conexion.Open();
                comando.ExecuteNonQuery();
            }
        }

        public InformacionPersonalViewModel ObtenerInformacionPersonal(int usuarioId, int perfilId)
        {
            AsegurarDatosCuenta();

            using (var conexion = new SqlConnection(ObtenerCadenaConexionProveedor()))
            using (var comando = conexion.CreateCommand())
            {
                comando.CommandText = @"
SELECT u.ID AS UsuarioID,
       u.Nombre AS NombreCompleto,
       u.NombreUsuario,
       u.Mail AS CorreoElectronico,
       u.FechaRegistro,
       u.EstadoCuenta,
       u.FotoPerfil,
       r.Nombre AS RolUsuario,
       p.Icono AS IconoPerfil
FROM dbo.usuarios u
INNER JOIN dbo.Roles r ON r.ID = u.RolID
LEFT JOIN dbo.perfiles p ON p.ID = @perfilId
WHERE u.ID = @usuarioId";

                comando.Parameters.Add("@usuarioId", SqlDbType.Int).Value = usuarioId;
                comando.Parameters.Add("@perfilId", SqlDbType.Int).Value = perfilId;

                conexion.Open();
                using (var lector = comando.ExecuteReader())
                {
                    if (!lector.Read())
                    {
                        return null;
                    }

                    return new InformacionPersonalViewModel
                    {
                        UsuarioID = Convert.ToInt32(lector["UsuarioID"]),
                        FotoPerfil = lector["FotoPerfil"] == DBNull.Value ? null : lector["FotoPerfil"].ToString(),
                        IconoPerfil = lector["IconoPerfil"] == DBNull.Value ? "red" : lector["IconoPerfil"].ToString(),
                        NombreCompleto = lector["NombreCompleto"].ToString(),
                        NombreUsuario = lector["NombreUsuario"] == DBNull.Value ? string.Empty : lector["NombreUsuario"].ToString(),
                        CorreoElectronico = lector["CorreoElectronico"].ToString(),
                        FechaRegistro = Convert.ToDateTime(lector["FechaRegistro"]),
                        RolUsuario = lector["RolUsuario"].ToString(),
                        EstadoCuenta = lector["EstadoCuenta"] == DBNull.Value ? "Activa" : lector["EstadoCuenta"].ToString()
                    };
                }
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
