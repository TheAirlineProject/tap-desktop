namespace TheAirline.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Players : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Players",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        MajorAirports = c.Boolean(nullable: false),
                        StartYear = c.Int(nullable: false),
                        Focus = c.Int(nullable: false),
                        NumOfOpponents = c.Int(nullable: false),
                        RandomOpponents = c.Boolean(nullable: false),
                        SameRegion = c.Boolean(nullable: false),
                        RealData = c.Boolean(nullable: false),
                        UseDays = c.Boolean(nullable: false),
                        Paused = c.Boolean(nullable: false),
                        Continent_Id = c.Int(),
                        Difficulty_Id = c.Int(),
                        Region_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Continents", t => t.Continent_Id)
                .ForeignKey("dbo.Difficulties", t => t.Difficulty_Id)
                .ForeignKey("dbo.Regions", t => t.Region_Id)
                .Index(t => t.Continent_Id)
                .Index(t => t.Difficulty_Id)
                .Index(t => t.Region_Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Players", "Region_Id", "dbo.Regions");
            DropForeignKey("dbo.Players", "Difficulty_Id", "dbo.Difficulties");
            DropForeignKey("dbo.Players", "Continent_Id", "dbo.Continents");
            DropIndex("dbo.Players", new[] { "Region_Id" });
            DropIndex("dbo.Players", new[] { "Difficulty_Id" });
            DropIndex("dbo.Players", new[] { "Continent_Id" });
            DropTable("dbo.Players");
        }
    }
}
