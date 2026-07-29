using AutoMapper;
using AutoMapper.QueryableExtensions;
using Fincore.Application.DTO;
using Fincore.Application.DTO.Capex;
using Fincore.Application.Interfaces.ICapex;
using Fincore.Domain.Models;
using Fincore.Infrastructure.CommonHelper;
using Fincore.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fincore.Infrastructure.Services.Capex
{
    public class PRItemService : IPRItemService
    {
        private readonly AppDbContext db;
        IMapper map;
        IMemoryCache memoryCache;
        public PRItemService(AppDbContext db, IMapper map, IMemoryCache memoryCache)
        {
            this.db = db;
            this.map = map;
            this.memoryCache = memoryCache;
        }

        public async Task<ApiResponse<string>> CreatePRItem(PRItemDTOPost pr)
        {

            var add = map.Map<PurchaseRequisitionItem>(pr);
            add.TaxAmount = CalculateTaxAmount(add.Quantity, add.EstimatedUnitPrice.Value, add.TaxPercentage);
            add.LineTotal = CalculateLineTotal(add.Quantity, add.EstimatedUnitPrice.Value, add.TaxAmount);

            await db.PurchaseRequisitionItems.AddAsync(add);
            var result = await db.SaveChangesAsync();
            memoryCache.Remove(pr);

            if (result > 0)
            {
                return ApiResponseHelper.SuccessRes(
                      result.ToString(), "Purchase Requisition Item added successfully");
            }
            else
            {
                return ApiResponseHelper.Failure<string>(
                     "Purchase Requisition Item add failed", "ERROR_OCCURED", "Failed to add Purchase Requisition Item");
            }
        }

        public async Task<ApiResponse<string>> DeletePRItem(int id)
        {
            var pr = await db.PurchaseRequisitionItems.FindAsync(id);

            if (pr == null)
            {
                return ApiResponseHelper.Failure<string>(
                                   "Cannot Delete Purchase Requistion Item", "NOT_FOUND", $"Purchase Requistion Item with id {id} Not found");

            }

            if (pr.ItemStatus == "Cancelled")
            {
                return ApiResponseHelper.Failure<string>(
                                                   "Purchase Requistion Item already Deleted", "ALREADY_DELETED", $"Purchase Requistion Item with id {id} has already been deleted");
            }

            bool hasQuotation = await db.QuotationItems.AnyAsync(x=> x.PurchaseRequisitionItem.PRItemId == id);
            if (hasQuotation) 
            {
                return ApiResponseHelper.Failure<string>(
                                  "Cannot Delete Purchase Requistion Item", "DELETE_RESTRICTED", $"Purchase Requistion Item with id {id} cannot be deleted because its linked to Quotation Item");

            }

            pr.ItemStatus = "Cancelled";
            await db.SaveChangesAsync();

            return ApiResponseHelper.SuccessRes($"Purchase Requistion Item Deleted Successfully with id {id}");

        }

        public async Task<ApiResponse<List<PRItemDTOGet>>> GetPRItem(int page, int pagesize)
        {
            string cacheKey = $"PR_{page}_{pagesize}";

            if (memoryCache.TryGetValue(cacheKey, out List<PRItemDTOGet> PRlist))
            {
                return ApiResponseHelper.SuccessRes(
                    PRlist, "Purchase Requistion Items fetched successfully", PRlist.Count, new { page = page, pagesize = pagesize });
            }

            if (page < 1)
            {
                return ApiResponseHelper.Failure<List<PRItemDTOGet>>(
                    "Invalid page number.", "INVALID_PAGE", "Page number must be greater than or equal to 1.");
            }

            if (pagesize < 1)
            {
                return ApiResponseHelper.Failure<List<PRItemDTOGet>>(
                    "Invalid page size.", "INVALID_PAGE_SIZE", "Page size must be greater than or equal to 1.");
            }

            PRlist = await db.PurchaseRequisitionItems
                .Where(x=> x.ItemStatus != "Cancelled")
                .Skip((page - 1) * pagesize).Take(pagesize)
                .ProjectTo<PRItemDTOGet>(map.ConfigurationProvider)
                .ToListAsync();

            if (PRlist == null & !PRlist.Any())
            {
                return ApiResponseHelper.Failure<List<PRItemDTOGet>>(
                    "Purchase Requistion Items not found", "EMPTY_DATA", "No Data to show");
            }

            memoryCache.Set(cacheKey, PRlist);

            return ApiResponseHelper.SuccessRes(
                    PRlist, "Purchase Requistion Items fetched successfully", PRlist.Count, new { page = page, pagesize = pagesize });

        }

        public async Task<ApiResponse<PRItemDTOGet>> GetPRItemById(int id)
        {
            string cacheKey = $"{id}";

            if (memoryCache.TryGetValue(cacheKey, out PRItemDTOGet pr))
            {
                return ApiResponseHelper.SuccessRes(
                    pr, $"Purchase Requistion Item with Id {id} found", 1);
            }

            pr = await db.PurchaseRequisitionItems
                .Where(x => x.PRItemId == id && x.ItemStatus != "Cancelled")
                .ProjectTo<PRItemDTOGet>(map.ConfigurationProvider)
                .FirstOrDefaultAsync();

            if (pr == null)
            {
                return ApiResponseHelper.Failure<PRItemDTOGet>(
                    "Purchase Requistion Item Record not found", "NOT_FOUND", $"Purchase Requistion Item with Id {id} not found");
            }

            return ApiResponseHelper.SuccessRes(
                    pr, $"Purchase Requistion Item with Id {id} found", 1);
        }

        public async Task<ApiResponse<string>> UpdatePRItem(int id, PRItemDTOPost pr)
        {
            var update = await db.PurchaseRequisitionItems.FirstOrDefaultAsync(x => x.PRItemId == id && x.ItemStatus != "Cancelled");

            if (update == null)
            {
                return ApiResponseHelper.Failure<string>(
                    "Purchase Requistion Item Not Found", "NOT_FOUND", $"Purchase Requistion Item with id {id} Not found");
            }

            if (update.ItemStatus != "Open")
            {
                return ApiResponseHelper.Failure<string>(
                    "Cannot Update Item",
                    "INVALID_STATUS",
                    "Items can only be updated when the Purchase Requisition Item is in Open status.");
            }

            update.TaxAmount = CalculateTaxAmount(update.Quantity, update.EstimatedUnitPrice.Value, update.TaxPercentage);
            update.LineTotal = CalculateLineTotal(update.Quantity, update.EstimatedUnitPrice.Value, update.TaxAmount);

            map.Map(pr, update);
            await db.SaveChangesAsync();

            string cache = $"{id}";
            memoryCache.Remove(cache);

            return ApiResponseHelper.SuccessRes($"Purchase Requistion Item Updated Successfully with id {id}");

        }

        private decimal CalculateTaxAmount(decimal quantity, decimal estimatedUnitPrice, decimal taxPercentage)
        {
            return (quantity * estimatedUnitPrice * taxPercentage) / 100;
        }

        private decimal CalculateLineTotal(decimal quantity, decimal estimatedUnitPrice, decimal taxAmount)
        {
            return (quantity * estimatedUnitPrice) + taxAmount;
        }

    }
}
