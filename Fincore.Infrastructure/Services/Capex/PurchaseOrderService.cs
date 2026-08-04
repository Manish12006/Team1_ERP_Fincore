using AutoMapper;
using Fincore.Application.DTO;
using Fincore.Application.DTO.Capex;
using Fincore.Application.Interfaces.ICapex;
using Fincore.Domain.Enums;
using Fincore.Domain.Models;
using Fincore.Infrastructure.CommonHelper;
using Fincore.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
//using Fincore.Application.Constants;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Document = QuestPDF.Fluent.Document;

namespace Fincore.Infrastructure.Services.Capex
{
    public class PurchaseOrderService : IPurchaseOrderService
    {
        private readonly AppDbContext db;
        private readonly IMapper mapper;
        private readonly IMemoryCache cache;

        private const string PurchaseOrderCacheKey = "PurchaseOrder";

        public PurchaseOrderService(AppDbContext db, IMapper mapper, IMemoryCache cache)
        {
            this.db = db;
            this.mapper = mapper;
            this.cache = cache;
        }
        private void ClearPurchaseOrderCache()
        {
            cache.Remove(PurchaseOrderCacheKey);

            for (int page = 1; page <= 50; page++)
            {
                for (int size = 10; size <= 100; size += 10)
                {
                    cache.Remove( $"{PurchaseOrderCacheKey}_{page}_{size}");
                }
            }
        }
        private async Task<bool> ValidateVendor(int vendorId)
        {
            return await db.Vendors.AnyAsync(x => x.VendorId == vendorId);
        }
        private async Task<bool> ValidatePurchaseRequisition(int id)
        {
            return await db.PurchaseRequisitions.AnyAsync(x =>x.PurchaseRequisitionId == id);
        }
        private async Task<bool> ValidateQuotation(int id)
        {
            return await db.Quotations.AnyAsync(x =>x.QuotationId == id);
        }



        public async Task<ApiResponse<PurchaseOrderDTO>> AddPurchaseOrder(PurchaseOrderDTO dto)
        {
            if (dto == null) return ApiResponseHelper.Failure<PurchaseOrderDTO>("Invalid Request", "400","Request body is missing");
            


            var vendorExists = await db.Vendors.AnyAsync(x => x.VendorId == dto.VendorId && x.IsActive == (byte)IsActive.Active);
            if (!vendorExists)return ApiResponseHelper.Failure<PurchaseOrderDTO>("Invalid Vendor", "400","Vendor does not exist or inactive");
            
            var duplicatePOCode = await db.PurchaseOrders.AnyAsync(x =>x.POCode == dto.POCode && x.IsActive == (byte)IsActive.Active);
            if (duplicatePOCode)return ApiResponseHelper.Failure<PurchaseOrderDTO>("Duplicate PO Code","400","Purchase Order Code already exists");

            var duplicatePR = await db.PurchaseOrders.AnyAsync(x =>x.PurchaseRequisitionId == dto.PurchaseRequisitionId &&x.IsActive == (byte)IsActive.Active);
            if (duplicatePR) return ApiResponseHelper.Failure<PurchaseOrderDTO>("Duplicate Purchase Requisition","400","Purchase Order already exists for this Purchase Requisition");

            var duplicateQuotation = await db.PurchaseOrders.AnyAsync(x =>x.QuotationId == dto.QuotationId && x.IsActive == (byte)IsActive.Active);
            if (duplicateQuotation)return ApiResponseHelper.Failure<PurchaseOrderDTO>("Duplicate Quotation","400","Purchase Order already exists for this Quotation");

            var prExists = await db.PurchaseRequisitions.AnyAsync(x =>x.PurchaseRequisitionId == dto.PurchaseRequisitionId && x.IsActive == (byte)IsActive.Active);
            if (!prExists)return ApiResponseHelper.Failure<PurchaseOrderDTO>("Invalid Purchase Requisition","400","Purchase Requisition not found or inactive");

            var quotation = await db.Quotations.AnyAsync(x => x.QuotationId == dto.QuotationId && x.VendorId == dto.VendorId);
            if (!quotation)return ApiResponseHelper.Failure<PurchaseOrderDTO>("Invalid Quotation","400","Quotation does not belong to selected vendor or inactive");

            if (dto.RequiredTillDate.HasValue && dto.RequiredTillDate.Value.Date < DateTime.Today)
            return ApiResponseHelper.Failure<PurchaseOrderDTO>("Invalid Date","400","Required date cannot be previous date");
            
            var data = mapper.Map<PurchaseOrder>(dto);
            data.POCode = dto.POCode;
            data.Status = "DRAFT";
            data.IsActive = 1;
            data.CreatedAt = DateTime.Now;
            data.ModifiedAt = DateTime.Now;

            await db.PurchaseOrders.AddAsync(data);
            await db.SaveChangesAsync();

            ClearPurchaseOrderCache();

            return ApiResponseHelper.SuccessRes(mapper.Map<PurchaseOrderDTO>(data),"Purchase Order Created Successfully");
        }



