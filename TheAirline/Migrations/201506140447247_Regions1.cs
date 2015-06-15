namespace TheAirline.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Regions1 : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Difficulties",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(maxLength: 4000),
                        AiLevel = c.Double(nullable: false),
                        MoneyLevel = c.Double(nullable: false),
                        LoanLevel = c.Double(nullable: false),
                        PassengersLevel = c.Double(nullable: false),
                        PriceLevel = c.Double(nullable: false),
                        StartDataLevel = c.Double(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            AddColumn("dbo.Regions", "Name", c => c.String(maxLength: 4000));
            AddColumn("dbo.Settings", "DifficultyDisplay_Id", c => c.Int());
            CreateIndex("dbo.Settings", "DifficultyDisplay_Id");
            AddForeignKey("dbo.Settings", "DifficultyDisplay_Id", "dbo.Difficulties", "Id");
            DropColumn("dbo.Settings", "DifficultyDisplay");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Settings", "DifficultyDisplay", c => c.Int(nullable: false));
            DropForeignKey("dbo.Settings", "DifficultyDisplay_Id", "dbo.Difficulties");
            DropIndex("dbo.Settings", new[] { "DifficultyDisplay_Id" });
            DropColumn("dbo.Settings", "DifficultyDisplay_Id");
            DropColumn("dbo.Regions", "Name");
            DropTable("dbo.Difficulties");
        }
    }
}
