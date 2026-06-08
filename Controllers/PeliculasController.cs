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
        private readonly TMDBService _tmdb = new TMDBService();
        private readonly RedFlixIIIEntities db = new RedFlixIIIEntities();

        public async Task<ActionResult> Index()
        {
            var profileRedirect = RedirectToProfileSelectionIfNeeded();
            if (profileRedirect != null)
            {
                return profileRedirect;
            }

            try
            {
                var response = await _tmdb.GetPopularMoviesAsync();
                ViewBag.Titulo = "Películas populares";
                LoadProfileContentState();
                return View(response.Results);
            }
            catch (Exception ex)
            {
                ViewBag.Error = "No se pudo conectar con TMDB: " + ex.Message;
                return View(new System.Collections.Generic.List<TmdbMovieResult>());
            }
        }

        public async Task<ActionResult> Tendencias()
        {
            var profileRedirect = RedirectToProfileSelectionIfNeeded();
            if (profileRedirect != null)
            {
                return profileRedirect;
            }

            try
            {
                var response = await _tmdb.GetTrendingMoviesAsync();
                ViewBag.Titulo = "Películas en tendencia";
                LoadProfileContentState();
                return View("Index", response.Results);
            }
            catch (Exception ex)
            {
                ViewBag.Error = "No se pudo conectar con TMDB: " + ex.Message;
                return View("Index", new System.Collections.Generic.List<TmdbMovieResult>());
            }
        }

        public async Task<ActionResult> Detalle(int? id)
        {
            var profileRedirect = RedirectToProfileSelectionIfNeeded();
            if (profileRedirect != null)
            {
                return profileRedirect;
            }

            if (id == null || id <= 0)
            {
                return RedirectToAction("Index");
            }

            try
            {
                var movie = await _tmdb.GetMovieDetailAsync(id.Value);
                LoadProfileContentState();
                return View(movie);
            }
            catch (Exception ex)
            {
                ViewBag.Error = "No se pudo cargar la película: " + ex.Message;
                return RedirectToAction("Index");
            }
        }

        public async Task<ActionResult> Buscar(string q)
        {
            var profileRedirect = RedirectToProfileSelectionIfNeeded();
            if (profileRedirect != null)
            {
                return profileRedirect;
            }

            ViewBag.Query = q;

            if (string.IsNullOrWhiteSpace(q))
            {
                return View(new System.Collections.Generic.List<TmdbMovieResult>());
            }

            try
            {
                var response = await _tmdb.SearchMoviesAsync(q);
                LoadProfileContentState();
                return View(response.Results);
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Error en la búsqueda: " + ex.Message;
                return View(new System.Collections.Generic.List<TmdbMovieResult>());
            }
        }

        private void LoadProfileContentState()
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
                db.favoritos
                    .Where(f => f.perfilID == perfilId && f.tipo == "Pelicula")
                    .Select(f => f.tmdbID)
                    .ToList());

            ViewBag.ListasPerfil = db.listas
                .Where(l => l.perfilID == perfilId)
                .OrderBy(l => l.nombre)
                .Select(l => new SelectListItem { Value = l.ID.ToString(), Text = l.nombre })
                .ToList();

            var listaIds = db.listas
                .Where(l => l.perfilID == perfilId)
                .Select(l => l.ID)
                .ToList();

            ViewBag.ListasContenidoKeys = new HashSet<string>(
                db.listaContenido
                    .Where(c => listaIds.Contains(c.listaID) && c.tipo == "Pelicula")
                    .Select(c => c.listaID + ":" + c.tmdbID)
                    .ToList());
        }

        private ActionResult RedirectToProfileSelectionIfNeeded()
        {
            if (Session["UsuarioID"] != null && Session["PerfilID"] == null)
            {
                return RedirectToAction("Index", "MiPerfil");
            }

            return null;
        }
    }
}
