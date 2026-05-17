using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebBanNongSan.Models;

namespace WebBanNongSan.Controllers
{
    public class HomeController : Controller
    {
        // GET: Home
        private AppDbContext db = new AppDbContext();
        public ActionResult Index()
        {
            var randomProducts = db.Products
                .ToList()
                .OrderBy(r => Guid.NewGuid())
                .Take(10)
                .ToList();

            return View(randomProducts);
        }

    }
}