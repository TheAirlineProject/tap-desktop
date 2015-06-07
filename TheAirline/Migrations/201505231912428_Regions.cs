using System.Data.Entity.Migrations;

namespace TheAirline.Infrastructure.Migrations
{
    public partial class Regions : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Continents",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(maxLength: 4000),
                        Uid = c.String(maxLength: 4000),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Regions",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Uid = c.String(maxLength: 4000),
                        FuelIndex = c.Double(nullable: false),
                        Continent_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Continents", t => t.Continent_Id)
                .Index(t => t.Continent_Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Regions", "Continent_Id", "dbo.Continents");
            DropIndex("dbo.Regions", new[] { "Continent_Id" });
            DropTable("dbo.Regions");
            DropTable("dbo.Continents");
        }
    }
}
