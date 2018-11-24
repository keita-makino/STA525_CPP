namespace BTC.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _20171112 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.TradeRecords", "last_CC", c => c.Double(nullable: false));
            AddColumn("dbo.TradeRecords", "ShortTrend_CC", c => c.Double(nullable: false));
            AddColumn("dbo.TradeRecords", "LongTrend_CC", c => c.Double(nullable: false));
            AddColumn("dbo.TradeRecords", "last_BT", c => c.Double(nullable: false));
            AddColumn("dbo.TradeRecords", "ShortTrend_BT", c => c.Double(nullable: false));
            AddColumn("dbo.TradeRecords", "LongTrend_BT", c => c.Double(nullable: false));
            DropColumn("dbo.TradeRecords", "last");
            DropColumn("dbo.TradeRecords", "ShortTrend");
            DropColumn("dbo.TradeRecords", "MidTrend");
            DropColumn("dbo.TradeRecords", "LongTrend");
        }
        
        public override void Down()
        {
            AddColumn("dbo.TradeRecords", "LongTrend", c => c.Double(nullable: false));
            AddColumn("dbo.TradeRecords", "MidTrend", c => c.Double(nullable: false));
            AddColumn("dbo.TradeRecords", "ShortTrend", c => c.Double(nullable: false));
            AddColumn("dbo.TradeRecords", "last", c => c.Double(nullable: false));
            DropColumn("dbo.TradeRecords", "LongTrend_BT");
            DropColumn("dbo.TradeRecords", "ShortTrend_BT");
            DropColumn("dbo.TradeRecords", "last_BT");
            DropColumn("dbo.TradeRecords", "LongTrend_CC");
            DropColumn("dbo.TradeRecords", "ShortTrend_CC");
            DropColumn("dbo.TradeRecords", "last_CC");
        }
    }
}
