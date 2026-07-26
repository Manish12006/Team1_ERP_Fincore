using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Fincore.Domain.Models
{
    public class Document
    {
        [Key]
        public int DocumentsId { get; set; }

        [Required]
        [ForeignKey("DocumentType")]
        public int DocumentTypeId { get; set; }
        public DocumentType DocumentType { get; set; }

        [Required]
        [ForeignKey("User")]
        public int UserId { get; set; }
        public User User { get; set; }

        [Column("Emp/Vendor/Customer")]
        public int? EntityId { get; set; }

        [ForeignKey("MasterType")]
        public int? MasterTypeId { get; set; }
        public MasterType MasterType { get; set; }

        [Required]
        [StringLength(100)]
        public string FilePath { get; set; }

        [Required]
        [StringLength(100)]
        public string FileType { get; set; }

        [Required]
        [StringLength(100)]
        public string FileName { get; set; }

        public DateTime? CreatedAt { get; set; }
        public DateTime? ModifiedAt { get; set; }
        
    }
}
