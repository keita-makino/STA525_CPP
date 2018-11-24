namespace BTC.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Initial : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.TradeRecords",
                c => new
                    {
                        timestamp = c.Long(nullable: false),
                        last = c.Double(nullable: false),
                        ShortTrend = c.Double(nullable: false),
                        MidTrend = c.Double(nullable: false),
                        LongTrend = c.Double(nullable: false),
                    })
                .PrimaryKey(t => t.timestamp);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.TradeRecords");
        }
    }
}
