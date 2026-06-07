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
    public class climasController : Controller
    {
        private RedFlixIIIEntities db = new RedFlixIIIEntities();

        // GET: climas
        public ActionResult Index()
        {
            return View(db.clima.ToList());
        }

        // GET: climas/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            clima clima = db.clima.Find(id);
            if (clima == null)
            {
                return HttpNotFound();
            }
            return View(clima);
        }

        // GET: climas/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: climas/Create
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que quiere enlazarse. Para obtener 
        // más detalles, vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ID,fecha,temperatura,descripcionClima,icono")] clima clima)
        {
            if (ModelState.IsValid)
            {
                db.clima.Add(clima);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(clima);
        }

        // GET: climas/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            clima clima = db.clima.Find(id);
            if (clima == null)
            {
                return HttpNotFound();
            }
            return View(clima);
        }

        // POST: climas/Edit/5
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que quiere enlazarse. Para obtener 
        // más detalles, vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ID,fecha,temperatura,descripcionClima,icono")] clima clima)
        {
            if (ModelState.IsValid)
            {
                db.Entry(clima).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(clima);
        }

        // GET: climas/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            clima clima = db.clima.Find(id);
            if (clima == null)
            {
                return HttpNotFound();
            }
            return View(clima);
        }

        // POST: climas/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            clima clima = db.clima.Find(id);
            db.clima.Remove(clima);
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
