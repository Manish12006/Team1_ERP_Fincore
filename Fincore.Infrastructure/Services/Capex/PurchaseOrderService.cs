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
        private async Task<bool> ValidateUser(int userId)
        {
            return await db.Users
                .AnyAsync(x => x.UserId == userId);
        }


        private async Task<bool> ValidateVendor(int vendorId)
        {
            return await db.Vendors
                .AnyAsync(x => x.VendorId == vendorId);
        }


        private async Task<bool> ValidatePurchaseRequisition(int id)
        {
            return await db.PurchaseRequisitions
                .AnyAsync(x =>
                x.PurchaseRequisitionId == id);
        }


        private async Task<bool> ValidateQuotation(int id)
        {
            return await db.Quotations
                .AnyAsync(x =>
                x.QuotationId == id);
        }
        private void ClearPurchaseOrderCache()
        {
            cache.Remove(PurchaseOrderCacheKey);

            for (int page = 1; page <= 50; page++)
            {
                for (int size = 10; size <= 100; size += 10)
                {
                    cache.Remove(
                        $"{PurchaseOrderCacheKey}_{page}_{size}");
                }
            }
        }
        private async Task<string> GeneratePOCode()
        {
            var year = DateTime.Now.Year;

            var lastPO = await db.PurchaseOrders
                .OrderByDescending(x => x.POId)
                .FirstOrDefaultAsync();

            int nextNumber = 1;

            if (lastPO != null && !string.IsNullOrWhiteSpace(lastPO.POCode))
            {
                var lastNumber = lastPO.POCode.Split('-').Last();

                if (int.TryParse(lastNumber, out int parsedNumber))
                {
                    nextNumber = parsedNumber + 1;
                }
            }

            return $"PO-{year}-{nextNumber:D4}";
        }

        public async Task<ApiResponse<PurchaseOrderDTO>> AddPurchaseOrder(PurchaseOrderDTO dto)
        {

            if (dto == null)
            {
                return ApiResponseHelper.Failure<PurchaseOrderDTO>(
                    "Invalid Request",
                    "400",
                    "Request body is missing");
            }
            var vendor = await db.Vendors
                .FirstOrDefaultAsync(x =>
                x.VendorId == dto.VendorId);


            if (vendor == null)
            {
                return ApiResponseHelper.Failure<PurchaseOrderDTO>(
                    "Vendor Not Found",
                    "404",
                    "Vendor does not exist");
            }


            if (vendor.IsActive == 0)
            {
                return ApiResponseHelper.Failure<PurchaseOrderDTO>(
                    "Vendor Deleted",
                    "400",
                    "Selected vendor is already deleted");
            }

            if (!Enum.TryParse<ApprovalStatus>(dto.ApprovalStatus, true, out _))
            {
                dto.ApprovalStatus = ApprovalStatus.Draft.ToString();
            }

            if (dto.Amount <= 0)
            {
                return ApiResponseHelper.Failure<PurchaseOrderDTO>(
                    "Invalid Amount",
                    "400",
                    "Amount must be greater than zero");
            }




            // Duplicate PO check

            var duplicatePO =
                await db.PurchaseOrders
                .AnyAsync(x =>
                x.PurchaseRequisitionId ==
                dto.PurchaseRequisitionId
                &&
                x.IsActive == (byte)IsActive.Active);


            if (duplicatePO)
            {
                return ApiResponseHelper.Failure<PurchaseOrderDTO>(
                    "Purchase Order Already Exists",
                    "400",
                    "PO already created for this Purchase Requisition");
            }

            if (!await db.PurchaseRequisitions
                .AnyAsync(x =>
                x.PurchaseRequisitionId ==
                dto.PurchaseRequisitionId))
            {
                return ApiResponseHelper.Failure<PurchaseOrderDTO>(
                    "Invalid Purchase Requisition",
                    "400",
                    "Purchase Requisition not found");
            }
            if (!await db.Quotations
                .AnyAsync(x =>
                x.QuotationId ==
                dto.QuotationId))
            {
                return ApiResponseHelper.Failure<PurchaseOrderDTO>(
                    "Invalid Quotation",
                    "400",
                    "Quotation not found");
            }

            if (!await db.Vendors
                .AnyAsync(x =>
                x.VendorId ==
                dto.VendorId))
            {
                return ApiResponseHelper.Failure<PurchaseOrderDTO>(
                    "Invalid Vendor",
                    "400",
                    "Vendor not found");
            }

            if (dto.RequiredTillDate.HasValue &&
               dto.RequiredTillDate.Value.Date < DateTime.Today)
            {
                return ApiResponseHelper.Failure<PurchaseOrderDTO>(
                    "Invalid Date",
                    "400",
                    "Required date cannot be previous date");
            }


            dto.POCode =await GeneratePOCode();
            var data =mapper.Map<PurchaseOrder>(dto);
            data.POCode =dto.POCode;
            
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

            if (id <= 0)
            {
                return ApiResponseHelper.Failure<PurchaseOrderDTO>(
                    "Invalid Id",
                    "400",
                    "Purchase Order Id must be greater than zero");
            }
            string cacheKey = $"{PurchaseOrderCacheKey}_{id}";

            if (cache.TryGetValue(cacheKey,
            out PurchaseOrderDTO cached))
            {
                return ApiResponseHelper.SuccessRes(
                    cached,
                    "Purchase Order Retrieved From Cache");
            }
            var data = await db.PurchaseOrders

                    .Include(x => x.Vendor)

                    .Include(x => x.PurchaseOrderItems)

                    .FirstOrDefaultAsync(x =>
                    x.POId == id);


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
            if (page <= 0 || pageSize <= 0)
            {
                return ApiResponseHelper
                .Failure<List<PurchaseOrderDTO>>(
                "Invalid Pagination",
                "400",
                "Invalid page or page size");
            }
            string cacheKey =
                $"{PurchaseOrderCacheKey}_{page}_{pageSize}";

            if (cache.TryGetValue(
                cacheKey,
                out List<PurchaseOrderDTO> cached))
            {

                return ApiResponseHelper.SuccessRes(
                    cached,
                    "Purchase Orders Retrieved Successfully");
            }

            var query =
                db.PurchaseOrders
                .Include(x => x.Vendor)
                .Where(x => x.IsActive == 1);
            var total =
                await query.CountAsync();
            var data =
                await query
                .OrderByDescending(x => x.POId)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            if (!data.Any())
            {
                return ApiResponseHelper.Failure<List<PurchaseOrderDTO>>(
                    "No Records Found",
                    "404",
                    "Purchase Order data not available");
            }
            var result =
                mapper.Map<List<PurchaseOrderDTO>>(data);

            cache.Set(
                cacheKey,
                result,
                TimeSpan.FromMinutes(5));
            return ApiResponseHelper.SuccessRes(
                result,
                "Purchase Orders Retrieved Successfully",
                total,
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
            if (dto == null)
            {
                return ApiResponseHelper.Failure<PurchaseOrderDTO>(
                    "Invalid Request",
                    "400",
                    "Request body is missing");
            }

            if (dto.PurchaseRequisitionId <= 0)
            {
                return ApiResponseHelper.Failure<PurchaseOrderDTO>(
                    "Invalid Purchase Requisition",
                    "400",
                    "PurchaseRequisitionId must be greater than zero");
            }

            if (dto.QuotationId <= 0)
            {
                return ApiResponseHelper.Failure<PurchaseOrderDTO>(
                    "Invalid Quotation",
                    "400",
                    "QuotationId must be greater than zero");
            }

            if (dto.VendorId <= 0)
            {
                return ApiResponseHelper.Failure<PurchaseOrderDTO>(
                    "Invalid Vendor",
                    "400",
                    "VendorId must be greater than zero");
            }

            if (dto.RequestedBy <= 0)
            {
                return ApiResponseHelper.Failure<PurchaseOrderDTO>(
                    "Invalid Requested By",
                    "400",
                    "RequestedBy must be greater than zero");
            }

            if (dto.CreatedBy <= 0)
            {
                return ApiResponseHelper.Failure<PurchaseOrderDTO>(
                    "Invalid Created By",
                    "400",
                    "CreatedBy must be greater than zero");
            }

            if (dto.ModifiedBy <= 0)
            {
                return ApiResponseHelper.Failure<PurchaseOrderDTO>(
                    "Invalid Modified By",
                    "400",
                    "ModifiedBy must be greater than zero");
            }
            if (dto.Amount <= 0)
            {
                return ApiResponseHelper.Failure<PurchaseOrderDTO>(
                "Invalid Amount",
                "400",
                "Amount must be greater than zero");
            }
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

           
            if (!await ValidatePurchaseRequisition(dto.PurchaseRequisitionId))
            {
                return ApiResponseHelper.Failure<PurchaseOrderDTO>(
                    "Invalid Purchase Requisition",
                    "400",
                    "Purchase Requisition does not exist");
            }


            if (!await ValidateQuotation(dto.QuotationId))
            {
                return ApiResponseHelper.Failure<PurchaseOrderDTO>(
                    "Invalid Quotation",
                    "400",
                    "Quotation does not exist");
            }


            if (!await ValidateVendor(dto.VendorId))
            {
                return ApiResponseHelper.Failure<PurchaseOrderDTO>(
                    "Invalid Vendor",
                    "400",
                    "Vendor does not exist");
            }


            if (!await ValidateUser(dto.RequestedBy))
            {
                return ApiResponseHelper.Failure<PurchaseOrderDTO>(
                    "Invalid Requested By",
                    "400",
                    "Requested User does not exist");
            }
            var quotationExists = await db.Quotations
                .AnyAsync(x => x.QuotationId == dto.QuotationId);

            if (!quotationExists)
            {
                return ApiResponseHelper.Failure<PurchaseOrderDTO>(
                    "Invalid Quotation",
                    "400",
                    "Quotation does not exist");
            }

            var vendorExists = await db.Vendors
                .AnyAsync(x => x.VendorId == dto.VendorId);

            if (!vendorExists)
            {
                return ApiResponseHelper.Failure<PurchaseOrderDTO>(
                    "Invalid Vendor",
                    "400",
                    "Vendor does not exist");
            }

            var requestedByExists = await db.Users
                .AnyAsync(x => x.UserId == dto.RequestedBy);

            if (!requestedByExists)
            {
                return ApiResponseHelper.Failure<PurchaseOrderDTO>(
                    "Invalid Requested By",
                    "400",
                    "RequestedBy user does not exist");
            }

            var createdByExists = await db.Users
                .AnyAsync(x => x.UserId == dto.CreatedBy);

            if (!createdByExists)
            {
                return ApiResponseHelper.Failure<PurchaseOrderDTO>(
                    "Invalid Created By",
                    "400",
                    "CreatedBy user does not exist");
            }

            var modifiedByExists = await db.Users
                .AnyAsync(x => x.UserId == dto.ModifiedBy);

            if (!modifiedByExists)
            {
                return ApiResponseHelper.Failure<PurchaseOrderDTO>(
                    "Invalid Modified By",
                    "400",
                    "ModifiedBy user does not exist");
            }

            if (dto.RequiredTillDate.HasValue && dto.RequiredTillDate.Value.Date < DateTime.Today)
            {
                return ApiResponseHelper.Failure<PurchaseOrderDTO>(
                    "Invalid Required Date",
                    "400",
                    "Required Till Date cannot be before today");
            }

            if (dto.Amount <= 0)
            {
                return ApiResponseHelper.Failure<PurchaseOrderDTO>(
                    "Invalid Amount",
                    "400",
                    "Purchase Order amount must be greater than zero");
            }

            var oldPOCode = data.POCode;
            
            var oldIsActive = data.IsActive;
            var oldCreatedAt = data.CreatedAt;
            
            var oldCreatedBy = data.CreatedBy;

            mapper.Map(dto, data);

            data.POCode = oldPOCode;
           
            data.IsActive = oldIsActive;
            data.CreatedAt = oldCreatedAt;
            
            data.CreatedBy = oldCreatedBy;
            data.ModifiedAt = DateTime.Now;

            await db.SaveChangesAsync();

            ClearPurchaseOrderCache();

            return ApiResponseHelper.SuccessRes(
                mapper.Map<PurchaseOrderDTO>(data),
                "Purchase Order Updated Successfully");
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

        

        
        public async Task<ApiResponse<List<DropdownDTO>>> GetQuotationDropdown()
        {
            var data = await db.Quotations
                .Select(x => new DropdownDTO
                {
                    Id = x.QuotationId,
                    Code = x.QuotationNumber
                })
                .ToListAsync();


            return ApiResponseHelper.SuccessRes(
                data,
                "Quotation Dropdown Retrieved Successfully");
        }
    }
}