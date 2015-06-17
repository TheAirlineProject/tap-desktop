using System.Data.Entity.Migrations;

namespace TheAirline.Migrations
{
    public partial class AddCurrencies : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Currencies",
                c => new
                {
                    Id = c.Int(false, true),
                    Name = c.String(maxLength: 4000),
                    Symbol = c.String(maxLength: 4000),
                    Exchange = c.Double(false),
                    From = c.DateTime(false),
                    To = c.DateTime(false),
                    SymbolOnRight = c.Boolean(false)
                })
                .PrimaryKey(t => t.Id);
        }

        public override void Down()
        {
            DropTable("dbo.Currencies");
        }
    }
}