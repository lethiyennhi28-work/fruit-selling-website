using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebBanNongSan.Models;
using System.Data.Entity;
using System.IO;
using BCrypt.Net;

namespace WebBanNongSan.Controllers
{
    public class AdminController : Controller
    {
        AppDbContext db = new AppDbContext();

        // ==========================================
        // HELPER: KIỂM TRA QUYỀN ADMIN & CHUYỂN HƯỚNG
        // ==========================================
        private bool CheckAdmin()
        {
            if (Session["Role"] != null && Session["Role"].ToString() == "1")
            {
                return true;
            }
            return false;
        }

        private ActionResult RedirectToLogin()
        {
            return RedirectToAction("Login", "Account");
        }

        // ==========================================
        // HELPER: HÀM CHUYỂN TIẾNG VIỆT CÓ DẤU THÀNH KHÔNG DẤU
        // ==========================================
        private string RemoveSign4VietnameseString(string str)
        {
            if (string.IsNullOrEmpty(str)) return str;
            string[] VietnameseSigns = new string[]
            {
                "aAeEoOuUiIdDyY",
                "áàạảãâấầậẩẫăắằặẳẵ",
                "ÁÀẠẢÃÂẤẦẬẨẪĂẮẰẶẲẴ",
                "éèẹẻẽêếềệểễ",
                "ÉÈẸẺẼÊẾỀỆỂỄ",
                "óòọỏõôốồộổỗơớờợởỡ",
                "ÓÒỌỎÕÔỐỒỘỔỖƠỚỜỢỞỠ",
                "úùụủũưứừựửữ",
                "ÚÙỤỦŨƯỨỪỰỬỮ",
                "íìịỉĩ",
                "ÍÌỊỈĨ",
                "đ",
                "Đ",
                "ýỳỵỷỹ",
                "ÝỲỴỶỸ"
            };
            for (int i = 1; i < VietnameseSigns.Length; i++)
            {
                for (int j = 0; j < VietnameseSigns[i].Length; j++)
                    str = str.Replace(VietnameseSigns[i][j], VietnameseSigns[0][i - 1]);
            }
            return str;
        }

        // ==========================================
        // 1. DASHBOARD
        // ==========================================
        public ActionResult Index()
        {
            if (!CheckAdmin()) return RedirectToLogin();

            ViewBag.NewOrders = db.Order.Where(o => o.Status == 1).Count();
            var validRevenue = db.OrderDetail.Where(d => d.Order.Status != 4);
            ViewBag.Revenue = validRevenue.Any() ? validRevenue.Sum(x => x.Total) : 0;
            ViewBag.TotalProducts = db.Products.Count();
            ViewBag.TotalUsers = db.Users.Count();

            return View();
        }

        // ==========================================
        // 2. QUẢN LÝ SẢN PHẨM (ĐÃ THÊM TÌM KIẾM KHÔNG DẤU)
        // ==========================================
        public ActionResult Products(string search, int? categoryId)
        {
            if (!CheckAdmin()) return RedirectToLogin();

            var query = db.Products.Include("Category").AsQueryable();

            // 1. Lọc theo danh mục (SQL) - Lọc cứng trước để giảm dữ liệu
            if (categoryId.HasValue && categoryId.Value > 0)
            {
                query = query.Where(x => x.CategoryId == categoryId.Value);
            }

            // 2. Lấy dữ liệu về RAM để xử lý tìm kiếm Tiếng Việt
            var listProducts = query.OrderByDescending(x => x.Id).ToList();

            // 3. Tìm kiếm không dấu (RAM)
            if (!string.IsNullOrEmpty(search))
            {
                string searchKey = RemoveSign4VietnameseString(search.ToLower());

                listProducts = listProducts.Where(x =>
                    RemoveSign4VietnameseString(x.Name.ToLower()).Contains(searchKey)
                ).ToList();
            }

            // Lưu lại Viewbag để hiển thị lại trên Form
            ViewBag.CategoryId = new SelectList(db.Categories, "Id", "CategoryName", categoryId);
            ViewBag.Search = search;

            return View(listProducts);
        }

