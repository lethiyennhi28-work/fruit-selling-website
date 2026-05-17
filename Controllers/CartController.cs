using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebBanNongSan.Models;
using System.Data.Entity;

namespace WebBanNongSan.Controllers
{
    public class CartController : Controller
    {
        AppDbContext db = new AppDbContext();

        private Cart GetUserCart()
        {
            if (Session["Id"] == null) return null; 

            int userId = (int)Session["Id"];

            var cart = db.Cart
                         .Include(c => c.CartItems.Select(ci => ci.Product))
                         .FirstOrDefault(c => c.CartId == userId);

            if (cart == null)
            {
                cart = new Cart
                {
                    CartId = userId,
                    CartItems = new List<CartItem>()
                };
                db.Cart.Add(cart);
                db.SaveChanges();
            }
            return cart;
        }

        public ActionResult Index()
        {
            if (Session["Id"] == null) return RedirectToAction("Login", "Account");

            var cart = GetUserCart();
            if (cart == null || !cart.CartItems.Any())
            {
                ViewBag.Empty = true;
                return View(new List<CartItem>());
            }
            return View(cart.CartItems.ToList());
        }

        [HttpPost]
        public ActionResult AddToCart(int Id, int Quantity = 1)
        {
            if (Session["Id"] == null) return RedirectToAction("Login", "Account");

            var cart = GetUserCart();
            var cartItem = cart.CartItems.FirstOrDefault(ci => ci.ProductId == Id);

            if (cartItem != null)
            {
                cartItem.Quantity += Quantity;
            }
            else
            {
                var product = db.Products.Find(Id);
                if (product == null) return HttpNotFound();

                cartItem = new CartItem
                {
                    CartId = cart.CartId,
                    ProductId = Id,
                    Quantity = Quantity,
                    Name = product.Name,
                    ImageUrl = product.ImageUrl,
                    Price = product.Price
                };
                db.CartItems.Add(cartItem);
            }
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        public ActionResult Plus(int id)
        {
            if (Session["Id"] == null) return RedirectToAction("Login", "Account");
            var cart = GetUserCart();
            var item = cart.CartItems.FirstOrDefault(ci => ci.ProductId == id);
            if (item != null) { item.Quantity++; db.SaveChanges(); }
            return RedirectToAction("Index");
        }

        public ActionResult Minus(int id)
        {
            if (Session["Id"] == null) return RedirectToAction("Login", "Account");
            var cart = GetUserCart();
            var item = cart.CartItems.FirstOrDefault(ci => ci.ProductId == id);
            if (item != null)
            {
                item.Quantity--;
                if (item.Quantity <= 0) db.CartItems.Remove(item);
                db.SaveChanges();
            }
            return RedirectToAction("Index");
        }

        public ActionResult Remove(int id)
        {
            if (Session["Id"] == null) return RedirectToAction("Login", "Account");
            var cart = GetUserCart();
            var item = cart.CartItems.FirstOrDefault(ci => ci.ProductId == id);
            if (item != null) { db.CartItems.Remove(item); db.SaveChanges(); }
            return RedirectToAction("Index");
        }
        [HttpPost]
        public ActionResult UpdateQuantity(int cartItemId, int Quantity)
        {
            if (Session["Id"] == null) return RedirectToAction("Login", "Account");

            var item = db.CartItems.Find(cartItemId);

            if (item != null)
            {
                item.Quantity = Quantity;
                if (item.Quantity < 1) item.Quantity = 1; 
                db.SaveChanges();
            }

            return RedirectToAction("Index");
        }

        public ActionResult Delete(int id)
        {
            if (Session["Id"] == null) return RedirectToAction("Login", "Account");

            var item = db.CartItems.Find(id);

            if (item != null)
            {
                db.CartItems.Remove(item);
                db.SaveChanges();
            }

            return RedirectToAction("Index");
        }
    }
}