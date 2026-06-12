using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using RedFlix.Authorization;
using RedFlix.Models.Tmdb;
using RedFlix.Services;

namespace RedFlix.Controllers
{
    [AuthorizePermission(Entity = PermissionKeys.Peliculas)]
    public class PeliculasController : Controller
    {
        private readonly TMDBService _servicioTmdb = new TMDBService();
        private readonly HistorialVisualizacionService _servicioHistorial = new HistorialVisualizacionService();
        private readonly RedFlixIIIEntities _baseDatos = new RedFlixIIIEntities();

        public async Task<ActionResult> Index()
        {
            var redireccionPerfil = RedirigirSeleccionPerfilSiHaceFalta();
            if (redireccionPerfil != null)
            {
                return redireccionPerfil;
            }

            try
            {
                var respuesta = await _servicioTmdb.GetPopularMoviesAsync();
                ViewBag.Titulo = "Peliculas populares";
                CargarEstadoContenidoPerfil();
                return View(respuesta.Results);
            }
            catch (Exception ex)
            {
                ViewBag.Error = "No se pudo conectar con TMDB: " + ex.Message;
                return View(new System.Collections.Generic.List<TmdbMovieResult>());
            }
        }

        public async Task<ActionResult> Tendencias()
        {
            var redireccionPerfil = RedirigirSeleccionPerfilSiHaceFalta();
            if (redireccionPerfil != null)
            {
                return redireccionPerfil;
            }

            try
            {
                var respuesta = await _servicioTmdb.GetTrendingMoviesAsync();
                ViewBag.Titulo = "Peliculas en tendencia";
                CargarEstadoContenidoPerfil();
                return View("Index", respuesta.Results);
            }
            catch (Exception ex)
            {
                ViewBag.Error = "No se pudo conectar con TMDB: " + ex.Message;
                return View("Index", new System.Collections.Generic.List<TmdbMovieResult>());
            }
        }

        public async Task<ActionResult> Detalle(int? id)
        {
            var redireccionPerfil = RedirigirSeleccionPerfilSiHaceFalta();
            if (redireccionPerfil != null)
            {
                return redireccionPerfil;
            }

            if (id == null || id <= 0)
            {
                return RedirectToAction("Index");
            }

            try
            {
                var pelicula = await _servicioTmdb.GetMovieDetailAsync(id.Value);
                CargarEstadoContenidoPerfil();
                CargarCalificacionPerfil(id.Value, "Pelicula");
                RegistrarVisualizacion(pelicula);
                return View(pelicula);
            }
            catch (Exception ex)
            {
                ViewBag.Error = "No se pudo cargar la pelicula: " + ex.Message;
                return RedirectToAction("Index");
            }
        }

        public async Task<ActionResult> Buscar(string q)
        {
            var redireccionPerfil = RedirigirSeleccionPerfilSiHaceFalta();
            if (redireccionPerfil != null)
            {
                return redireccionPerfil;
            }

            ViewBag.Query = q;

            if (string.IsNullOrWhiteSpace(q))
            {
                return View(new System.Collections.Generic.List<TmdbMovieResult>());
            }

            try
            {
                var respuesta = await _servicioTmdb.SearchMoviesAsync(q);
                CargarEstadoContenidoPerfil();
                return View(respuesta.Results);
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Error en la busqueda: " + ex.Message;
                return View(new System.Collections.Generic.List<TmdbMovieResult>());
            }
        }

        private void CargarEstadoContenidoPerfil()
        {
            if (Session["PerfilID"] == null)
            {
                ViewBag.FavoritosIds = new HashSet<int>();
                ViewBag.ListasPerfil = new List<SelectListItem>();
                ViewBag.ListasContenidoKeys = new HashSet<string>();
                return;
            }

            var perfilId = (int)Session["PerfilID"];
            ViewBag.FavoritosIds = new HashSet<int>(
                _baseDatos.favoritos
                    .Where(f => f.perfilID == perfilId && f.tipo == "Pelicula")
                    .Select(f => f.tmdbID)
                    .ToList());

            ViewBag.ListasPerfil = _baseDatos.listas
                .Where(l => l.perfilID == perfilId)
                .OrderBy(l => l.nombre)
                .Select(l => new SelectListItem { Value = l.ID.ToString(), Text = l.nombre })
                .ToList();

            var listaIds = _baseDatos.listas
                .Where(l => l.perfilID == perfilId)
                .Select(l => l.ID)
                .ToList();

            ViewBag.ListasContenidoKeys = new HashSet<string>(
                _baseDatos.listaContenido
                    .Where(c => listaIds.Contains(c.listaID) && c.tipo == "Pelicula")
                    .Select(c => c.listaID + ":" + c.tmdbID)
                    .ToList());
        }

        private void CargarCalificacionPerfil(int tmdbId, string tipo)
        {
            ViewBag.CalificacionPersonal = 0;

            if (Session["PerfilID"] == null)
            {
                return;
            }

            var perfilId = (int)Session["PerfilID"];
            var calificacion = _baseDatos.calificaciones.FirstOrDefault(c =>
                c.perfilID == perfilId &&
                c.tmdbID == tmdbId &&
                c.tipo == tipo);

            if (calificacion != null)
            {
                ViewBag.CalificacionPersonal = calificacion.puntaje;
            }
        }

        private void RegistrarVisualizacion(TmdbMovieDetail pelicula)
        {
            if (Session["PerfilID"] == null || pelicula == null || DebeOmitirHistorialVisualizacion())
            {
                return;
            }

            var generos = pelicula.Genres == null
                ? string.Empty
                : string.Join(", ", pelicula.Genres.Select(g => g.Name));

            _servicioHistorial.RegistrarVisualizacion(
                (int)Session["PerfilID"],
                pelicula.Id,
                "Pelicula",
                pelicula.Title,
                generos,
                pelicula.VoteAverage,
                pelicula.PosterPath);
        }

        private bool DebeOmitirHistorialVisualizacion()
        {
            return TempData["OmitirHistorialVisualizacion"] != null;
        }

        private ActionResult RedirigirSeleccionPerfilSiHaceFalta()
        {
            if (Session["UsuarioID"] != null && Session["PerfilID"] == null)
            {
                return RedirectToAction("Index", "MiPerfil");
            }

            return null;
        }
    }
}
