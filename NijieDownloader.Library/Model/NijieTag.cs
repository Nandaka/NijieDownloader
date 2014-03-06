using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NijieDownloader.Library.Model
{
    public class NijieTag
    {
        [Key]
        [DatabaseGenerated(System.ComponentModel.DataAnnotations.Schema.DatabaseGeneratedOption.None)]
        public string Name { get; set; }
        public virtual ICollection<NijieImage> Images { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }
}
