using System.Data.Entity.Migrations;

namespace TheAirline.Infrastructure.Migrations
{
    public partial class Settings : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Settings",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        AirportCodeDisplay = c.Int(nullable: false),
                        AutoSave = c.Int(nullable: false),
                        ClearStats = c.Int(nullable: false),
                        CurrencyShorten = c.Boolean(nullable: false),
                        DifficultyDisplay = c.Int(nullable: false),
                        GameSpeed = c.Int(nullable: false),
                        MailsOnAirlineRoutes = c.Boolean(nullable: false),
                        MailsOnBadWeather = c.Boolean(nullable: false),
                        MailsOnLandings = c.Boolean(nullable: false),
                        Mode = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.Settings");
        }
    }
}
