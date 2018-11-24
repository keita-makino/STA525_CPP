namespace BTC.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _20171112_3 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.TradeRecords", "BchStatus", c => c.Int(nullable: false));
            DropColumn("dbo.TradeRecords", "LongSlope_BT");
        }
        
        public override void Down()
        {
            AddColumn("dbo.TradeRecords", "LongSlope_BT", c => c.Double(nullable: false));
            DropColumn("dbo.TradeRecords", "BchStatus");
        }
    }
}
