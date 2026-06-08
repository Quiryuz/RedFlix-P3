using System;
using System.Threading.Tasks;
using System.Web.Mvc;
using RedFlix.Models.Tmdb;
using RedFlix.Services;

namespace RedFlix.Controllers
{
    public class PeliculasController : Controller
    {
        private readonly TMDBService _tmdb = new TMDBService();

        public async Task<ActionResult> Index()
        {
            try
            {
                var response = await _tmdb.GetPopularMoviesAsync();
                ViewBag.Titulo = "Películas populares";
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
            try
            {
                var response = await _tmdb.GetTrendingMoviesAsync();
                ViewBag.Titulo = "Películas en tendencia";
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
            if (id == null || id <= 0)
            {
                return RedirectToAction("Index");
            }

            try
            {
                var movie = await _tmdb.GetMovieDetailAsync(id.Value);
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
            ViewBag.Query = q;

            if (string.IsNullOrWhiteSpace(q))
            {
                return View(new System.Collections.Generic.List<TmdbMovieResult>());
            }

            try
            {
                var response = await _tmdb.SearchMoviesAsync(q);
                return View(response.Results);
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Error en la búsqueda: " + ex.Message;
                return View(new System.Collections.Generic.List<TmdbMovieResult>());
            }
        }
    }
}