        public async Task<ApiResponse<PurchaseOrderDTO>> GetPurchaseOrder(int id)
        {

            if (id <= 0)return ApiResponseHelper.Failure<PurchaseOrderDTO>("Invalid Id","400","Purchase Order Id must be greater than zero");
            string cacheKey = $"{PurchaseOrderCacheKey}_{id}";

            if (cache.TryGetValue(cacheKey,out PurchaseOrderDTO cached))
                return ApiResponseHelper.SuccessRes(cached,"Purchase Order Retrieved From Cache");
            var data = await db.PurchaseOrders.Include(x => x.Vendor).Include(x => x.PurchaseOrderItems).FirstOrDefaultAsync(x =>x.POId == id);


            if (data == null)
            {
                return ApiResponseHelper.Failure<PurchaseOrderDTO>(
                    "Purchase Order Not Found",
                    "404",
                    "Record not found");
            }
            if (data.IsActive == 0)
            {
                return ApiResponseHelper.Failure<PurchaseOrderDTO>(
                    "Purchase Order Already Deleted",
                    "400",
                    "Record is already deleted");
            }
            var result =
                mapper.Map<PurchaseOrderDTO>(data);

            cache.Set(
                cacheKey,
                result,
                TimeSpan.FromMinutes(5));
            return ApiResponseHelper.SuccessRes(
                result,
                "Purchase Order Retrieved Successfully");

        }

        public async Task<ApiResponse<List<PurchaseOrderDTO>>> GetAllPurchaseOrder(int page, int pageSize)
        {
            if (page <= 0 || pageSize <= 0)return ApiResponseHelper.Failure<List<PurchaseOrderDTO>>("Invalid Pagination","400","Invalid page or page size");
            string cacheKey =$"{PurchaseOrderCacheKey}_{page}_{pageSize}";

            if (cache.TryGetValue(cacheKey,out List<PurchaseOrderDTO> cached))
            return ApiResponseHelper.SuccessRes(cached,"Purchase Orders Retrieved Successfully");

            var query =db.PurchaseOrders.Include(x => x.Vendor).Where(x => x.IsActive == 1);
            var total =await query.CountAsync();
            var data =await query.OrderByDescending(x => x.POId).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            if (!data.Any())return ApiResponseHelper.Failure<List<PurchaseOrderDTO>>("No Records Found","404","Purchase Order data not available");
            
            var result =mapper.Map<List<PurchaseOrderDTO>>(data);

            cache.Set(cacheKey,result,TimeSpan.FromMinutes(5));
            return ApiResponseHelper.SuccessRes(result,"Purchase Orders Retrieved Successfully",total,
                new
                {
                    page,
                    pageSize,
                    totalPages =
                    Math.Ceiling((double)total / pageSize)
                });

        }

