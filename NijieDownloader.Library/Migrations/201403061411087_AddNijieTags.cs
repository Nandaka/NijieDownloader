namespace NijieDownloader.Library.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddNijieTags : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.NijieTags",
                c => new
                    {
                        Name = c.String(nullable: false, maxLength: 4000),
                    })
                .PrimaryKey(t => t.Name);
            
            CreateTable(
                "dbo.NijieTagNijieImages",
                c => new
                    {
                        NijieTag_Name = c.String(nullable: false, maxLength: 4000),
                        NijieImage_ImageId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.NijieTag_Name, t.NijieImage_ImageId })
                .ForeignKey("dbo.NijieTags", t => t.NijieTag_Name, cascadeDelete: true)
                .ForeignKey("dbo.NijieImages", t => t.NijieImage_ImageId, cascadeDelete: true)
                .Index(t => t.NijieTag_Name)
                .Index(t => t.NijieImage_ImageId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.NijieTagNijieImages", "NijieImage_ImageId", "dbo.NijieImages");
            DropForeignKey("dbo.NijieTagNijieImages", "NijieTag_Name", "dbo.NijieTags");
            DropIndex("dbo.NijieTagNijieImages", new[] { "NijieImage_ImageId" });
            DropIndex("dbo.NijieTagNijieImages", new[] { "NijieTag_Name" });
            DropTable("dbo.NijieTagNijieImages");
            DropTable("dbo.NijieTags");
        }
    }
}
