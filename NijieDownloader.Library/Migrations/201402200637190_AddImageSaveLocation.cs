namespace NijieDownloader.Library.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddImageSaveLocation : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.NijieImages", "SavedFilename", c => c.String(maxLength: 4000));
        }
        
        public override void Down()
        {
            DropColumn("dbo.NijieImages", "SavedFilename");
        }
    }
}
