using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;
using RedFlix.Models;
using RedFlix.Models.Tmdb;
using RedFlix.Services;

namespace RedFlix.Controllers
{
    public class MiPerfilController : Controller
    {
        private readonly RedFlixIIIEntities db = new RedFlixIIIEntities();
        private readonly ProfileService _profileService = new ProfileService();
        private readonly TMDBService _tmdb = new TMDBService();

        public ActionResult Index()
        {
            var usuarioId = GetCurrentUserId();
            if (!usuarioId.HasValue)
            {
                return RedirectToAction("Index", "Login");
            }

            var perfiles = db.perfiles
                .Include(p => p.usuarios)
                .Where(p => p.usuarioID == usuarioId.Value)
                .ToList()
                .Select(p => _profileService.ToViewModel(p))
                .ToList();

            return View(perfiles);
        }

        public ActionResult Menu()
        {
            var perfil = GetCurrentProfile();
            if (perfil == null)
            {
                return RedirectToAction("Index");
            }

            ViewBag.TotalListas = db.listas.Count(l => l.perfilID == perfil.ID);
            ViewBag.TotalFavoritos = db.favoritos.Count(f => f.perfilID == perfil.ID);

            return View(_profileService.ToViewModel(perfil));
        }

        public ActionResult Seleccionar(int id)
        {
            var usuarioId = GetCurrentUserId();
            if (!usuarioId.HasValue)
            {
                return RedirectToAction("Index", "Login");
            }

            var perfil = db.perfiles.FirstOrDefault(p => p.ID == id && p.usuarioID == usuarioId.Value);
            if (perfil == null)
            {
                return HttpNotFound();
            }

            Session["PerfilID"] = perfil.ID;
            Session["PerfilNombre"] = perfil.Nombre;
            return RedirectToAction("Index", "Home");
        }

        public ActionResult Listas()
        {
            var perfil = GetCurrentProfile();
            if (perfil == null)
            {
                return RedirectToAction("Index");
            }

            var listas = db.listas
                .Where(l => l.perfilID == perfil.ID)
                .OrderBy(l => l.nombre)
                .ToList();

            ViewBag.PerfilActivo = perfil.Nombre;
            return View(listas);
        }

