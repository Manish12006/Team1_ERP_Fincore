using System;

namespace Fincore.Application.DTO.Payment.AccountsReceivable.Responses
{
    public class AROutstandingResponseDto
    {
        public int ARInvoiceId { get; set; }

        public string InvoiceNumber { get; set; }

        public string CustomerName { get; set; }

        public DateTime DueDate { get; set; }

        public decimal Amount { get; set; }

        public decimal? AmountReceived { get; set; }

        public decimal? AmountOutstanding { get; set; }

        public string PaymentStatus { get; set; }
    }
}