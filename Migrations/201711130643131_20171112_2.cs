namespace BTC.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _20171112_2 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.TradeRecords", "Before_BT", c => c.Double(nullable: false));
            AddColumn("dbo.TradeRecords", "After_BT", c => c.Double(nullable: false));
            AddColumn("dbo.TradeRecords", "ShortSlope_BT", c => c.Double(nullable: false));
            AddColumn("dbo.TradeRecords", "LongSlope_BT", c => c.Double(nullable: false));
            DropColumn("dbo.TradeRecords", "ShortTrend_BT");
            DropColumn("dbo.TradeRecords", "LongTrend_BT");
        }
        
        public override void Down()
        {
            AddColumn("dbo.TradeRecords", "LongTrend_BT", c => c.Double(nullable: false));
            AddColumn("dbo.TradeRecords", "ShortTrend_BT", c => c.Double(nullable: false));
            DropColumn("dbo.TradeRecords", "LongSlope_BT");
            DropColumn("dbo.TradeRecords", "ShortSlope_BT");
            DropColumn("dbo.TradeRecords", "After_BT");
            DropColumn("dbo.TradeRecords", "Before_BT");
        }
    }
}
