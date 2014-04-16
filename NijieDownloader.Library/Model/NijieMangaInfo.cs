using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NijieDownloader.Library.Model
{
    public class NijieMangaInfo
    {        
        public NijieImage Image { get; set; }
        
        [Key, Column(Order = 0)]
        [DatabaseGenerated(System.ComponentModel.DataAnnotations.Schema.DatabaseGeneratedOption.None)]
        public int ImageId { get; set; }

        [Key, Column(Order = 1)]
        [DatabaseGenerated(System.ComponentModel.DataAnnotations.Schema.DatabaseGeneratedOption.None)]
        public int Page { get; set; }

        public string SavedFilename { get; set; }
        public string ServerFilename { get; set; }
        public long Filesize { get; set; }
        public string ImageUrl { get; set; }
    }
}
