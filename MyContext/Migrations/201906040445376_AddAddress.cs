namespace DbContextTests.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddAddress : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Users", "Address_City", c => c.String());
            AddColumn("dbo.Users", "Address_Street", c => c.String());
            AddColumn("dbo.Users", "Address_HouseNo", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Users", "Address_HouseNo");
            DropColumn("dbo.Users", "Address_Street");
            DropColumn("dbo.Users", "Address_City");
        }
    }
}
