namespace NijieDownloader.Library.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _1031 : DbMigration
    {
        public override void Up()
        {
            //CreateIndex("dbo.NijieImages", "Member_MemberId");
            //CreateIndex("dbo.NijieTagNijieImages", "NijieTag_Name");
            //CreateIndex("dbo.NijieTagNijieImages", "NijieImage_ImageId");
        }
        
        public override void Down()
        {
            //DropIndex("dbo.NijieTagNijieImages", new[] { "NijieImage_ImageId" });
            //DropIndex("dbo.NijieTagNijieImages", new[] { "NijieTag_Name" });
            //DropIndex("dbo.NijieImages", new[] { "Member_MemberId" });
        }
    }
}
