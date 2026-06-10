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
        private readonly ProfileService _servicioPerfiles = new ProfileService();
        private readonly TMDBService _servicioTmdb = new TMDBService();

        public ActionResult Index()
        {
            var usuarioId = ObtenerUsuarioActualId();
            if (!usuarioId.HasValue)
            {
                return RedirectToAction("Index", "Login");
            }

            var perfiles = db.perfiles
                .Include(p => p.usuarios)
                .Where(p => p.usuarioID == usuarioId.Value)
                .ToList()
                .Select(p => _servicioPerfiles.ConvertirAViewModel(p))
                .ToList();

            return View(perfiles);
        }

        public ActionResult Menu()
        {
            var perfil = ObtenerPerfilActual();
            if (perfil == null)
            {
                return RedirectToAction("Index");
            }

            ViewBag.TotalListas = db.listas.Count(l => l.perfilID == perfil.ID);
            ViewBag.TotalFavoritos = db.favoritos.Count(f => f.perfilID == perfil.ID);
            ViewBag.TotalCalificaciones = db.calificaciones.Count(c => c.perfilID == perfil.ID);

            return View(_servicioPerfiles.ConvertirAViewModel(perfil));
        }

        public ActionResult Seleccionar(int id)
        {
            var usuarioId = ObtenerUsuarioActualId();
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
            var perfil = ObtenerPerfilActual();
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
            var perfil = ObtenerPerfilActual();
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
            var perfil = ObtenerPerfilActual();
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
            var lista = ObtenerListaPropia(id);
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
            var existente = ObtenerListaPropia(lista.ID);
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
            var lista = ObtenerListaPropia(id);
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
            var lista = ObtenerListaPropia(id);
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
            var lista = ObtenerListaPropia(id);
            if (lista == null)
            {
                return id == null ? (ActionResult)new HttpStatusCodeResult(HttpStatusCode.BadRequest) : HttpNotFound();
            }

            var modelo = new UserListViewModel
            {
                ID = lista.ID,
                Nombre = lista.nombre,
                PerfilID = lista.perfilID,
                PerfilNombre = lista.perfiles != null ? lista.perfiles.Nombre : null,
                Contenidos = new System.Collections.Generic.List<ContentItemViewModel>()
            };

            foreach (var contenido in db.listaContenido.Where(c => c.listaID == lista.ID).ToList())
            {
                modelo.Contenidos.Add(await CargarContenidoAsync(contenido.tmdbID, contenido.tipo));
            }

            return View(modelo);
        }

        public async Task<ActionResult> Favoritos()
        {
            var perfil = ObtenerPerfilActual();
            if (perfil == null)
            {
                return RedirectToAction("Index");
            }

            var favoritos = db.favoritos
                .Where(f => f.perfilID == perfil.ID)
                .ToList();

            var modelo = new System.Collections.Generic.List<ContentItemViewModel>();
            foreach (var favorito in favoritos)
            {
                modelo.Add(await CargarContenidoAsync(favorito.tmdbID, favorito.tipo));
            }

            ViewBag.PerfilActivo = perfil.Nombre;
            return View(modelo);
        }

        public async Task<ActionResult> MisCalificaciones()
        {
            var perfil = ObtenerPerfilActual();
            if (perfil == null)
            {
                return RedirectToAction("Index");
            }

            var calificaciones = db.calificaciones
                .Where(c => c.perfilID == perfil.ID)
                .OrderByDescending(c => c.fechaCalificacion)
                .ToList();

            var modelo = new System.Collections.Generic.List<CalificacionPerfilViewModel>();

            foreach (var calificacion in calificaciones)
            {
                modelo.Add(new CalificacionPerfilViewModel
                {
                    Contenido = await CargarContenidoAsync(calificacion.tmdbID, calificacion.tipo),
                    PuntajePersonal = calificacion.puntaje,
                    FechaCalificacion = calificacion.fechaCalificacion
                });
            }

            ViewBag.PerfilActivo = perfil.Nombre;
            return View(modelo);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ToggleFavorito(int tmdbId, string tipo, string returnUrl)
        {
            var perfil = ObtenerPerfilActual();
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
            return RedirigirLocal(returnUrl);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AgregarALista(int? listaId, int tmdbId, string tipo, string returnUrl)
        {
            if (!listaId.HasValue)
            {
                return RedirigirLocal(returnUrl);
            }

            var lista = ObtenerListaPropia(listaId);
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

            return RedirigirLocal(returnUrl);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CrearListaYAgregar(string nombreLista, int tmdbId, string tipo, string returnUrl)
        {
            var perfil = ObtenerPerfilActual();
            if (perfil == null)
            {
                return RedirectToAction("Index");
            }

            if (string.IsNullOrWhiteSpace(nombreLista))
            {
                return RedirigirLocal(returnUrl);
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
            return RedirigirLocal(returnUrl);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult QuitarDeLista(int listaId, int tmdbId, string tipo)
        {
            var lista = ObtenerListaPropia(listaId);
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
            var perfil = ObtenerPerfilActual();
            if (perfil == null)
            {
                return RedirectToAction("Index");
            }

            if (tmdbId <= 0 || string.IsNullOrWhiteSpace(tipo) || puntaje < 1 || puntaje > 5)
            {
                return RedirigirLocal(returnUrl);
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
            return RedirigirLocal(returnUrl);
        }

        public ActionResult Cuenta()
        {
            var usuarioId = ObtenerUsuarioActualId();
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
            var perfil = ObtenerPerfilPropio(id);
            if (perfil == null)
            {
                return id == null ? (ActionResult)new HttpStatusCodeResult(HttpStatusCode.BadRequest) : HttpNotFound();
            }

            return View(_servicioPerfiles.ConvertirAViewModel(perfil));
        }

        public ActionResult Create()
        {
            var usuarioId = ObtenerUsuarioActualId();
            if (!usuarioId.HasValue)
            {
                return RedirectToAction("Index", "Login");
            }

            return View(new ProfileViewModel
            {
                UsuarioID = usuarioId.Value,
                Icono = ProfileService.IconosPredeterminados[0],
                IconosDisponibles = ProfileService.IconosPredeterminados
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(ProfileViewModel model)
        {
            var usuarioId = ObtenerUsuarioActualId();
            if (!usuarioId.HasValue)
            {
                return RedirectToAction("Index", "Login");
            }

            if (ModelState.IsValid)
            {
                var perfil = new perfiles
                {
                    Nombre = model.Nombre,
                    Icono = string.IsNullOrWhiteSpace(model.Icono) ? ProfileService.IconosPredeterminados[0] : model.Icono,
                    usuarioID = usuarioId.Value
                };

                db.perfiles.Add(perfil);
                db.SaveChanges();
                _servicioPerfiles.GuardarContrasenaPerfil(perfil.ID, model.ContrasenaPerfil);
                return RedirectToAction("Index");
            }

            model.IconosDisponibles = ProfileService.IconosPredeterminados;
            return View(model);
        }

        public ActionResult Edit(int? id)
        {
            var perfil = ObtenerPerfilPropio(id);
            if (perfil == null)
            {
                return id == null ? (ActionResult)new HttpStatusCodeResult(HttpStatusCode.BadRequest) : HttpNotFound();
            }

            return View(_servicioPerfiles.ConvertirAViewModel(perfil));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(ProfileViewModel model)
        {
            var perfil = ObtenerPerfilPropio(model.ID);
            if (perfil == null)
            {
                return HttpNotFound();
            }

            if (ModelState.IsValid)
            {
                perfil.Nombre = model.Nombre;
                perfil.Icono = string.IsNullOrWhiteSpace(model.Icono) ? ProfileService.IconosPredeterminados[0] : model.Icono;
                db.Entry(perfil).State = EntityState.Modified;
                db.SaveChanges();
                _servicioPerfiles.GuardarContrasenaPerfil(perfil.ID, model.ContrasenaPerfil);
                return RedirectToAction("Index");
            }

            model.IconosDisponibles = ProfileService.IconosPredeterminados;
            return View(model);
        }

        public ActionResult Delete(int? id)
        {
            var perfil = ObtenerPerfilPropio(id);
            if (perfil == null)
            {
                return id == null ? (ActionResult)new HttpStatusCodeResult(HttpStatusCode.BadRequest) : HttpNotFound();
            }

            return View(_servicioPerfiles.ConvertirAViewModel(perfil));
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            var perfil = ObtenerPerfilPropio(id);
            if (perfil == null)
            {
                return HttpNotFound();
            }

            EliminarDependenciasPerfil(perfil.ID);
            db.perfiles.Remove(perfil);
            db.SaveChanges();

            if (Session["PerfilID"] != null && (int)Session["PerfilID"] == id)
            {
                Session.Remove("PerfilID");
                Session.Remove("PerfilNombre");
            }

            return RedirectToAction("Index");
        }

        private int? ObtenerUsuarioActualId()
        {
            return Session["UsuarioID"] == null ? (int?)null : (int)Session["UsuarioID"];
        }

        private perfiles ObtenerPerfilActual()
        {
            var usuarioId = ObtenerUsuarioActualId();
            if (!usuarioId.HasValue || Session["PerfilID"] == null)
            {
                return null;
            }

            var perfilId = (int)Session["PerfilID"];
            return db.perfiles.FirstOrDefault(p => p.ID == perfilId && p.usuarioID == usuarioId.Value);
        }

        private perfiles ObtenerPerfilPropio(int? id)
        {
            var usuarioId = ObtenerUsuarioActualId();
            if (!id.HasValue || !usuarioId.HasValue)
            {
                return null;
            }

            return db.perfiles
                .Include(p => p.usuarios)
                .FirstOrDefault(p => p.ID == id.Value && p.usuarioID == usuarioId.Value);
        }

        private listas ObtenerListaPropia(int? id)
        {
            var usuarioId = ObtenerUsuarioActualId();
            if (!id.HasValue || !usuarioId.HasValue)
            {
                return null;
            }

            return db.listas
                .Include(l => l.perfiles)
                .FirstOrDefault(l => l.ID == id.Value && l.perfiles.usuarioID == usuarioId.Value);
        }

        private async Task<ContentItemViewModel> CargarContenidoAsync(int tmdbId, string tipo)
        {
            if (tipo == "Serie")
            {
                var serie = await _servicioTmdb.GetSeriesDetailAsync(tmdbId);
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

            var pelicula = await _servicioTmdb.GetMovieDetailAsync(tmdbId);
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

        private ActionResult RedirigirLocal(string returnUrl)
        {
            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction("Index", "Home");
        }

        private void EliminarDependenciasPerfil(int perfilId)
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
