namespace Solomon.WebUI.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UpdateUserProfile : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.UserProfile", "LastAccessTime", c => c.DateTime());
            DropColumn("dbo.UserProfile", "LastLoginTime");
        }
        
        public override void Down()
        {
            AddColumn("dbo.UserProfile", "LastLoginTime", c => c.DateTime());
            DropColumn("dbo.UserProfile", "LastAccessTime");
        }
    }
}