        public ActionResult CrearLista()
        {
            var perfil = GetCurrentProfile();
            if (perfil == null)
            {
                return RedirectToAction("Index");
            }

            return View(new listas { perfilID = perfil.ID });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CrearLista([Bind(Include = "nombre")] listas lista)
        {
            var perfil = GetCurrentProfile();
            if (perfil == null)
            {
                return RedirectToAction("Index");
            }

            if (ModelState.IsValid)
            {
                lista.perfilID = perfil.ID;
                db.listas.Add(lista);
                db.SaveChanges();
                return RedirectToAction("Listas");
            }

            return View(lista);
        }

        public ActionResult EditarLista(int? id)
        {
            var lista = GetOwnedList(id);
            if (lista == null)
            {
                return id == null ? (ActionResult)new HttpStatusCodeResult(HttpStatusCode.BadRequest) : HttpNotFound();
            }

            return View(lista);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditarLista([Bind(Include = "ID,nombre")] listas lista)
        {
            var existente = GetOwnedList(lista.ID);
            if (existente == null)
            {
                return HttpNotFound();
            }

            if (ModelState.IsValid)
            {
                existente.nombre = lista.nombre;
                db.Entry(existente).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Listas");
            }

            return View(lista);
        }

        public ActionResult EliminarLista(int? id)
        {
            var lista = GetOwnedList(id);
            if (lista == null)
            {
                return id == null ? (ActionResult)new HttpStatusCodeResult(HttpStatusCode.BadRequest) : HttpNotFound();
            }

            return View(lista);
        }

        [HttpPost, ActionName("EliminarLista")]
        [ValidateAntiForgeryToken]
        public ActionResult EliminarListaConfirmed(int id)
        {
            var lista = GetOwnedList(id);
            if (lista == null)
            {
                return HttpNotFound();
            }

            var contenidos = db.listaContenido.Where(c => c.listaID == lista.ID).ToList();
            db.listaContenido.RemoveRange(contenidos);
            db.listas.Remove(lista);
            db.SaveChanges();
            return RedirectToAction("Listas");
        }

        public async Task<ActionResult> DetalleLista(int? id)
        {
            var lista = GetOwnedList(id);
            if (lista == null)
            {
                return id == null ? (ActionResult)new HttpStatusCodeResult(HttpStatusCode.BadRequest) : HttpNotFound();
            }

            var model = new UserListViewModel
            {
                ID = lista.ID,
                Nombre = lista.nombre,
                PerfilID = lista.perfilID,
                PerfilNombre = lista.perfiles != null ? lista.perfiles.Nombre : null,
                Contenidos = new System.Collections.Generic.List<ContentItemViewModel>()
            };

            foreach (var contenido in db.listaContenido.Where(c => c.listaID == lista.ID).ToList())
            {
                model.Contenidos.Add(await LoadContentItemAsync(contenido.tmdbID, contenido.tipo));
            }

            return View(model);
        }

        public async Task<ActionResult> Favoritos()
        {
            var perfil = GetCurrentProfile();
            if (perfil == null)
            {
                return RedirectToAction("Index");
            }

            var favoritos = db.favoritos
                .Where(f => f.perfilID == perfil.ID)
                .ToList();

            var model = new System.Collections.Generic.List<ContentItemViewModel>();
            foreach (var favorito in favoritos)
            {
                model.Add(await LoadContentItemAsync(favorito.tmdbID, favorito.tipo));
            }

            ViewBag.PerfilActivo = perfil.Nombre;
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ToggleFavorito(int tmdbId, string tipo, string returnUrl)
        {
            var perfil = GetCurrentProfile();
            if (perfil == null)
            {
                return RedirectToAction("Index");
            }

            var favorito = db.favoritos.FirstOrDefault(f => f.perfilID == perfil.ID && f.tmdbID == tmdbId && f.tipo == tipo);
            if (favorito == null)
            {
                db.favoritos.Add(new favoritos { perfilID = perfil.ID, tmdbID = tmdbId, tipo = tipo });
            }
            else
            {
                db.favoritos.Remove(favorito);
            }

            db.SaveChanges();
            return RedirectToLocal(returnUrl);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AgregarALista(int? listaId, int tmdbId, string tipo, string returnUrl)
        {
            if (!listaId.HasValue)
            {
                return RedirectToLocal(returnUrl);
            }

            var lista = GetOwnedList(listaId);
            if (lista == null)
            {
                return HttpNotFound();
            }

            var existe = db.listaContenido.Any(c => c.listaID == lista.ID && c.tmdbID == tmdbId && c.tipo == tipo);
            if (!existe)
            {
                db.listaContenido.Add(new listaContenido { listaID = lista.ID, tmdbID = tmdbId, tipo = tipo });
                db.SaveChanges();
            }

            return RedirectToLocal(returnUrl);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CrearListaYAgregar(string nombreLista, int tmdbId, string tipo, string returnUrl)
        {
            var perfil = GetCurrentProfile();
            if (perfil == null)
            {
                return RedirectToAction("Index");
            }

            if (string.IsNullOrWhiteSpace(nombreLista))
            {
                return RedirectToLocal(returnUrl);
            }

            var lista = new listas
            {
                nombre = nombreLista.Trim(),
                perfilID = perfil.ID
            };

            db.listas.Add(lista);
            db.SaveChanges();

            db.listaContenido.Add(new listaContenido
            {
                listaID = lista.ID,
                tmdbID = tmdbId,
                tipo = tipo
            });

            db.SaveChanges();
            return RedirectToLocal(returnUrl);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult QuitarDeLista(int listaId, int tmdbId, string tipo)
        {
            var lista = GetOwnedList(listaId);
            if (lista == null)
            {
                return HttpNotFound();
            }

            var contenido = db.listaContenido.FirstOrDefault(c => c.listaID == lista.ID && c.tmdbID == tmdbId && c.tipo == tipo);
            if (contenido != null)
            {
                db.listaContenido.Remove(contenido);
                db.SaveChanges();
            }

            return RedirectToAction("DetalleLista", new { id = lista.ID });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Calificar(int tmdbId, string tipo, int puntaje, string returnUrl)
        {
            var perfil = GetCurrentProfile();
            if (perfil == null)
            {
                return RedirectToAction("Index");
            }

            if (tmdbId <= 0 || string.IsNullOrWhiteSpace(tipo) || puntaje < 1 || puntaje > 10)
            {
                return RedirectToLocal(returnUrl);
            }

            var calificacion = db.calificaciones.FirstOrDefault(c =>
                c.perfilID == perfil.ID &&
                c.tmdbID == tmdbId &&
                c.tipo == tipo);

            if (calificacion == null)
            {
                db.calificaciones.Add(new calificaciones
                {
                    perfilID = perfil.ID,
                    tmdbID = tmdbId,
                    tipo = tipo,
                    puntaje = puntaje,
                    fechaCalificacion = System.DateTime.Now
                });
            }
            else
            {
                calificacion.puntaje = puntaje;
                calificacion.fechaCalificacion = System.DateTime.Now;
                db.Entry(calificacion).State = EntityState.Modified;
            }

            db.SaveChanges();
            return RedirectToLocal(returnUrl);
        }

        public ActionResult Cuenta()
        {
            var usuarioId = GetCurrentUserId();
            if (!usuarioId.HasValue)
            {
                return RedirectToAction("Index", "Login");
            }

            var usuario = db.usuarios.Include(u => u.Roles).FirstOrDefault(u => u.ID == usuarioId.Value);
            if (usuario == null)
            {
                return HttpNotFound();
            }

            return View(usuario);
        }

        public ActionResult Details(int? id)
        {
            var perfil = GetOwnedProfile(id);
            if (perfil == null)
            {
                return id == null ? (ActionResult)new HttpStatusCodeResult(HttpStatusCode.BadRequest) : HttpNotFound();
            }

            return View(_profileService.ToViewModel(perfil));
        }

        public ActionResult Create()
        {
            var usuarioId = GetCurrentUserId();
            if (!usuarioId.HasValue)
            {
                return RedirectToAction("Index", "Login");
            }

            return View(new ProfileViewModel
            {
                UsuarioID = usuarioId.Value,
                Icono = ProfileService.DefaultIcons[0],
                IconosDisponibles = ProfileService.DefaultIcons
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(ProfileViewModel model)
        {
            var usuarioId = GetCurrentUserId();
            if (!usuarioId.HasValue)
            {
                return RedirectToAction("Index", "Login");
            }

            if (ModelState.IsValid)
            {
                var perfil = new perfiles
                {
                    Nombre = model.Nombre,
                    Icono = string.IsNullOrWhiteSpace(model.Icono) ? ProfileService.DefaultIcons[0] : model.Icono,
                    usuarioID = usuarioId.Value
                };

                db.perfiles.Add(perfil);
                db.SaveChanges();
                _profileService.SaveProfilePassword(perfil.ID, model.ContrasenaPerfil);
                return RedirectToAction("Index");
            }

            model.IconosDisponibles = ProfileService.DefaultIcons;
            return View(model);
        }

        public ActionResult Edit(int? id)
        {
            var perfil = GetOwnedProfile(id);
            if (perfil == null)
            {
                return id == null ? (ActionResult)new HttpStatusCodeResult(HttpStatusCode.BadRequest) : HttpNotFound();
            }

            return View(_profileService.ToViewModel(perfil));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(ProfileViewModel model)
        {
            var perfil = GetOwnedProfile(model.ID);
            if (perfil == null)
            {
                return HttpNotFound();
            }

            if (ModelState.IsValid)
            {
                perfil.Nombre = model.Nombre;
                perfil.Icono = string.IsNullOrWhiteSpace(model.Icono) ? ProfileService.DefaultIcons[0] : model.Icono;
                db.Entry(perfil).State = EntityState.Modified;
                db.SaveChanges();
                _profileService.SaveProfilePassword(perfil.ID, model.ContrasenaPerfil);
                return RedirectToAction("Index");
            }

            model.IconosDisponibles = ProfileService.DefaultIcons;
            return View(model);
        }

        public ActionResult Delete(int? id)
        {
            var perfil = GetOwnedProfile(id);
            if (perfil == null)
            {
                return id == null ? (ActionResult)new HttpStatusCodeResult(HttpStatusCode.BadRequest) : HttpNotFound();
            }

            return View(_profileService.ToViewModel(perfil));
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            var perfil = GetOwnedProfile(id);
            if (perfil == null)
            {
                return HttpNotFound();
            }

            RemoveProfileDependencies(perfil.ID);
            db.perfiles.Remove(perfil);
            db.SaveChanges();

            if (Session["PerfilID"] != null && (int)Session["PerfilID"] == id)
            {
                Session.Remove("PerfilID");
                Session.Remove("PerfilNombre");
            }

            return RedirectToAction("Index");
        }

        private int? GetCurrentUserId()
        {
            return Session["UsuarioID"] == null ? (int?)null : (int)Session["UsuarioID"];
        }

        private perfiles GetCurrentProfile()
        {
            var usuarioId = GetCurrentUserId();
            if (!usuarioId.HasValue || Session["PerfilID"] == null)
            {
                return null;
            }

            var perfilId = (int)Session["PerfilID"];
            return db.perfiles.FirstOrDefault(p => p.ID == perfilId && p.usuarioID == usuarioId.Value);
        }

        private perfiles GetOwnedProfile(int? id)
        {
            var usuarioId = GetCurrentUserId();
            if (!id.HasValue || !usuarioId.HasValue)
            {
                return null;
            }

            return db.perfiles
                .Include(p => p.usuarios)
                .FirstOrDefault(p => p.ID == id.Value && p.usuarioID == usuarioId.Value);
        }

        private listas GetOwnedList(int? id)
        {
            var usuarioId = GetCurrentUserId();
            if (!id.HasValue || !usuarioId.HasValue)
            {
                return null;
            }

            return db.listas
                .Include(l => l.perfiles)
                .FirstOrDefault(l => l.ID == id.Value && l.perfiles.usuarioID == usuarioId.Value);
        }

        private async Task<ContentItemViewModel> LoadContentItemAsync(int tmdbId, string tipo)
        {
            if (tipo == "Serie")
            {
                var serie = await _tmdb.GetSeriesDetailAsync(tmdbId);
                return new ContentItemViewModel
                {
                    TmdbId = serie.Id,
                    Tipo = "Serie",
                    Titulo = serie.Name,
                    PosterPath = serie.PosterPath,
                    Fecha = serie.FirstAirDate,
                    Puntaje = serie.VoteAverage
                };
            }

            var pelicula = await _tmdb.GetMovieDetailAsync(tmdbId);
            return new ContentItemViewModel
            {
                TmdbId = pelicula.Id,
                Tipo = "Pelicula",
                Titulo = pelicula.Title,
                PosterPath = pelicula.PosterPath,
                Fecha = pelicula.ReleaseDate,
                Puntaje = pelicula.VoteAverage
            };
        }

        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction("Index", "Home");
        }

        private void RemoveProfileDependencies(int perfilId)
        {
            var listas = db.listas.Where(l => l.perfilID == perfilId).ToList();
            var listaIds = listas.Select(l => l.ID).ToList();
            var contenidos = db.listaContenido.Where(c => listaIds.Contains(c.listaID)).ToList();
            db.listaContenido.RemoveRange(contenidos);
            db.listas.RemoveRange(listas);

            var favoritos = db.favoritos.Where(f => f.perfilID == perfilId).ToList();
            db.favoritos.RemoveRange(favoritos);

            var calificaciones = db.calificaciones.Where(c => c.perfilID == perfilId).ToList();
            db.calificaciones.RemoveRange(calificaciones);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}
