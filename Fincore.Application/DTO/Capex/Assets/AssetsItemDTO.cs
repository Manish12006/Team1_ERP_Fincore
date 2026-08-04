using Fincore.Domain.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fincore.Application.DTO.Capex.Assets
{
    public class AssetsItemDTO
    {
        public int AssetId { get; set; }
        public string AssetCode { get; set; }
        public string AssetName { get; set; }
        public int CapexRequestId { get; set; }
        public int PurchaseOrderId { get; set; }
        public int GRNId { get; set; }
        public int VendorId { get; set; }
        public int DepartmentId { get; set; }
        public DateTime PurchaseDate { get; set; }
        public decimal? PurchaseCost { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ModifiedAt { get; set; }
    }
}
