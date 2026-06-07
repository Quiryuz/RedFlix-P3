using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using RedFlix;

namespace RedFlix.Controllers
{
    public class calificacionesController : Controller
    {
        private RedFlixIIIEntities db = new RedFlixIIIEntities();

        // GET: calificaciones
        public ActionResult Index()
        {
            var calificaciones = db.calificaciones.Include(c => c.perfiles);
            return View(calificaciones.ToList());
        }

        // GET: calificaciones/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            calificaciones calificaciones = db.calificaciones.Find(id);
            if (calificaciones == null)
            {
                return HttpNotFound();
            }
            return View(calificaciones);
        }

        // GET: calificaciones/Create
        public ActionResult Create()
        {
            ViewBag.perfilID = new SelectList(db.perfiles, "ID", "Nombre");
            return View();
        }

        // POST: calificaciones/Create
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que quiere enlazarse. Para obtener 
        // más detalles, vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ID,perfilID,tmdbID,tipo,puntaje,fechaCalificacion")] calificaciones calificaciones)
        {
            if (ModelState.IsValid)
            {
                db.calificaciones.Add(calificaciones);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.perfilID = new SelectList(db.perfiles, "ID", "Nombre", calificaciones.perfilID);
            return View(calificaciones);
        }

        // GET: calificaciones/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            calificaciones calificaciones = db.calificaciones.Find(id);
            if (calificaciones == null)
            {
                return HttpNotFound();
            }
            ViewBag.perfilID = new SelectList(db.perfiles, "ID", "Nombre", calificaciones.perfilID);
            return View(calificaciones);
        }

        // POST: calificaciones/Edit/5
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que quiere enlazarse. Para obtener 
        // más detalles, vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ID,perfilID,tmdbID,tipo,puntaje,fechaCalificacion")] calificaciones calificaciones)
        {
            if (ModelState.IsValid)
            {
                db.Entry(calificaciones).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.perfilID = new SelectList(db.perfiles, "ID", "Nombre", calificaciones.perfilID);
            return View(calificaciones);
        }

        // GET: calificaciones/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            calificaciones calificaciones = db.calificaciones.Find(id);
            if (calificaciones == null)
            {
                return HttpNotFound();
            }
            return View(calificaciones);
        }

        // POST: calificaciones/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            calificaciones calificaciones = db.calificaciones.Find(id);
            db.calificaciones.Remove(calificaciones);
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
    }
}
