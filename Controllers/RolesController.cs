using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using RedFlix;
using RedFlix.Authorization;
using RedFlix.Helpers;
using RedFlix.Services;

namespace RedFlix.Controllers
{
    [AuthorizePermission(Entity = PermissionKeys.Roles)]
    public class RolesController : Controller
    {
        private readonly RedFlixIIIEntities db = new RedFlixIIIEntities();
        private readonly PermissionService _permissionService = new PermissionService();

        public ActionResult Index()
        {
            return View(db.Roles.ToList());
        }

        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Roles roles = db.Roles.Find(id);
            if (roles == null)
            {
                return HttpNotFound();
            }

            ViewBag.PermissionGroups = _permissionService.BuildPermissionGroups(id);
            return View(roles);
        }

        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ID,Nombre")] Roles roles)
        {
            if (ModelState.IsValid)
            {
                db.Roles.Add(roles);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(roles);
        }

        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Roles roles = db.Roles.Find(id);
            if (roles == null)
            {
                return HttpNotFound();
            }

            ViewBag.PermissionGroups = _permissionService.BuildPermissionGroups(id);
            return View(roles);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ID,Nombre")] Roles roles, int[] permisoIds)
        {
            if (ModelState.IsValid)
            {
                db.Entry(roles).State = EntityState.Modified;
                db.SaveChanges();
                _permissionService.AssignPermissionsToRole(roles.ID, permisoIds ?? new int[0]);

                if (Session["RolID"] != null && Convert.ToInt32(Session["RolID"]) == roles.ID)
                {
                    PermissionHelper.SetUserPermissions(
                        Session,
                        _permissionService.GetPermissionNamesForRole(roles.ID));
                }

                return RedirectToAction("Index");
            }

            ViewBag.PermissionGroups = _permissionService.BuildPermissionGroups(roles.ID);
            return View(roles);
        }

        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Roles roles = db.Roles.Find(id);
            if (roles == null)
            {
                return HttpNotFound();
            }

            return View(roles);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Roles roles = db.Roles.Find(id);
            db.Roles.Remove(roles);
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