        public async Task<ApiResponse<PurchaseOrderDTO>> UpdatePurchaseOrder(int id, PurchaseOrderDTO dto)
        {
            if (dto == null) return ApiResponseHelper.Failure<PurchaseOrderDTO>("Invalid Request","400","Request body is missing");
           
            var data = await db.PurchaseOrders.FirstOrDefaultAsync(x => x.POId == id);
            if (data == null)return ApiResponseHelper.Failure<PurchaseOrderDTO>("Purchase Order Not Found","404","Purchase Order does not exist");
            
            if (data.IsActive == (byte)IsActive.Inactive)return ApiResponseHelper.Failure<PurchaseOrderDTO>("Purchase Order Inactive","400","Cannot update inactive Purchase Order");
           
            if (dto.Amount <= 0)return ApiResponseHelper.Failure<PurchaseOrderDTO>("Invalid Amount","400","Amount must be greater than zero");

            if (dto.RequiredTillDate.HasValue &&dto.RequiredTillDate.Value.Date < DateTime.Today)
            return ApiResponseHelper.Failure<PurchaseOrderDTO>("Invalid Date","400","Required Till Date cannot be previous date");
            


            var poCodeExists = await db.PurchaseOrders.AnyAsync(x =>
                x.POCode == dto.POCode &&
                x.POId != id &&
                x.IsActive == (byte)IsActive.Active);


            if (poCodeExists)return ApiResponseHelper.Failure<PurchaseOrderDTO>("Duplicate PO Code","400","Another Purchase Order with same PO Code already exists");

            var prExists = await db.PurchaseRequisitions.AnyAsync(x => x.PurchaseRequisitionId == dto.PurchaseRequisitionId && x.IsActive == (byte)IsActive.Active);
            if (!prExists)return ApiResponseHelper.Failure<PurchaseOrderDTO>("Invalid Purchase Requisition","400","Purchase Requisition does not exist or inactive");
            
            var quotationExists = await db.Quotations.AnyAsync(x => x.QuotationId == dto.QuotationId && x.VendorId == dto.VendorId);
            if (!quotationExists) return ApiResponseHelper.Failure<PurchaseOrderDTO>("Invalid Quotation","400","Quotation does not belong to selected vendor or inactive");
            
            var quotationDuplicate = await db.PurchaseOrders.AnyAsync(x =>x.QuotationId == dto.QuotationId && x.POId != id &&x.IsActive == (byte)IsActive.Active);
            if (quotationDuplicate)return ApiResponseHelper.Failure<PurchaseOrderDTO>("Duplicate Quotation","400","Another Purchase Order already exists for this quotation");
            
            var vendorExists = await db.Vendors.AnyAsync(x =>x.VendorId == dto.VendorId &&x.IsActive == (byte)IsActive.Active);
            if (!vendorExists)return ApiResponseHelper.Failure<PurchaseOrderDTO>("Invalid Vendor","400","Vendor does not exist or inactive");
            
            var oldPOCode = data.POCode;
            var oldCreatedAt = data.CreatedAt;
            var oldCreatedBy = data.CreatedBy;
            var oldIsActive = data.IsActive;
            mapper.Map(dto, data);
            data.POCode = dto.POCode;
            data.CreatedAt = oldCreatedAt;
            data.CreatedBy = oldCreatedBy;
            data.IsActive = oldIsActive;
            data.ModifiedAt = DateTime.Now;
            await db.SaveChangesAsync();
            ClearPurchaseOrderCache();
            return ApiResponseHelper.SuccessRes( mapper.Map<PurchaseOrderDTO>(data), "Purchase Order Updated Successfully");
        }

        public async Task<ApiResponse<PurchaseOrderDTO>> DeletePurchaseOrder(int id)
        {
            var data = await db.PurchaseOrders
                .FirstOrDefaultAsync(x => x.POId == id);

            if (data == null)
            {
                return ApiResponseHelper.Failure<PurchaseOrderDTO>(
                    "Purchase Order Not Found",
                    "404",
                    "Record not found");
            }

            if (data.IsActive == (byte)IsActive.Inactive)
            {
                return ApiResponseHelper.Failure<PurchaseOrderDTO>(
                    "Purchase Order Already Deleted",
                    "400",
                    "Record is already deleted");
            }

            

            var grnExists = await db.GRNs
                .AnyAsync(x => x.POId == id);

            var invoiceExists =
                await db.APInvoices
                .AnyAsync(x =>
                x.PurchaseOrderId == id);


            if (invoiceExists)
            {
                return ApiResponseHelper.Failure<PurchaseOrderDTO>(
                    "Cannot Delete Purchase Order",
                    "400",
                    "Invoice already generated for this PO");
            }
            if (grnExists)
            {
                return ApiResponseHelper.Failure<PurchaseOrderDTO>(
                    "Cannot Delete Purchase Order",
                    "400",
                    "GRN already generated for this Purchase Order");
            }

            var result = mapper.Map<PurchaseOrderDTO>(data);

            data.IsActive = (byte)IsActive.Inactive;
            data.ModifiedAt = DateTime.Now;

            await db.SaveChangesAsync();

            ClearPurchaseOrderCache();

            return ApiResponseHelper.SuccessRes(
                result,
                "Purchase Order Deleted Successfully");
        }

        public async Task<ApiResponse<QuotationDetailsDTO>> ReadQuotationById(int id)
        {
            var quotation = await db.Quotations
                .Where(x => x.QuotationId == id && x.IsActive == 1 && x.IsSelected == 1 && !db.PurchaseOrders.Any(po => po.QuotationId == x.QuotationId))
                .Select(x => new QuotationDetailsDTO
                {
                    QuotationId = x.QuotationId,
                    QuotationNumber = x.QuotationNumber,
                    PurchaseRequisitionId = x.PurchaseRequisitionId,
                    VendorId = x.VendorId,
                    VendorCode = x.Vendor.VendorCode
                }).FirstOrDefaultAsync();

            if (quotation == null) return ApiResponseHelper.Failure<QuotationDetailsDTO>("Quotation not found or already used in Purchase Order.");
            
            return ApiResponseHelper.SuccessRes( quotation, "Quotation retrieved successfully.");
        }
    }
}