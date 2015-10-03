namespace Solomon.WebUI.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddCheckPendingToProblem : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Problem", "CheckPending", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Problem", "CheckPending");
        }
    }
}
