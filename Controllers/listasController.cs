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
    public class listasController : Controller
    {
        private RedFlixIIIEntities db = new RedFlixIIIEntities();

        // GET: listas
        public ActionResult Index()
        {
            var listas = db.listas.Include(l => l.perfiles);
            return View(listas.ToList());
        }

        // GET: listas/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            listas listas = db.listas.Find(id);
            if (listas == null)
            {
                return HttpNotFound();
            }
            return View(listas);
        }

        // GET: listas/Create
        public ActionResult Create()
        {
            ViewBag.perfilID = new SelectList(db.perfiles, "ID", "Nombre");
            return View();
        }

        // POST: listas/Create
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que quiere enlazarse. Para obtener 
        // más detalles, vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ID,nombre,perfilID")] listas listas)
        {
            if (ModelState.IsValid)
            {
                db.listas.Add(listas);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.perfilID = new SelectList(db.perfiles, "ID", "Nombre", listas.perfilID);
            return View(listas);
        }

        // GET: listas/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            listas listas = db.listas.Find(id);
            if (listas == null)
            {
                return HttpNotFound();
            }
            ViewBag.perfilID = new SelectList(db.perfiles, "ID", "Nombre", listas.perfilID);
            return View(listas);
        }

        // POST: listas/Edit/5
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que quiere enlazarse. Para obtener 
        // más detalles, vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ID,nombre,perfilID")] listas listas)
        {
            if (ModelState.IsValid)
            {
                db.Entry(listas).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.perfilID = new SelectList(db.perfiles, "ID", "Nombre", listas.perfilID);
            return View(listas);
        }

        // GET: listas/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            listas listas = db.listas.Find(id);
            if (listas == null)
            {
                return HttpNotFound();
            }
            return View(listas);
        }

        // POST: listas/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            listas listas = db.listas.Find(id);
            db.listas.Remove(listas);
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
