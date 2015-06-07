using System.Data.Entity.Migrations;

namespace TheAirline.Infrastructure.Migrations
{
    public partial class Settings1 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Settings", "Language", c => c.String(maxLength: 4000));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Settings", "Language");
        }
    }
}
