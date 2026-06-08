using System;
using System.Threading.Tasks;
using System.Web.Mvc;
using RedFlix.Services;

namespace RedFlix.Controllers
{
    public class HomeController : Controller
    {
        private readonly TMDBService _tmdb = new TMDBService();

        public async Task<ActionResult> Index()
        {
            if (Session["UsuarioID"] != null && Session["PerfilID"] == null)
            {
                return RedirectToAction("Index", "MiPerfil");
            }

            try
            {
                ViewBag.TrendingMovies = (await _tmdb.GetTrendingMoviesAsync()).Results;
                ViewBag.TrendingSeries = (await _tmdb.GetTrendingSeriesAsync()).Results;
            }
            catch (Exception ex)
            {
                ViewBag.Error = "No se pudo conectar con TMDB: " + ex.Message;
            }

            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "RedFlix+ - Plataforma de streaming.";
            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Contacto RedFlix+.";
            return View();
        }

        public ActionResult AccessDenied()
        {
            return View();
        }
    }
}