        [HttpGet]
        public ActionResult Create()
        {
            if (!CheckAdmin()) return RedirectToLogin();
            ViewBag.CategoryId = new SelectList(db.Categories, "Id", "CategoryName");
            return View();
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Create(Product product, HttpPostedFileBase ImageFile)
        {
            if (!CheckAdmin()) return RedirectToLogin();

            if (ModelState.IsValid)
            {
                if (ImageFile != null && ImageFile.ContentLength > 0)
                {
                    string extension = Path.GetExtension(ImageFile.FileName).ToLower();
                    if (extension != ".jpg" && extension != ".jpeg" && extension != ".png")
                    {
                        ModelState.AddModelError("ImageFile", "Chỉ chấp nhận file ảnh (.jpg, .jpeg, .png)");
                        ViewBag.CategoryId = new SelectList(db.Categories, "Id", "CategoryName", product.CategoryId);
                        return View(product);
                    }

                    string fileName = Path.GetFileName(ImageFile.FileName);
                    string folderPath = Server.MapPath("~/Images/");

                    if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);

                    string path = Path.Combine(folderPath, fileName);
                    ImageFile.SaveAs(path);
                    product.ImageUrl = "~/Images/" + fileName;
                }
                else
                {
                    product.ImageUrl = "~/Images/default.png";
                }

                db.Products.Add(product);
                db.SaveChanges();
                TempData["Success"] = "Thêm sản phẩm thành công!";
                return RedirectToAction("Products");
            }

            ViewBag.CategoryId = new SelectList(db.Categories, "Id", "CategoryName", product.CategoryId);
            return View(product);
        }

