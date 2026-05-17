namespace WebBanNongSan.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddOrderDateStatus : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Orders", "OrderDate", c => c.DateTime(nullable: false));
            AddColumn("dbo.Orders", "Status", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Orders", "Status");
            DropColumn("dbo.Orders", "OrderDate");
        }
    }
}
