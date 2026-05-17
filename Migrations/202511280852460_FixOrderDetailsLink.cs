namespace WebBanNongSan.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class FixOrderDetailsLink : DbMigration
    {
        public override void Up()
        {
            CreateIndex("dbo.OrderDetails", "productId");
            AddForeignKey("dbo.OrderDetails", "productId", "dbo.Product", "Id", cascadeDelete: true);
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.OrderDetails", "productId", "dbo.Product");
            DropIndex("dbo.OrderDetails", new[] { "productId" });
        }
    }
}
