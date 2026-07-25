using System;
using System.ComponentModel.DataAnnotations;

namespace Fincore.Application.DTO.Payment.AccountsReceivable.Requests
{
    public class CreateARPaymentRequestDto
    {
        [Required]
        public int ARInvoiceId { get; set; }

        [Required]
        public decimal Amount { get; set; }

        [Required]
        public DateTime PaymentDate { get; set; }

        [Required]
        public string PaymentMethod { get; set; }
    }
}