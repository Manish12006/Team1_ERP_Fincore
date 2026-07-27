using System;
using System.ComponentModel.DataAnnotations;

namespace Fincore.Application.DTO.Capex
{
    public class AssetDTO
    {

        public int AssetId { get; set; }



        [Required(ErrorMessage = "Asset Code is required")]
        [StringLength(50)]
        public string? AssetCode { get; set; }



        [Required(ErrorMessage = "Asset Name is required")]
        [StringLength(40)]
        public string? AssetName { get; set; }



        public int? CapexRequestId { get; set; }


        public int? PurchaseOrderId { get; set; }


        public int? GRNId { get; set; }


        public int? VendorId { get; set; }


        public int? DepartmentId { get; set; }



        public DateTime? PurchaseDate { get; set; }



        [Range(typeof(decimal),"0.01","999999999",
        ErrorMessage = "Purchase Cost must be greater than zero")]
        public decimal? PurchaseCost { get; set; }




        public string? Status { get; set; }



        public DateTime? CreatedAt { get; set; }



        public DateTime? ModifiedAt { get; set; }

    }
}