using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebBanNongSan.Models;
using System.Data.Entity;

namespace WebBanNongSan.Controllers
{
    public class CheckoutController : Controller
    {
        private AppDbContext db = new AppDbContext();

        // Helper: Lấy ID người dùng từ Session
        private int GetUserId()
        {
            if (Session["Id"] == null) return -1;
            return (int)Session["Id"];
        }

        [HttpGet]
        public ActionResult Checkout()
        {
            int userId = GetUserId();
            if (userId == -1) return RedirectToAction("Login", "Account");

            var cartItems = db.CartItems.Include("Product").Where(c => c.CartId == userId).ToList();

            if (cartItems.Count == 0)
            {
                return RedirectToAction("Index", "Cart"); 
            }

            ViewBag.CartItems = cartItems;
            return View();
        }

        [HttpPost]
        public ActionResult Checkout(Order order)
        {
            int userId = GetUserId();
            if (userId == -1) return RedirectToAction("Login", "Account");

            var cartItems = db.CartItems.Where(c => c.CartId == userId).ToList();
            if (cartItems.Count == 0) return RedirectToAction("Index", "Cart");

            order.userId = userId;
            order.OrderDate = DateTime.Now;
            order.Status = 1; 

            decimal tongTienHang = cartItems.Sum(x => x.Quantity * x.Price);
            decimal tienShip = (tongTienHang >= 300000) ? 0 : 20000; 
            order.TongTien = tongTienHang + tienShip;

            db.Order.Add(order);
            try
            {
                db.SaveChanges();
            }
            catch (System.Data.Entity.Validation.DbEntityValidationException ex)
            {
                var errorMessages = ex.EntityValidationErrors
                        .SelectMany(x => x.ValidationErrors)
                        .Select(x => x.ErrorMessage);

                var fullErrorMessage = string.Join("; ", errorMessages);
                var exceptionMessage = string.Concat(ex.Message, " Lỗi cụ thể là: ", fullErrorMessage);

                throw new System.Data.Entity.Validation.DbEntityValidationException(exceptionMessage, ex.EntityValidationErrors);
            }

            foreach (var item in cartItems)
            {
                var detail = new OrderDetails
                {
                    OrderId = order.Id, 
                    productId = item.ProductId,
                    Quantity = item.Quantity,
                    Price = item.Price,
                    Total = item.Quantity * item.Price
                };
                db.OrderDetail.Add(detail);
            }

            db.CartItems.RemoveRange(cartItems);

            db.SaveChanges();

            return RedirectToAction("Success", new { id = order.Id });
        }

        public ActionResult Success(int id)
        {
            var order = db.Order.Find(id);

            return View(order);
        }
    }
}