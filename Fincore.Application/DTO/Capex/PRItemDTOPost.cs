using Fincore.Domain.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fincore.Application.DTO.Capex
{
    public class PRItemDTOPost
    {
        public int PRItemId { get; set; }
        public int PurchaseRequisitionId { get; set; }
        public string ItemName { get; set; }
        public string ItemDescription { get; set; }
        public int CategoryId { get; set; }
        public decimal Quantity { get; set; }
        public string UnitOfMaterial { get; set; }

        public decimal? EstimatedUnitPrice { get; set; }
        public decimal TaxPercentage { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal LineTotal { get; set; }
        public string ItemStatus { get; set; }

    }
}
