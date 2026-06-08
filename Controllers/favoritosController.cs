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

namespace RedFlix.Controllers
{
    [AuthorizePermission(Entity = PermissionKeys.Favoritos)]
    public class favoritosController : Controller
    {
        private RedFlixIIIEntities db = new RedFlixIIIEntities();

        // GET: favoritos
        public ActionResult Index()
        {
            var favoritos = db.favoritos.Include(f => f.perfiles);
            return View(favoritos.ToList());
        }

        // GET: favoritos/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            favoritos favoritos = db.favoritos.Find(id);
            if (favoritos == null)
            {
                return HttpNotFound();
            }
            return View(favoritos);
        }

        // GET: favoritos/Create
        public ActionResult Create()
        {
            ViewBag.perfilID = new SelectList(db.perfiles, "ID", "Nombre");
            return View();
        }

        // POST: favoritos/Create
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que quiere enlazarse. Para obtener 
        // más detalles, vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ID,perfilID,tmdbID,tipo")] favoritos favoritos)
        {
            if (ModelState.IsValid)
            {
                db.favoritos.Add(favoritos);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.perfilID = new SelectList(db.perfiles, "ID", "Nombre", favoritos.perfilID);
            return View(favoritos);
        }

        // GET: favoritos/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            favoritos favoritos = db.favoritos.Find(id);
            if (favoritos == null)
            {
                return HttpNotFound();
            }
            ViewBag.perfilID = new SelectList(db.perfiles, "ID", "Nombre", favoritos.perfilID);
            return View(favoritos);
        }

        // POST: favoritos/Edit/5
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que quiere enlazarse. Para obtener 
        // más detalles, vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ID,perfilID,tmdbID,tipo")] favoritos favoritos)
        {
            if (ModelState.IsValid)
            {
                db.Entry(favoritos).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.perfilID = new SelectList(db.perfiles, "ID", "Nombre", favoritos.perfilID);
            return View(favoritos);
        }

        // GET: favoritos/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            favoritos favoritos = db.favoritos.Find(id);
            if (favoritos == null)
            {
                return HttpNotFound();
            }
            return View(favoritos);
        }

        // POST: favoritos/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            favoritos favoritos = db.favoritos.Find(id);
            db.favoritos.Remove(favoritos);
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
