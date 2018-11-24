namespace BTC.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _20171112_4 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.TradeRecords", "last", c => c.Double(nullable: false));
            DropColumn("dbo.TradeRecords", "last_CC");
        }
        
        public override void Down()
        {
            AddColumn("dbo.TradeRecords", "last_CC", c => c.Double(nullable: false));
            DropColumn("dbo.TradeRecords", "last");
        }
    }
}
