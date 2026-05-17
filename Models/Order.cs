using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace WebBanNongSan.Models
{
    [Table("Orders")]
    public class Order
    {
        [Key]
        public int Id { get; set; }

        // --- LIÊN KẾT USER ---
        public int userId { get; set; }

        [ForeignKey("userId")]
        public virtual User User { get; set; }

        // --- THÔNG TIN GIAO HÀNG (Đổi tên để khớp với Controller) ---
        [Display(Name = "Tên người nhận")]
        [Required(ErrorMessage = "Vui lòng nhập tên người nhận")]
        public string ShipName { get; set; } // Cũ là Name

        [Display(Name = "Số điện thoại")]
        [Required(ErrorMessage = "Vui lòng nhập số điện thoại")]
        [StringLength(10, MinimumLength = 10, ErrorMessage = "Số điện thoại phải đủ 10 ký tự.")]
        public string ShipMobile { get; set; } // Cũ là SoDienThoai

        [Display(Name = "Địa chỉ giao hàng")]
        [Required(ErrorMessage = "Vui lòng nhập địa chỉ")]
        public string ShipAddress { get; set; } // Cũ là DiaChi

        public string ShipEmail { get; set; }

        [Display(Name = "Ghi chú")]
        public string GhiChu { get; set; }

        // --- THỜI GIAN & TRẠNG THÁI ---
        public DateTime OrderDate { get; set; } = DateTime.Now; // Cũ là NgayTao

        // 1: Mới đặt, 2: Đang giao, 3: Đã giao, 4: Hủy
        public int Status { get; set; } = 1;

        // --- THÔNG TIN THANH TOÁN ---
        public decimal TongTien { get; set; } // Tổng tiền cuối cùng

        // Có thể giữ lại hoặc bỏ tùy nhu cầu của bạn
        public string ThanhToan { get; set; } // Hình thức: COD, CK...

        // --- LIÊN KẾT CHI TIẾT ---
        public virtual ICollection<OrderDetails> OrderDetails { get; set; }
    }
}