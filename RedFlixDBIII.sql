create database RedFlixIII
use RedFlixIII

CREATE TABLE calificaciones (
    ID INT IDENTITY(1,1) PRIMARY KEY,
    perfilID INT NOT NULL,
    tmdbID INT NOT NULL,
    tipo VARCHAR(50) NOT NULL,
    puntaje INT NOT NULL CHECK (puntaje BETWEEN 1 AND 5),
    fechaCalificacion DATETIME NOT NULL DEFAULT GETDATE(),

    CONSTRAINT uq_calificacion
    UNIQUE (perfilID, tmdbID, tipo),

    CONSTRAINT fk_perfil_calificaciones
    FOREIGN KEY (perfilID)
    REFERENCES perfiles(ID)
)

CREATE TABLE clima (
    ID INT IDENTITY(1,1) PRIMARY KEY,
    fecha DATETIME NOT NULL,
    temperatura INT NOT NULL,
    descripcionClima VARCHAR(MAX) NOT NULL,
    icono VARCHAR(MAX) NULL
)

CREATE TABLE cotizaciones (
    ID INT IDENTITY(1,1) PRIMARY KEY,
    tipoMoneda VARCHAR(50) NOT NULL,
    valor DECIMAL(18,0) NOT NULL,
    fecha DATE NOT NULL
)

CREATE TABLE favoritos (
    ID INT IDENTITY(1,1) PRIMARY KEY,
    perfilID INT NOT NULL,
    tmdbID INT NOT NULL,
    tipo VARCHAR(50) NOT NULL,

    CONSTRAINT uq_favorito
    UNIQUE (perfilID, tmdbID, tipo),

    CONSTRAINT fk_perfil_favoritos
    FOREIGN KEY (perfilID)
    REFERENCES perfiles(ID)
)

CREATE TABLE listas (
    ID INT IDENTITY(1,1) PRIMARY KEY,
    nombre VARCHAR(50) NOT NULL,
    perfilID INT NOT NULL,

    CONSTRAINT fk_listas_perfil
    FOREIGN KEY (perfilID)
    REFERENCES perfiles(ID)
)

CREATE TABLE listaContenido (
    listaID INT NOT NULL,
    tmdbID INT NOT NULL,
    tipo VARCHAR(50) NOT NULL,

    PRIMARY KEY (listaID, tmdbID, tipo),

    CONSTRAINT fk_listaCont_listas
    FOREIGN KEY (listaID)
    REFERENCES listas(ID)
)

CREATE TABLE permisos (
    ID INT IDENTITY(1,1) PRIMARY KEY,
    Nombre VARCHAR(50) NOT NULL
)

CREATE TABLE Roles (
    ID INT IDENTITY(1,1) PRIMARY KEY,
    Nombre VARCHAR(50) NOT NULL
)

CREATE TABLE usuarios (
    ID INT IDENTITY(1,1) PRIMARY KEY,
    Nombre VARCHAR(50) NOT NULL,
    Mail VARCHAR(50) NOT NULL,
    RolID INT NOT NULL,

    CONSTRAINT fk_rol_usuario
    FOREIGN KEY (RolID)
    REFERENCES Roles(ID)
)

CREATE TABLE perfiles (
    ID INT IDENTITY(1,1) PRIMARY KEY,
    Nombre VARCHAR(50) NOT NULL,
    Icono VARCHAR(MAX) NULL,
    ContrasenaPerfil VARCHAR(255) NULL,
    usuarioID INT NOT NULL,

    CONSTRAINT fk_perfil_usuario
    FOREIGN KEY (usuarioID)
    REFERENCES usuarios(ID)
)

CREATE TABLE rolesPermisos (
    RolID INT NOT NULL,
    PermisoID INT NOT NULL,

    PRIMARY KEY (RolID, PermisoID),

    CONSTRAINT fk_rol_permiso
    FOREIGN KEY (RolID)
    REFERENCES Roles(ID),

    CONSTRAINT fk_permiso_rol
    FOREIGN KEY (PermisoID)
    REFERENCES permisos(ID)
)

CREATE TABLE auditoriaUsuarios (
    ID INT IDENTITY(1,1) PRIMARY KEY,
    UsuarioCreadorID INT NULL,
    NombreCreador VARCHAR(100) NOT NULL,
    UsuarioCreadoID INT NOT NULL,
    RolAsignadoID INT NOT NULL,
    RolAsignadoNombre VARCHAR(50) NOT NULL,
    Fecha DATETIME NOT NULL DEFAULT GETDATE(),
    DireccionIP VARCHAR(50) NULL
)

ALTER TABLE usuarios
ADD CONSTRAINT uq_usuario_mail
UNIQUE (Mail)

ALTER TABLE Roles
ADD CONSTRAINT uq_rol_nombre
UNIQUE (Nombre)

ALTER TABLE permisos
ADD CONSTRAINT uq_permiso_nombre
UNIQUE (Nombre)


ALTER TABLE perfiles
ADD CONSTRAINT uq_perfil_usuario
UNIQUE (usuarioID, Nombre)

ALTER TABLE favoritos
ADD CONSTRAINT ck_favoritos_tipo
CHECK (tipo IN ('Pelicula', 'Serie'))

ALTER TABLE calificaciones
ADD CONSTRAINT ck_calificaciones_tipo
CHECK (tipo IN ('Pelicula', 'Serie'))

ALTER TABLE listaContenido
ADD CONSTRAINT ck_listaContenido_tipo
CHECK (tipo IN ('Pelicula', 'Serie'))

ALTER TABLE usuarios
ADD Contrasena VARCHAR(255) NOT NULL
DEFAULT ''

ALTER TABLE cotizaciones
ALTER COLUMN valor DECIMAL(18,4) NOT NULL

IF COL_LENGTH('dbo.perfiles', 'ContrasenaPerfil') IS NULL
BEGIN
    ALTER TABLE dbo.perfiles ADD ContrasenaPerfil VARCHAR(255) NULL
END