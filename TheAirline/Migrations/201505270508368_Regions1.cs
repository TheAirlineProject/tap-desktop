using System.Data.Entity.Migrations;

namespace TheAirline.Infrastructure.Migrations
{
    public partial class Regions1 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Regions", "Name", c => c.String(maxLength: 4000));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Regions", "Name");
        }
    }
}
