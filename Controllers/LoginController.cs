using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using RedFlix.Controllers;

namespace RedFlix.Controllers
{
    public class LoginController : Controller
    {

        private RedFlixIIIEntities db = new RedFlixIIIEntities();

        // GET: Login
        public ActionResult Index()
        {
            return View();
        }

        // GET: Login/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: Login/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Login/Create
        [HttpPost]
        public ActionResult Index(string email, string contrasenia)
        {
            var usuario = db.usuarios.FirstOrDefault(x => x.Mail == email && x.Contrasena == contrasenia);

            if (usuario != null)
            {
                Session["UsuarioID"] = usuario.ID;
                Session["Nombre"] = usuario.Nombre;
                Session["RolID"] = usuario.RolID;
                return RedirectToAction("Index", "Home");
            }
            else
            {
                
            }
        }

        // GET: Login/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: Login/Edit/5
        [HttpPost]
        public ActionResult Edit(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add update logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        // GET: Login/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: Login/Delete/5
        [HttpPost]
        public ActionResult Delete(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add delete logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }
    }
}
