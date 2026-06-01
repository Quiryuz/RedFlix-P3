create database RedFlix	
use RedFlix

create table peliculas(
ID int primary key identity (1,1) NOT NULL,
Titulo varchar (50) NOT NULL,
Duracion time NOT NULL,
FechaDeEstreno date NOT NULL,
Idioma varchar (50) NOT NULL,
Clasificacion varchar (10) NOT NULL,
Sinopsis varchar (max) NULL
)

create table series(
ID int primary key identity (1,1) NOT NULL,
Titulo varchar (50) NOT NULL,
CantidadCapitulos int NOT NULL,
FechaDeEstreno date NOT NULL,
Idioma varchar (50) NOT NULL,
Clasificacion varchar (10) NOT NULL,
Sinopsis varchar (max) NULL,
)

create table usuarios(
ID int primary key identity (1,1) NOT NULL,
Nombre varchar (50) NOT NULL,
Mail varchar (50) NOT NULL,
Rol varchar (50) NOT NULL,
RolID int NOT NULL,
constraint fk_rol_usuario
foreign key (RolID)
references Roles(ID)
)

create table Roles(
ID int primary key identity(1,1) NOT NULL,
Nombre varchar(50) NOT NULL,
)

create table perfiles(
ID int primary key identity(1,1) NOT NULL,
Nombre varchar (50) NOT NULL,
Icono varchar(max) NULL,
usuarioID int NOT NULL,
constraint fk_perfil_usuario
foreign key (usuarioID)
references usuarios(ID)
)

create table listas(
ID int identity (1,1) primary key NOT NULL,
nombre varchar (50) NOT NULL,
perfilID int NOT NULL,
constraint fk_listas_usuario
foreign key (perfilID)
references perfiles(ID)
)

create table permisos(
ID int primary key identity(1,1) NOT NULL,
Nombre varchar(50) NOT NULL
)

create table rolesPermisos(
RolID int NOT NULL,
PermisoID int NOT NULL,
primary key (rolID, permisoID),
constraint fk_rol_permiso
foreign key (rolID)
references roles(ID),
constraint fk_permiso_rol
foreign key (permisoID)
references permisos(ID)
)

create table listasPelis(
primary key(listaID, peliculaID),
listaID int NOT NULL,
peliculaID int NOT NULL,
perfilID int NOT NULL,
constraint fk_pelis_listas
foreign key (peliculaID)
references peliculas(ID),
constraint fk_listas_pelis
foreign key (listaID)
references listas(ID),
constraint fk_perfil_listasP
foreign key (perfilID)
references perfiles(ID)
)

create table listaSeries(
primary key(listaID, serieID),
listaID int NOT NULL,
serieID int NOT NULL,
perfilID int NOT NULL,
constraint fk_series_listas
foreign key (serieID) 
references series(ID),
constraint fk_listas_series
foreign key (listaID)
references listas(ID),
constraint fk_perfil_listasS
foreign key (perfilID)
references perfiles(ID)
)


create table clima(
ID int identity (1,1) primary key NOT NULL,
fecha datetime NOT NULL,
temperatura int NOT NULL,
descripcionClima varchar(max) NOT NULL,
icono varchar (max) NULL
)

create table cotizaciones(
ID int identity (1,1) NOT NULL,
tipoMoneda varchar(50) NOT NULL,
valor decimal (18,0) NOT NULL,
fecha date NOT NULL
)

create table calificaciones(
ID int identity(1,1) primary key NOT NULL,
perfilID int NOT NULL,
peliculaID int null,
serieID int null,
puntaje int not null check (puntaje between 1 and 5),
fechaCalificacion DATETIME NOT NULL DEFAULT GETDATE(),
constraint fk_pelicula_calificaciones
foreign key (peliculaID) 
references peliculas(ID),
constraint fk_serie_calificaciones
foreign key (serieID)
references series(ID),
constraint fk_perfil_calificaciones
foreign key (perfilID)
references perfiles(ID),
constraint ck_calificacion_tipo
check(
(PeliculaID IS NOT NULL AND SerieID IS NULL)
OR
(PeliculaID IS NULL AND SerieID IS NOT NULL)
)
)

create table generos(
ID int identity(1,1) primary key NOT NULL, 
nombre varchar (50) NOT NULL
)

create table actores(
ID int identity (1,1) primary key NOT NULL,
nombre varchar(100) NOT NULL
)



create table generos_peliculas(
primary key(peliculaID, generoID),
peliculaID int NOT NULL,
generoID int NOT NULL,
constraint fk_genero_pelicula
foreign key (generoID)
references generos(ID),
constraint fk_pelicula_genero
foreign key (peliculaID)
references peliculas(ID)
)

create table actores_peliculas(
primary key(actorID, peliculaID),
actorID int NOT NULL,
peliculaID int NOT NULL,
constraint fk_actor_pelicula
foreign key (actorID)
references actores(ID),
constraint fk_pelicula_actor
foreign key (peliculaID)
references peliculas(ID)
)

create table generos_series(
primary key(serieID, generoID),
serieID int NOT NULL,
generoID int NOT NULL,
constraint fk_genero_serie
foreign key (generoID)
references generos(ID),
constraint fk_serie_genero
foreign key (serieID)
references series(ID)
)

create table actores_series(
primary key (actorID, serieID),
actorID int NOT NULL,
serieID int NOT NULL,
constraint fk_actor_serie
foreign key (actorID)
references actores(ID),
constraint fk_serie_actor
foreign key (serieID)
references series(ID)
)



CREATE UNIQUE INDEX IX_Calificacion_Pelicula
ON calificaciones(perfilID, PeliculaID)
WHERE PeliculaID IS NOT NULL;
GO

CREATE UNIQUE INDEX IX_Calificacion_Serie
ON Calificaciones(perfilID, SerieID)
WHERE SerieID IS NOT NULL;
GO                  
