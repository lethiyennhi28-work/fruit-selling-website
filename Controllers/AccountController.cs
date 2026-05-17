using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using WebBanNongSan.Models;
using System.Data.Entity;
using BCrypt.Net; // QUAN TRỌNG: Thư viện mã hóa mật khẩu

namespace WebBanNongSan.Controllers
{
    public class AccountController : Controller
    {
        AppDbContext db = new AppDbContext();

        // ==========================================
        // 1. ĐĂNG NHẬP & ĐĂNG KÝ
        // ==========================================

        [HttpGet]
        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Login(string Username, string Password)
        {
            // 1. Tìm user theo username trước
            var user = db.Users.FirstOrDefault(x => x.Username == Username);

            if (user != null)
            {
                // 2. Kiểm tra mật khẩu bằng BCrypt
                // Lưu ý: Cần xử lý cả trường hợp mật khẩu cũ (chưa mã hóa) nếu có
                bool isPasswordValid = false;

                try
                {
                    // Thử verify theo chuẩn BCrypt
                    isPasswordValid = BCrypt.Net.BCrypt.Verify(Password, user.PasswordHash);
                }
                catch
                {
                    // Nếu lỗi (do mật khẩu cũ chưa mã hóa), so sánh trực tiếp
                    if (user.PasswordHash == Password) isPasswordValid = true;
                }

                if (isPasswordValid)
                {
                    FormsAuthentication.SetAuthCookie(user.Username, false);
                    Session["Role"] = user.RoleId;
                    Session["FullName"] = user.FullName;
                    Session["Id"] = user.Id;

                    // Nếu là Admin (Role = 1) thì chuyển hướng vào trang Admin
                    if (user.RoleId == 1)
                    {
                        return RedirectToAction("Index", "Admin");
                    }

                    // Nếu là User thường thì về trang chủ
                    return RedirectToAction("Index", "Home");
                }
            }

            ViewBag.Error = "Sai tài khoản hoặc mật khẩu";
            return View();
        }

        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            Session.Clear();
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public ActionResult DangKy()
        {
            return View();
        }

        [HttpPost]
        public ActionResult DangKy(string Username, string Password, string ConfirmPassword, string FullName)
        {
            if (Password != ConfirmPassword)
            {
                ViewBag.Error = "Mật khẩu xác nhận không trùng khớp";
                return View();
            }

            var check = db.Users.FirstOrDefault(x => x.Username == Username);
            if (check != null)
            {
                ViewBag.Error = "Tài khoản đã tồn tại";
                return View();
            }

            // [THAY ĐỔI]: Mã hóa mật khẩu trước khi tạo user mới
            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(Password);

            var newUser = new User
            {
                Username = Username,
                PasswordHash = hashedPassword, // Lưu mật khẩu đã mã hóa
                FullName = FullName,
                RoleId = 2 // Mặc định là User thường
            };

            db.Users.Add(newUser);
            db.SaveChanges();

            TempData["Success"] = "Đăng ký thành công! Vui lòng đăng nhập.";
            return RedirectToAction("Login", "Account");
        }

        // ==========================================
        // 2. QUẢN LÝ ĐƠN HÀNG CÁ NHÂN
        // ==========================================

        // Danh sách đơn hàng của tôi
        public ActionResult MyOrders()
        {
            if (Session["Id"] == null)
            {
                return RedirectToAction("Login");
            }

            int userId = (int)Session["Id"];

            var orders = db.Order
                           .Where(o => o.userId == userId)
                           .OrderByDescending(o => o.OrderDate)
                           .ToList();

            return View(orders);
        }

        public ActionResult OrderDetails(int id)
        {
            if (Session["Id"] == null) return RedirectToAction("Login");

            int userId = (int)Session["Id"];

            var order = db.Order.FirstOrDefault(o => o.Id == id && o.userId == userId);

            if (order == null)
            {
                return HttpNotFound();
            }

            var details = db.OrderDetail
                            .Include("Product")
                            .Where(d => d.OrderId == id)
                            .ToList();

            ViewBag.Details = details;
            return View(order);
        }

        public ActionResult CancelOrder(int id)
        {
            if (Session["Id"] == null) return RedirectToAction("Login");

            int userId = (int)Session["Id"];

            var order = db.Order.FirstOrDefault(o => o.Id == id && o.userId == userId);

            if (order != null && order.Status == 1)
            {
                order.Status = 4; // 4 = Đã hủy
                db.SaveChanges();
                TempData["Success"] = "Đã hủy đơn hàng thành công.";
            }
            else
            {
                TempData["Error"] = "Đơn hàng đang giao hoặc đã hoàn thành, không thể hủy.";
            }

            return RedirectToAction("OrderDetails", new { id = id });
        }
    }
}