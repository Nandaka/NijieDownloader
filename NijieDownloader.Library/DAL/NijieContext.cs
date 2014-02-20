using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Entity;
using NijieDownloader.Library.Model;

namespace NijieDownloader.Library.DAL
{
    public class NijieContext : DbContext
    {
        public NijieContext()
        {
            //Database.SetInitializer(new MigrateDatabaseToLatestVersion<NijieContext, Migrations.Configuration>()); 
        }

        public DbSet<NijieImage> Images { get; set; }
        public DbSet<NijieMember> Members { get; set; }
    }
}
