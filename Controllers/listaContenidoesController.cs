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
    public class listaContenidoesController : Controller
    {
        private RedFlixIIIEntities db = new RedFlixIIIEntities();

        // GET: listaContenidoes
        public ActionResult Index()
        {
            var listaContenido = db.listaContenido.Include(l => l.listas);
            return View(listaContenido.ToList());
        }

        // GET: listaContenidoes/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            listaContenido listaContenido = db.listaContenido.Find(id);
            if (listaContenido == null)
            {
                return HttpNotFound();
            }
            return View(listaContenido);
        }

        // GET: listaContenidoes/Create
        public ActionResult Create()
        {
            ViewBag.listaID = new SelectList(db.listas, "ID", "nombre");
            return View();
        }

        // POST: listaContenidoes/Create
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que quiere enlazarse. Para obtener 
        // más detalles, vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "listaID,tmdbID,tipo")] listaContenido listaContenido)
        {
            if (ModelState.IsValid)
            {
                db.listaContenido.Add(listaContenido);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.listaID = new SelectList(db.listas, "ID", "nombre", listaContenido.listaID);
            return View(listaContenido);
        }

        // GET: listaContenidoes/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            listaContenido listaContenido = db.listaContenido.Find(id);
            if (listaContenido == null)
            {
                return HttpNotFound();
            }
            ViewBag.listaID = new SelectList(db.listas, "ID", "nombre", listaContenido.listaID);
            return View(listaContenido);
        }

        // POST: listaContenidoes/Edit/5
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que quiere enlazarse. Para obtener 
        // más detalles, vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "listaID,tmdbID,tipo")] listaContenido listaContenido)
        {
            if (ModelState.IsValid)
            {
                db.Entry(listaContenido).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.listaID = new SelectList(db.listas, "ID", "nombre", listaContenido.listaID);
            return View(listaContenido);
        }

        // GET: listaContenidoes/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            listaContenido listaContenido = db.listaContenido.Find(id);
            if (listaContenido == null)
            {
                return HttpNotFound();
            }
            return View(listaContenido);
        }

        // POST: listaContenidoes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            listaContenido listaContenido = db.listaContenido.Find(id);
            db.listaContenido.Remove(listaContenido);
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