        [HttpGet]
        public ActionResult Edit(int id)
        {
            if (!CheckAdmin()) return RedirectToLogin();
            var product = db.Products.Find(id);
            if (product == null) return HttpNotFound();
            ViewBag.CategoryId = new SelectList(db.Categories, "Id", "CategoryName", product.CategoryId);
            return View(product);
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Edit(Product product, HttpPostedFileBase ImageFile)
        {
            if (!CheckAdmin()) return RedirectToLogin();

            if (ModelState.IsValid)
            {
                var existingProduct = db.Products.Find(product.Id);
                if (existingProduct != null)
                {
                    existingProduct.Name = product.Name;
                    existingProduct.Price = product.Price;
                    existingProduct.Description = product.Description;
                    existingProduct.CategoryId = product.CategoryId;
                    existingProduct.Unit = product.Unit;

                    if (ImageFile != null && ImageFile.ContentLength > 0)
                    {
                        string fileName = Path.GetFileName(ImageFile.FileName);
                        string path = Path.Combine(Server.MapPath("~/Images/"), fileName);
                        ImageFile.SaveAs(path);
                        existingProduct.ImageUrl = "~/Images/" + fileName;
                    }

                    db.SaveChanges();
                    TempData["Success"] = "Cập nhật sản phẩm thành công!";
                    return RedirectToAction("Products");
                }
            }
            ViewBag.CategoryId = new SelectList(db.Categories, "Id", "CategoryName", product.CategoryId);
            return View(product);
        }

        public ActionResult Delete(int id)
        {
            if (!CheckAdmin()) return RedirectToLogin();
            var product = db.Products.Find(id);
            if (product != null)
            {
                bool hasOrder = db.OrderDetail.Any(od => od.productId == id);
                if (hasOrder)
                {
                    TempData["Error"] = "Sản phẩm này đang có trong đơn hàng, không thể xóa!";
                }
                else
                {
                    db.Products.Remove(product);
                    db.SaveChanges();
                    TempData["Success"] = "Đã xóa sản phẩm!";
                }
            }
            return RedirectToAction("Products");
        }

        // ==========================================
        // 3. QUẢN LÝ ĐƠN HÀNG
        // ==========================================
        public ActionResult Orders(string keyword, int? status)
        {
            if (!CheckAdmin()) return RedirectToLogin();

            var query = db.Order.AsQueryable();

            if (!string.IsNullOrEmpty(keyword))
            {
                query = query.Where(o => o.ShipName.Contains(keyword) || o.ShipMobile.Contains(keyword));
            }

            if (status.HasValue)
            {
                query = query.Where(o => o.Status == status.Value);
            }

            var orders = query.OrderByDescending(o => o.OrderDate).ToList();

            ViewBag.Keyword = keyword;
            ViewBag.Status = status;

            return View(orders);
        }

        public ActionResult OrderDetails(int id)
        {
            if (!CheckAdmin()) return RedirectToLogin();
            var order = db.Order.Find(id);
            if (order == null) return HttpNotFound();

            ViewBag.Details = db.OrderDetail.Include("Product").Where(d => d.OrderId == id).ToList();
            return View(order);
        }

        [HttpPost]
        public ActionResult UpdateStatus(int id, int status)
        {
            if (!CheckAdmin()) return RedirectToLogin();
            var order = db.Order.Find(id);
            if (order != null)
            {
                order.Status = status;
                db.SaveChanges();
                TempData["Success"] = "Đã cập nhật trạng thái!";
            }
            return RedirectToAction("OrderDetails", new { id = id });
        }

        public ActionResult DeleteOrder(int id)
        {
            if (!CheckAdmin()) return RedirectToLogin();
            var order = db.Order.Find(id);
            if (order != null)
            {
                if (order.Status == 4)
                {
                    var details = db.OrderDetail.Where(d => d.OrderId == id).ToList();
                    db.OrderDetail.RemoveRange(details);
                    db.Order.Remove(order);
                    db.SaveChanges();
                    TempData["Success"] = "Đã xóa đơn hàng!";
                }
                else
                {
                    TempData["Error"] = "Chỉ xóa được đơn hàng ĐÃ HỦY!";
                    return RedirectToAction("OrderDetails", new { id = id });
                }
            }
            return RedirectToAction("Orders");
        }

        // ==========================================
        // 4. QUẢN LÝ DANH MỤC
        // ==========================================
        public ActionResult Categories()
        {
            if (!CheckAdmin()) return RedirectToLogin();
            return View(db.Categories.ToList());
        }

        public ActionResult CreateCategory()
        {
            if (!CheckAdmin()) return RedirectToLogin();
            return View();
        }

        [HttpPost]
        public ActionResult CreateCategory(Category category)
        {
            if (!CheckAdmin()) return RedirectToLogin();
            if (ModelState.IsValid)
            {
                db.Categories.Add(category);
                db.SaveChanges();
                TempData["Success"] = "Thêm danh mục thành công!";
                return RedirectToAction("Categories");
            }
            return View(category);
        }

        public ActionResult EditCategory(int id)
        {
            if (!CheckAdmin()) return RedirectToLogin();
            var cat = db.Categories.Find(id);
            return View(cat);
        }

        [HttpPost]
        public ActionResult EditCategory(Category category)
        {
            if (!CheckAdmin()) return RedirectToLogin();
            if (ModelState.IsValid)
            {
                db.Entry(category).State = EntityState.Modified;
                db.SaveChanges();
                TempData["Success"] = "Cập nhật danh mục thành công!";
                return RedirectToAction("Categories");
            }
            return View(category);
        }

        public ActionResult DeleteCategory(int id)
        {
            if (!CheckAdmin()) return RedirectToLogin();
            var cat = db.Categories.Find(id);
            if (cat != null)
            {
                if (db.Products.Any(p => p.CategoryId == id))
                {
                    TempData["Error"] = "Không thể xóa danh mục đang chứa sản phẩm!";
                }
                else
                {
                    db.Categories.Remove(cat);
                    db.SaveChanges();
                    TempData["Success"] = "Đã xóa danh mục!";
                }
            }
            return RedirectToAction("Categories");
        }

        // ==========================================
        // 5. QUẢN LÝ TÀI KHOẢN (ĐÃ MÃ HÓA PASSWORD)
        // ==========================================
        // Trong AdminController.cs

        public ActionResult Users(string keyword, int? roleId)
        {
            if (!CheckAdmin()) return RedirectToLogin();

            var query = db.Users.AsQueryable();

            // 1. Tìm kiếm theo Tên hoặc Username
            if (!string.IsNullOrEmpty(keyword))
            {
                query = query.Where(u => u.FullName.Contains(keyword) || u.Username.Contains(keyword));
            }

            // 2. Lọc theo quyền hạn (Admin/User)
            if (roleId.HasValue)
            {
                query = query.Where(u => u.RoleId == roleId.Value);
            }

            // Lưu lại giá trị để hiển thị trên View
            ViewBag.Keyword = keyword;
            ViewBag.RoleId = roleId;

            return View(query.OrderByDescending(u => u.Id).ToList());
        }

        public ActionResult CreateUser()
        {
            if (!CheckAdmin()) return RedirectToLogin();
            return View();
        }

        [HttpPost]
        public ActionResult CreateUser(User user)
        {
            if (!CheckAdmin()) return RedirectToLogin();

            if (db.Users.Any(u => u.Username == user.Username))
            {
                ModelState.AddModelError("Username", "Tên đăng nhập này đã tồn tại!");
                return View(user);
            }

            if (ModelState.IsValid)
            {
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(user.PasswordHash);

                db.Users.Add(user);
                db.SaveChanges();
                TempData["Success"] = "Thêm tài khoản thành công!";
                return RedirectToAction("Users");
            }
            return View(user);
        }

        public ActionResult EditUser(int id)
        {
            if (!CheckAdmin()) return RedirectToLogin();
            var user = db.Users.Find(id);
            if (user == null) return HttpNotFound();
            return View(user);
        }

        [HttpPost]
        public ActionResult EditUser(User user)
        {
            if (!CheckAdmin()) return RedirectToLogin();

            var existingUser = db.Users.Find(user.Id);
            if (existingUser != null)
            {
                existingUser.FullName = user.FullName;
                existingUser.RoleId = user.RoleId;

                if (!string.IsNullOrEmpty(user.PasswordHash))
                {
                    existingUser.PasswordHash = BCrypt.Net.BCrypt.HashPassword(user.PasswordHash);
                }

                db.SaveChanges();
                TempData["Success"] = "Cập nhật tài khoản thành công!";
                return RedirectToAction("Users");
            }

            ModelState.AddModelError("", "Không tìm thấy tài khoản.");
            return View(user);
        }

        public ActionResult DeleteUser(int id)
        {
            if (!CheckAdmin()) return RedirectToLogin();

            if (Session["Id"] != null && (int)Session["Id"] == id)
            {
                TempData["Error"] = "Bạn không thể xóa chính mình!";
                return RedirectToAction("Users");
            }

            var user = db.Users.Find(id);
            if (user != null)
            {
                // 1. Kiểm tra xem user có đơn hàng không (giữ nguyên)
                if (db.Order.Any(o => o.userId == id))
                {
                    TempData["Error"] = "Tài khoản đã có đơn hàng, không thể xóa!";
                    return RedirectToAction("Users");
                }

                // =============================================================
                // 2. XÓA GIỎ HÀNG (Sửa lại theo Model Cart của bạn)
                // =============================================================

                // Vì CartId cũng chính là UserId (khóa chính chung) nên ta tìm theo id luôn
                var cart = db.Cart.FirstOrDefault(c => c.CartId == id);

                if (cart != null)
                {
                    // 2a. Xóa chi tiết giỏ hàng trước (CartItems)
                    // Lưu ý: Đảm bảo Model CartItem có cột CartId
                    var cartItems = db.CartItems.Where(ci => ci.CartId == cart.CartId).ToList();

                    if (cartItems.Any())
                    {
                        db.CartItems.RemoveRange(cartItems);
                    }

                    // 2b. Xóa giỏ hàng
                    db.Cart.Remove(cart);
                }
                // =============================================================

                // 3. Xóa User
                db.Users.Remove(user);

                db.SaveChanges();
                TempData["Success"] = "Đã xóa tài khoản và giỏ hàng!";
            }
            return RedirectToAction("Users");
        }
    }
}