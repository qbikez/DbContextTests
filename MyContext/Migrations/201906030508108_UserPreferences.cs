namespace DbContextTests.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UserPreferences : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.UserPreferences",
                c => new
                    {
                        UserId = c.Int(nullable: false),
                        FavoriteProduct = c.String(),
                    })
                .PrimaryKey(t => t.UserId)
                .ForeignKey("dbo.Users", t => t.UserId)
                .Index(t => t.UserId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.UserPreferences", "UserId", "dbo.Users");
            DropIndex("dbo.UserPreferences", new[] { "UserId" });
            DropTable("dbo.UserPreferences");
        }
    }
}
