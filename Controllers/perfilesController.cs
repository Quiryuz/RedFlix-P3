using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using RedFlix;
using RedFlix.Authorization;
using RedFlix.Services;

namespace RedFlix.Controllers
{
    [AuthorizePermission(Entity = PermissionKeys.Perfiles)]
    public class perfilesController : Controller
    {
        private RedFlixIIIEntities db = new RedFlixIIIEntities();
        private readonly ProfileService _servicioPerfiles = new ProfileService();

        // GET: perfiles
        public ActionResult Index()
        {
            var perfiles = db.perfiles.Include(p => p.usuarios);
            return View(perfiles.ToList());
        }

        // GET: perfiles/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            perfiles perfiles = db.perfiles.Find(id);
            if (perfiles == null)
            {
                return HttpNotFound();
            }
            ViewBag.ContrasenaPerfil = _servicioPerfiles.ObtenerContrasenaPerfil(perfiles.ID);
            return View(perfiles);
        }

        // GET: perfiles/Create
        public ActionResult Create()
        {
            ViewBag.usuarioID = new SelectList(db.usuarios, "ID", "Nombre");
            ViewBag.Iconos = ProfileService.IconosPredeterminados;
            return View();
        }

        // POST: perfiles/Create
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que quiere enlazarse. Para obtener 
        // más detalles, vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ID,Nombre,Icono,usuarioID")] perfiles perfiles, string ContrasenaPerfil)
        {
            if (ModelState.IsValid)
            {
                if (string.IsNullOrWhiteSpace(perfiles.Icono))
                {
                    perfiles.Icono = ProfileService.IconosPredeterminados[0];
                }

                db.perfiles.Add(perfiles);
                db.SaveChanges();
                _servicioPerfiles.GuardarContrasenaPerfil(perfiles.ID, ContrasenaPerfil);
                return RedirectToAction("Index");
            }

            ViewBag.usuarioID = new SelectList(db.usuarios, "ID", "Nombre", perfiles.usuarioID);
            ViewBag.Iconos = ProfileService.IconosPredeterminados;
            return View(perfiles);
        }

        // GET: perfiles/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            perfiles perfiles = db.perfiles.Find(id);
            if (perfiles == null)
            {
                return HttpNotFound();
            }
            ViewBag.usuarioID = new SelectList(db.usuarios, "ID", "Nombre", perfiles.usuarioID);
            ViewBag.Iconos = ProfileService.IconosPredeterminados;
            ViewBag.ContrasenaPerfil = _servicioPerfiles.ObtenerContrasenaPerfil(perfiles.ID);
            return View(perfiles);
        }

        // POST: perfiles/Edit/5
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que quiere enlazarse. Para obtener 
        // más detalles, vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ID,Nombre,Icono,usuarioID")] perfiles perfiles, string ContrasenaPerfil)
        {
            if (ModelState.IsValid)
            {
                if (string.IsNullOrWhiteSpace(perfiles.Icono))
                {
                    perfiles.Icono = ProfileService.IconosPredeterminados[0];
                }

                db.Entry(perfiles).State = EntityState.Modified;
                db.SaveChanges();
                _servicioPerfiles.GuardarContrasenaPerfil(perfiles.ID, ContrasenaPerfil);
                return RedirectToAction("Index");
            }
            ViewBag.usuarioID = new SelectList(db.usuarios, "ID", "Nombre", perfiles.usuarioID);
            ViewBag.Iconos = ProfileService.IconosPredeterminados;
            ViewBag.ContrasenaPerfil = ContrasenaPerfil;
            return View(perfiles);
        }

        // GET: perfiles/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            perfiles perfiles = db.perfiles.Find(id);
            if (perfiles == null)
            {
                return HttpNotFound();
            }
            ViewBag.ContrasenaPerfil = _servicioPerfiles.ObtenerContrasenaPerfil(perfiles.ID);
            return View(perfiles);
        }

        // POST: perfiles/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            perfiles perfiles = db.perfiles.Find(id);
            EliminarDependenciasPerfil(id);
            db.perfiles.Remove(perfiles);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
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
    }
}
