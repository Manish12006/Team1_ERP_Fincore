using System;
using System.ComponentModel.DataAnnotations;

namespace Fincore.Application.DTO.Payment.AccountsReceivable.Requests
{
    public class CreateARInvoiceRequestDto
    {
        [Required]
        public int CustomerId { get; set; }

        [Required]
        public int RevenueEntryId { get; set; }

        [Required]
        public DateTime InvoiceDate { get; set; }

        [Required]
        public DateTime DueDate { get; set; }

        [Required]
        public decimal Amount { get; set; }
    }
}