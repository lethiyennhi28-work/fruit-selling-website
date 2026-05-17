namespace WebBanNongSan.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class FixOrderModel : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Orders", "ShipName", c => c.String(nullable: false));
            AddColumn("dbo.Orders", "ShipMobile", c => c.String(nullable: false, maxLength: 10));
            AddColumn("dbo.Orders", "ShipAddress", c => c.String(nullable: false));
            AddColumn("dbo.Orders", "ShipEmail", c => c.String());
            AlterColumn("dbo.Orders", "GhiChu", c => c.String());
            AlterColumn("dbo.Orders", "ThanhToan", c => c.String());
            DropColumn("dbo.Orders", "Name");
            DropColumn("dbo.Orders", "NgayTao");
            DropColumn("dbo.Orders", "DiaChi");
            DropColumn("dbo.Orders", "SoDienThoai");
            DropColumn("dbo.Orders", "TongTienHang");
            DropColumn("dbo.Orders", "TienShip");
            DropColumn("dbo.Orders", "GiaoHang");
            DropColumn("dbo.Orders", "TrangThai");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Orders", "TrangThai", c => c.String(nullable: false, maxLength: 50));
            AddColumn("dbo.Orders", "GiaoHang", c => c.String(maxLength: 100));
            AddColumn("dbo.Orders", "TienShip", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AddColumn("dbo.Orders", "TongTienHang", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AddColumn("dbo.Orders", "SoDienThoai", c => c.String(nullable: false, maxLength: 10));
            AddColumn("dbo.Orders", "DiaChi", c => c.String(nullable: false, maxLength: 255));
            AddColumn("dbo.Orders", "NgayTao", c => c.DateTime(nullable: false));
            AddColumn("dbo.Orders", "Name", c => c.String(nullable: false));
            AlterColumn("dbo.Orders", "ThanhToan", c => c.String(maxLength: 100));
            AlterColumn("dbo.Orders", "GhiChu", c => c.String(maxLength: 500));
            DropColumn("dbo.Orders", "ShipEmail");
            DropColumn("dbo.Orders", "ShipAddress");
            DropColumn("dbo.Orders", "ShipMobile");
            DropColumn("dbo.Orders", "ShipName");
        }
    }
}
