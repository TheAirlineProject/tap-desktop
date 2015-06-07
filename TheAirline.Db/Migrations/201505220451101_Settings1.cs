namespace TheAirline.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
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
