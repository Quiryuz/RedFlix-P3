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
    public class perfilesController : Controller
    {
        private RedFlixIIIEntities db = new RedFlixIIIEntities();

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
            return View(perfiles);
        }

        // GET: perfiles/Create
        public ActionResult Create()
        {
            ViewBag.usuarioID = new SelectList(db.usuarios, "ID", "Nombre");
            return View();
        }

        // POST: perfiles/Create
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que quiere enlazarse. Para obtener 
        // más detalles, vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ID,Nombre,Icono,usuarioID")] perfiles perfiles)
        {
            if (ModelState.IsValid)
            {
                db.perfiles.Add(perfiles);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.usuarioID = new SelectList(db.usuarios, "ID", "Nombre", perfiles.usuarioID);
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
            return View(perfiles);
        }

        // POST: perfiles/Edit/5
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que quiere enlazarse. Para obtener 
        // más detalles, vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ID,Nombre,Icono,usuarioID")] perfiles perfiles)
        {
            if (ModelState.IsValid)
            {
                db.Entry(perfiles).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.usuarioID = new SelectList(db.usuarios, "ID", "Nombre", perfiles.usuarioID);
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
            return View(perfiles);
        }

        // POST: perfiles/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            perfiles perfiles = db.perfiles.Find(id);
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
    }
}
