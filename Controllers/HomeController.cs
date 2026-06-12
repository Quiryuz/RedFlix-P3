using System;
using System.Threading.Tasks;
using System.Web.Mvc;
using RedFlix.Services;

namespace RedFlix.Controllers
{
    public class HomeController : Controller
    {
        private readonly TMDBService _servicioTmdb = new TMDBService();
        private readonly ClimaService _servicioClima = new ClimaService();

        public async Task<ActionResult> Index()
        {
            if (Session["UsuarioID"] != null && Session["PerfilID"] == null)
            {
                return RedirectToAction("Index", "MiPerfil");
            }

            try
            {
                ViewBag.PeliculasTendencia = (await _servicioTmdb.GetTrendingMoviesAsync()).Results;
                ViewBag.SeriesTendencia = (await _servicioTmdb.GetTrendingSeriesAsync()).Results;
                var recomendacionClima = await _servicioClima.ObtenerRecomendacionPorClimaAsync();
                ViewBag.ClimaRecomendacion = recomendacionClima;
                ViewBag.PeliculasPorClima = (await _servicioTmdb.GetMoviesByGenreAsync(recomendacionClima.GeneroTmdbId)).Results;
            }
            catch (Exception ex)
            {
                ViewBag.Error = "No se pudo cargar informacion externa: " + ex.Message;
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
