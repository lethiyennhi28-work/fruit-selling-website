using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebBanNongSan.Models
{
    public class Cart
    {
        [Key, ForeignKey("User")]
        public int CartId { get; set; }
        public virtual User User { get; set; }

        // Danh sách sản phẩm trong giỏ
        public virtual ICollection<CartItem> CartItems { get; set; }
    }
}