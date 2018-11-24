namespace BTC.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _20171112_5 : DbMigration
    {
        public override void Up()
        {
            DropPrimaryKey("dbo.TradeRecords");
            AddColumn("dbo.TradeRecords", "id", c => c.Long(nullable: false, identity: true));
            AddPrimaryKey("dbo.TradeRecords", "id");
        }
        
        public override void Down()
        {
            DropPrimaryKey("dbo.TradeRecords");
            DropColumn("dbo.TradeRecords", "id");
            AddPrimaryKey("dbo.TradeRecords", "timestamp");
        }
    }
}
