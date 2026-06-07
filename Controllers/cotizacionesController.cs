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
    public class cotizacionesController : Controller
    {
        private RedFlixIIIEntities db = new RedFlixIIIEntities();

        // GET: cotizaciones
        public ActionResult Index()
        {
            return View(db.cotizaciones.ToList());
        }

        // GET: cotizaciones/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            cotizaciones cotizaciones = db.cotizaciones.Find(id);
            if (cotizaciones == null)
            {
                return HttpNotFound();
            }
            return View(cotizaciones);
        }

        // GET: cotizaciones/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: cotizaciones/Create
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que quiere enlazarse. Para obtener 
        // más detalles, vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ID,tipoMoneda,valor,fecha")] cotizaciones cotizaciones)
        {
            if (ModelState.IsValid)
            {
                db.cotizaciones.Add(cotizaciones);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(cotizaciones);
        }

        // GET: cotizaciones/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            cotizaciones cotizaciones = db.cotizaciones.Find(id);
            if (cotizaciones == null)
            {
                return HttpNotFound();
            }
            return View(cotizaciones);
        }

        // POST: cotizaciones/Edit/5
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que quiere enlazarse. Para obtener 
        // más detalles, vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ID,tipoMoneda,valor,fecha")] cotizaciones cotizaciones)
        {
            if (ModelState.IsValid)
            {
                db.Entry(cotizaciones).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(cotizaciones);
        }

        // GET: cotizaciones/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            cotizaciones cotizaciones = db.cotizaciones.Find(id);
            if (cotizaciones == null)
            {
                return HttpNotFound();
            }
            return View(cotizaciones);
        }

        // POST: cotizaciones/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            cotizaciones cotizaciones = db.cotizaciones.Find(id);
            db.cotizaciones.Remove(cotizaciones);
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
