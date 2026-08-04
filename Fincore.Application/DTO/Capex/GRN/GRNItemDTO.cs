using Fincore.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fincore.Application.DTO.Capex.GRN
{
    public class GRNItemDTO
    {
        public int GRNId { get; set; }
        public string GRNCode { get; set; }
        public int POId { get; set; }
        public int VendorId { get; set; }
        public byte IsActive { get; set; }
        public DateTime ReceivedDate { get; set; }
        public int ReceivedBy { get; set; }
        public User ReceivedByUser { get; set; }
        public string QualityCheckStatus { get; set; }
        public int QualityCheckedBy { get; set; }
        public string GRNStatus { get; set; }
        public string Remarks { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ModifiedAt { get; set; }
        public int CreatedBy { get; set; }
    }
}
