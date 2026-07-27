using System.ComponentModel.DataAnnotations;

namespace Fincore.Application.DTO.Capex
{
    public class GRNDTO
    {
        public int GRNId { get; set; }


        [Required(ErrorMessage = "GRN Code is required")]
        [StringLength(30)]
        public string GRNCode { get; set; }



        [Required(ErrorMessage = "Purchase Order is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Invalid PO Id")]
        public int POId { get; set; }



        [Required(ErrorMessage = "Vendor is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Invalid Vendor Id")]
        public int VendorId { get; set; }



        public byte IsActive { get; set; }



        public DateTime? ReceivedDate { get; set; }



        [Required(ErrorMessage = "Received By is required")]
        [Range(1, int.MaxValue)]
        public int ReceivedBy { get; set; }



        public string QualityCheckStatus { get; set; }



        public int? QualityCheckedBy { get; set; }



        public string GRNStatus { get; set; }



        [StringLength(500)]
        public string Remarks { get; set; }



        public DateTime? CreatedAt { get; set; }


        public DateTime? ModifiedAt { get; set; }



        [Required(ErrorMessage = "Created By is required")]
        [Range(1, int.MaxValue)]
        public int CreatedBy { get; set; }

    }
}