namespace BTC.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _20171115 : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Configs",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Switch = c.Boolean(nullable: false),
                        Threshold_High = c.Double(nullable: false),
                        Threshold_Low = c.Double(nullable: false),
                        Multiplier = c.Double(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.Configs");
        }
    }
}
