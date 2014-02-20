namespace NijieDownloader.Library.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class InitialCreate : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.NijieImages",
                c => new
                    {
                        ImageId = c.Int(nullable: false),
                        BigImageUrl = c.String(maxLength: 4000),
                        MediumImageUrl = c.String(maxLength: 4000),
                        ThumbImageUrl = c.String(maxLength: 4000),
                        Title = c.String(maxLength: 4000),
                        Description = c.String(maxLength: 4000),
                        WorkDate = c.DateTime(nullable: false),
                        IsManga = c.Boolean(nullable: false),
                        Referer = c.String(maxLength: 4000),
                        IsFriendOnly = c.Boolean(nullable: false),
                        NuitaCount = c.Int(nullable: false),
                        GoodCount = c.Int(nullable: false),
                        IsAnimated = c.Boolean(nullable: false),
                        Member_MemberId = c.Int(),
                    })
                .PrimaryKey(t => t.ImageId)
                .ForeignKey("dbo.NijieMembers", t => t.Member_MemberId)
                .Index(t => t.Member_MemberId);
            
            CreateTable(
                "dbo.NijieMembers",
                c => new
                    {
                        MemberId = c.Int(nullable: false),
                        UserName = c.String(maxLength: 4000),
                        AvatarUrl = c.String(maxLength: 4000),
                    })
                .PrimaryKey(t => t.MemberId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.NijieImages", "Member_MemberId", "dbo.NijieMembers");
            DropIndex("dbo.NijieImages", new[] { "Member_MemberId" });
            DropTable("dbo.NijieMembers");
            DropTable("dbo.NijieImages");
        }
    }
}
