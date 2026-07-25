using System;

namespace Fincore.Application.DTO.Payment.AccountsReceivable.Responses
{
    public class ARPaymentResponseDto
    {
        public int ARInvoiceId { get; set; }

        public string InvoiceNumber { get; set; }

        public decimal PaidAmount { get; set; }

        public decimal OutstandingAmount { get; set; }

        public string PaymentStatus { get; set; }

        public DateTime PaymentDate { get; set; }
    }
}