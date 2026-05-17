using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebBanNongSan.Models;
using System.Data.Entity;
using System.Text.RegularExpressions;

namespace WebBanNongSan.Controllers
{
    public class ProductController : Controller
    {
        AppDbContext db = new AppDbContext();

        // --- HÀM HỖ TRỢ: CHUYỂN TIẾNG VIỆT CÓ DẤU THÀNH KHÔNG DẤU ---
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

        // --- ACTION INDEX: KẾT HỢP TÌM KIẾM VÀ BỘ LỌC ---
        public ActionResult Index(string search, string sortBy,
                                  List<int> categoryIds,
                                  List<string> priceRanges,
                                  List<string> origins,
                                  List<string> units,
                                  int page = 1)
        {
            // Bước 1: Khởi tạo Query SQL
            var query = db.Products.Include("Category").AsQueryable();

            // Bước 2: Lọc trên SQL (Database) - Giúp giảm tải dữ liệu lấy về
            // Kết hợp điều kiện: Danh mục AND Đơn vị

            if (categoryIds != null && categoryIds.Any())
            {
                query = query.Where(p => categoryIds.Contains(p.CategoryId));
            }

            if (units != null && units.Any())
            {
                query = query.Where(p => units.Contains(p.Unit));
            }

            // [LƯU Ý QUAN TRỌNG]
            // Mình tạm thời comment phần này lại vì Model Product của bạn chưa có cột Origin.
            // Nếu mở ra sẽ bị lỗi CS1061. Hãy thêm cột Origin vào Database rồi mới mở dòng này ra.
            /*
            if (origins != null && origins.Any())
            {
                query = query.Where(p => origins.Contains(p.Origin));
            }
            */

            // Bước 3: Đưa dữ liệu về RAM (Server)
            // Từ dòng này trở đi, chúng ta xử lý trên danh sách List<Product> trong bộ nhớ
            var listProducts = query.ToList();

            // Bước 4: KẾT HỢP Tìm kiếm (Search) - Xử lý tiếng Việt không dấu
            if (!string.IsNullOrEmpty(search))
            {
                string searchKey = RemoveSign4VietnameseString(search.ToLower());

                // Lọc trên danh sách đã có ở Bước 3
                listProducts = listProducts.Where(x =>
                    RemoveSign4VietnameseString(x.Name.ToLower()).Contains(searchKey)
                ).ToList();
            }

            // Bước 5: KẾT HỢP Lọc theo giá (Price Range)
            // Tiếp tục lọc trên danh sách kết quả của Bước 4
            if (priceRanges != null && priceRanges.Any())
            {
                listProducts = listProducts.Where(p =>
                    (priceRanges.Contains("under50") && p.Price < 50000) ||
                    (priceRanges.Contains("50to100") && p.Price >= 50000 && p.Price <= 100000) ||
                    (priceRanges.Contains("above100") && p.Price > 100000)
                ).ToList();
            }

            // Bước 6: Sắp xếp
            switch (sortBy)
            {
                case "name":
                    listProducts = listProducts.OrderBy(x => x.Name).ToList();
                    break;
                case "price_asc":
                    listProducts = listProducts.OrderBy(x => x.Price).ToList();
                    break;
                case "price_desc":
                    listProducts = listProducts.OrderByDescending(x => x.Price).ToList();
                    break;
                default:
                    listProducts = listProducts.OrderByDescending(x => x.Id).ToList();
                    break;
            }

            // Bước 7: Phân trang
            int pageSize = 9;
            int totalProducts = listProducts.Count();
            int totalPages = (int)Math.Ceiling((double)totalProducts / pageSize);

            if (page < 1) page = 1;
            if (page > totalPages && totalPages > 0) page = totalPages;

            var pageData = listProducts
                            .Skip((page - 1) * pageSize)
                            .Take(pageSize)
                            .ToList();

            // Truyền dữ liệu sang View để giữ trạng thái các ô checkbox/input
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.Search = search;
            ViewBag.SortBy = sortBy;

            return View(pageData);
        }

        public ActionResult Details(int id)
        {
            var product = db.Products.Include("Category").FirstOrDefault(x => x.Id == id);

            if (product == null) return HttpNotFound();

            var related = db.Products
                            .Where(p => p.Id != id && p.CategoryId == product.CategoryId)
                            .OrderBy(x => Guid.NewGuid())
                            .Take(4)
                            .ToList();

            ViewBag.Related = related;

            return View(product);
        }
    }
}