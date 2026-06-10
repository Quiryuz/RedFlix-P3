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
    [AuthorizePermission(Entity = PermissionKeys.Series)]
    public class SeriesController : Controller
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
                var response = await _tmdb.GetPopularSeriesAsync();
                ViewBag.Titulo = "Series populares";
                LoadProfileContentState();
                return View(response.Results);
            }
            catch (Exception ex)
            {
                ViewBag.Error = "No se pudo conectar con TMDB: " + ex.Message;
                return View(new System.Collections.Generic.List<TmdbTvResult>());
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
                var response = await _tmdb.GetTrendingSeriesAsync();
                ViewBag.Titulo = "Series en tendencia";
                LoadProfileContentState();
                return View("Index", response.Results);
            }
            catch (Exception ex)
            {
                ViewBag.Error = "No se pudo conectar con TMDB: " + ex.Message;
                return View("Index", new System.Collections.Generic.List<TmdbTvResult>());
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
                var serie = await _tmdb.GetSeriesDetailAsync(id.Value);
                LoadProfileContentState();
                LoadProfileRating(id.Value, "Serie");
                return View(serie);
            }
            catch (Exception ex)
            {
                ViewBag.Error = "No se pudo cargar la serie: " + ex.Message;
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
                return View(new System.Collections.Generic.List<TmdbTvResult>());
            }

            try
            {
                var response = await _tmdb.SearchSeriesAsync(q);
                LoadProfileContentState();
                return View(response.Results);
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Error en la busqueda: " + ex.Message;
                return View(new System.Collections.Generic.List<TmdbTvResult>());
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
                    .Where(f => f.perfilID == perfilId && f.tipo == "Serie")
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
                    .Where(c => listaIds.Contains(c.listaID) && c.tipo == "Serie")
                    .Select(c => c.listaID + ":" + c.tmdbID)
                    .ToList());
        }

        private void LoadProfileRating(int tmdbId, string tipo)
        {
            ViewBag.CalificacionPersonal = 0;

            if (Session["PerfilID"] == null)
            {
                return;
            }

            var perfilId = (int)Session["PerfilID"];
            var calificacion = db.calificaciones.FirstOrDefault(c =>
                c.perfilID == perfilId &&
                c.tmdbID == tmdbId &&
                c.tipo == tipo);

            if (calificacion != null)
            {
                ViewBag.CalificacionPersonal = calificacion.puntaje;
            }
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
