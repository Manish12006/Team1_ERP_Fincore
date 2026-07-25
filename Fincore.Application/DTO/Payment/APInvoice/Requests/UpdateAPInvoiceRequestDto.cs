

namespace Fincore.Application.DTO.Payment.APInvoice.Requests
{
    public class UpdateAPInvoiceRequestDto
    {
        public int VendorId { get; set; }

        public int PurchaseOrderId { get; set; }

        public int GRNId { get; set; }

        public decimal InvoiceAmount { get; set; }

        public DateTime InvoiceDate { get; set; }

        public DateTime DueDate { get; set; }
    }
}
