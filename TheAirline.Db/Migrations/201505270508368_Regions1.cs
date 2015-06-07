namespace TheAirline.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
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
