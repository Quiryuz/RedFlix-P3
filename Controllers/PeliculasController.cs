using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace RedFlix.Controllers
{
    public class PeliculasController : Controller
    {
        // GET: Peliculas
        public ActionResult Index()
        {
            return View();
        }

       /* public ActionResult Populares();
        public ActionResult Tendencias();
        public ActionResult Detalle(int id);
        public ActionResult Buscar(string nombre);*/
    }
}