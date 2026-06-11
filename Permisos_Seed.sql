USE RedFlixIII;
GO

-- Catálogo de permisos por entidad
INSERT INTO permisos (Nombre)
SELECT p.Nombre
FROM (VALUES
    ('Roles.Ver'), ('Roles.Crear'), ('Roles.Editar'), ('Roles.Eliminar'),
    ('Permisos.Ver'), ('Permisos.Crear'), ('Permisos.Editar'), ('Permisos.Eliminar'),
    ('Perfiles.Ver'), ('Perfiles.Crear'), ('Perfiles.Editar'), ('Perfiles.Eliminar'),
    ('Usuarios.Ver'), ('Usuarios.Crear'), ('Usuarios.Editar'), ('Usuarios.Eliminar'),
    ('Listas.Ver'), ('Listas.Crear'), ('Listas.Editar'), ('Listas.Eliminar'),
    ('Favoritos.Ver'), ('Favoritos.Crear'), ('Favoritos.Editar'), ('Favoritos.Eliminar'),
    ('Peliculas.Ver'),
    ('Series.Ver'),
    ('Auditoria.Ver')
) AS p(Nombre)
WHERE NOT EXISTS (SELECT 1 FROM permisos x WHERE x.Nombre = p.Nombre);
GO

-- Rol Administrador con todos los permisos
IF NOT EXISTS (SELECT 1 FROM Roles WHERE Nombre = 'Administrador')
    INSERT INTO Roles (Nombre) VALUES ('Administrador');

IF NOT EXISTS (SELECT 1 FROM Roles WHERE Nombre = 'Usuario')
    INSERT INTO Roles (Nombre) VALUES ('Usuario');
GO

DECLARE @AdminId INT = (SELECT ID FROM Roles WHERE Nombre = 'Administrador');
DECLARE @UserId INT = (SELECT ID FROM Roles WHERE Nombre = 'Usuario');

INSERT INTO rolesPermisos (RolID, PermisoID)
SELECT @AdminId, p.ID
FROM permisos p
WHERE NOT EXISTS (
    SELECT 1 FROM rolesPermisos rp
    WHERE rp.RolID = @AdminId AND rp.PermisoID = p.ID
);
GO

INSERT INTO rolesPermisos (RolID, PermisoID)
SELECT @UserId, p.ID
FROM permisos p
WHERE p.Nombre IN ('Peliculas.Ver', 'Series.Ver')
AND NOT EXISTS (
    SELECT 1 FROM rolesPermisos rp
    WHERE rp.RolID = @UserId AND rp.PermisoID = p.ID
);
GO
