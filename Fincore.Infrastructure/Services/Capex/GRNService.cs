using AutoMapper;
using Fincore.Application.DTO;
using Fincore.Application.DTO.Capex;
using Fincore.Application.Interfaces.ICapex;
using Fincore.Domain.Models;
using Fincore.Infrastructure.CommonHelper;
using Fincore.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace Fincore.Infrastructure.Services.Capex
{
    public class GRNService : IGRNService
    {
        private readonly AppDbContext db;
        private readonly IMapper mapper;
        private readonly IMemoryCache cache;

        private const string GRNCacheKey = "GRN";

        public GRNService(
            AppDbContext db,
            IMapper mapper,
            IMemoryCache cache)
        {
            this.db = db;
            this.mapper = mapper;
            this.cache = cache;
        }


        private void ClearGRNCache()
        {
            cache.Remove(GRNCacheKey);

            for (int page = 1; page <= 50; page++)
            {
                for (int size = 10; size <= 100; size += 10)
                {
                    cache.Remove($"{GRNCacheKey}_{page}_{size}");
                }
            }
        }



        public async Task<ApiResponse<GRNDTO>> CreateGRN(GRNDTO dto)
        {

            if (dto == null)
            {
                return ApiResponseHelper.Failure<GRNDTO>(
                    "Invalid Request",
                    "400",
                    "GRN data is required");
            }


            if (string.IsNullOrWhiteSpace(dto.GRNCode))
            {
                return ApiResponseHelper.Failure<GRNDTO>(
                    "Invalid GRN Code",
                    "400",
                    "GRN Code is required");
            }


            if (dto.POId <= 0)
            {
                return ApiResponseHelper.Failure<GRNDTO>(
                    "Invalid PO",
                    "400",
                    "Purchase Order Id required");
            }


            if (dto.VendorId <= 0)
            {
                return ApiResponseHelper.Failure<GRNDTO>(
                    "Invalid Vendor",
                    "400",
                    "Vendor Id required");
            }



            var duplicate = await db.GRNs
                .AnyAsync(x => x.GRNCode == dto.GRNCode);


            if (duplicate)
            {
                return ApiResponseHelper.Failure<GRNDTO>(
                    "GRN Code Already Exists",
                    "400",
                    "Duplicate GRN Code");
            }




            var po = await db.PurchaseOrders
                .FirstOrDefaultAsync(x =>
                    x.POId == dto.POId &&
                    x.IsActive == 1);



            if (po == null)
            {
                return ApiResponseHelper.Failure<GRNDTO>(
                    "Invalid Purchase Order",
                    "404",
                    "Purchase Order not found or deleted");
            }




            if (po.VendorId != dto.VendorId)
            {
                return ApiResponseHelper.Failure<GRNDTO>(
                    "Vendor Mismatch",
                    "400",
                    "Vendor does not belong to selected PO");
            }





            dto.GRNStatus = "OPEN";
            dto.QualityCheckStatus = "PENDING";
            dto.IsActive = 1;
            dto.CreatedAt = DateTime.Now;



            var grn = mapper.Map<GRN>(dto);



            await db.GRNs.AddAsync(grn);


            var result = await db.SaveChangesAsync();



            if (result > 0)
            {
                ClearGRNCache();


                return ApiResponseHelper.SuccessRes(
                    mapper.Map<GRNDTO>(grn),
                    "GRN Created Successfully");
            }



            return ApiResponseHelper.Failure<GRNDTO>(
                "GRN Creation Failed",
                "500",
                "Database error");
        }




        public async Task<ApiResponse<GRNDTO>> DeleteGRN(int id)
        {

            var grn = await db.GRNs
                .FirstOrDefaultAsync(x => x.GRNId == id);



            if (grn == null)
            {
                return ApiResponseHelper.Failure<GRNDTO>(
                    "GRN Not Found",
                    "404",
                    "Record not found");
            }



            if (grn.IsActive == 0)
            {
                return ApiResponseHelper.Failure<GRNDTO>(
                    "GRN Already Deleted",
                    "400",
                    "Record is already deleted");
            }





            var invoiceExists = await db.APInvoices
                .AnyAsync(x => x.GRNId == id);



            if (invoiceExists)
            {
                return ApiResponseHelper.Failure<GRNDTO>(
                    "Cannot Delete GRN",
                    "400",
                    "Invoice already generated");
            }

            grn.IsActive = 0;
            grn.ModifiedAt = DateTime.Now;

            await db.SaveChangesAsync();

            ClearGRNCache();

            return ApiResponseHelper.SuccessRes(
                mapper.Map<GRNDTO>(grn),
                "GRN Deleted Successfully");

        }
        public async Task<ApiResponse<List<GRNDTO>>> GetAllGRN(int page,int pageSize)
        {
            if (page <= 0) page = 1;
            if (pageSize <= 0)pageSize = 10;
            string cacheKey =
                $"{GRNCacheKey}_{page}_{pageSize}";

            if (cache.TryGetValue(cacheKey,
                out List<GRNDTO> cached))
            {

                var total =
                    await db.GRNs
                    .Where(x => x.IsActive == 1)
                    .CountAsync();

                return ApiResponseHelper.SuccessRes(
                    cached,
                    "GRN Retrieved Successfully",
                    total,
                    new
                    {
                        page,
                        pageSize
                    });
            }

            var query = db.GRNs
                .Where(x => x.IsActive == 1);

            var totalRecords =
                await query.CountAsync();

            var data =
                await query
                .OrderByDescending(x => x.GRNId)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            if (!data.Any())
            {
                return ApiResponseHelper.Failure<List<GRNDTO>>(
                    "GRN Not Found",
                    "404",
                    "No active GRN found");
            }

            var result =
                mapper.Map<List<GRNDTO>>(data);

            cache.Set(
                cacheKey,
                result,
                TimeSpan.FromMinutes(5));

            return ApiResponseHelper.SuccessRes(
                result,
                "GRN Retrieved Successfully",
                totalRecords,
                new
                {
                    page,
                    pageSize
                });

        }
        public async Task<ApiResponse<GRNDTO>> GetGRNById(int id)
        {

            if (id <= 0)
            {
                return ApiResponseHelper.Failure<GRNDTO>(
                    "Invalid GRN Id",
                    "400",
                    "GRN Id must be greater than zero");
            }

            var data = await db.GRNs
                .Include(x => x.PurchaseOrder)
                .Include(x => x.Vendor)
                .FirstOrDefaultAsync(x => x.GRNId == id);

            if (data == null)
            {
                return ApiResponseHelper.Failure<GRNDTO>(
                    "GRN Not Found",
                    "404",
                    "Record not found");
            }


            if (data.IsActive == 0)
            {
                return ApiResponseHelper.Failure<GRNDTO>(
                    "GRN Already Deleted",
                    "400",
                    "Record is already deleted");
            }

            return ApiResponseHelper.SuccessRes(
                mapper.Map<GRNDTO>(data),
                "GRN Retrieved Successfully");

        }

        public async Task<ApiResponse<GRNDTO>> UpdateGRN(GRNDTO dto,int id)
        {

            if (dto == null)
            {
                return ApiResponseHelper.Failure<GRNDTO>(
                    "Invalid Request",
                    "400",
                    "GRN data required");
            }
            var grn = await db.GRNs.FirstOrDefaultAsync(x => x.GRNId == id);

            if (grn == null)
            {
                return ApiResponseHelper.Failure<GRNDTO>(
                    "GRN Not Found",
                    "404",
                    "Record not found");
            }
            if (grn.IsActive == 0)
            {
                return ApiResponseHelper.Failure<GRNDTO>(
                    "GRN Already Deleted",
                    "400",
                    "Record is already deleted");
            }

            if (grn.GRNStatus == "CLOSED")
            {
                return ApiResponseHelper.Failure<GRNDTO>(
                    "Cannot Update GRN",
                    "400",
                    "Closed GRN cannot be updated");
            }

            if (dto.ReceivedBy <= 0)
            {
                return ApiResponseHelper.Failure<GRNDTO>(
                    "Invalid Received By",
                    "400",
                    "Received user required");
            }

            var userExists =
                await db.Users.AnyAsync(x =>
                x.UserId == dto.ReceivedBy);

            if (!userExists)
            {
                return ApiResponseHelper.Failure<GRNDTO>(
                    "Invalid User",
                    "404",
                    "Received user not found");
            }

            grn.ReceivedDate = dto.ReceivedDate;
            grn.ReceivedBy = dto.ReceivedBy;
            grn.Remarks = dto.Remarks;
            grn.ModifiedAt = DateTime.Now;

            await db.SaveChangesAsync();

            ClearGRNCache();

            return ApiResponseHelper.SuccessRes(
                mapper.Map<GRNDTO>(grn),
                "GRN Updated Successfully");

        }

        public async Task<ApiResponse<GRNDTO>> ApproveQualityCheck(int id)
        {

            var grn = await db.GRNs
                .FirstOrDefaultAsync(x => x.GRNId == id);

            if (grn == null)
            {
                return ApiResponseHelper.Failure<GRNDTO>(
                    "GRN Not Found",
                    "404",
                    "Record not found");
            }
            if (grn.IsActive == 0)
            {
                return ApiResponseHelper.Failure<GRNDTO>(
                    "GRN Already Deleted",
                    "400",
                    "Record is already deleted");
            }
            if (grn.QualityCheckStatus == "PASSED")
            {
                return ApiResponseHelper.Failure<GRNDTO>(
                    "Already Approved",
                    "400",
                    "Quality check already completed");
            }

            grn.QualityCheckStatus = "PASSED";
            grn.GRNStatus = "APPROVED";
            grn.ModifiedAt = DateTime.Now;

            await db.SaveChangesAsync();

            ClearGRNCache();

            return ApiResponseHelper.SuccessRes(
                mapper.Map<GRNDTO>(grn),
                "Quality Check Approved");

        }
        public async Task<ApiResponse<GRNDTO>> RejectQualityCheck(int id)
        {

            var grn = await db.GRNs.FirstOrDefaultAsync(x => x.GRNId == id);

            if (grn == null)
            {
                return ApiResponseHelper.Failure<GRNDTO>(
                    "GRN Not Found",
                    "404",
                    "Record not found");
            }

            if (grn.IsActive == 0)
            {
                return ApiResponseHelper.Failure<GRNDTO>(
                    "GRN Already Deleted",
                    "400",
                    "Record is already deleted");
            }
            grn.QualityCheckStatus = "FAILED";
            grn.GRNStatus = "REJECTED";
            grn.ModifiedAt = DateTime.Now;

            await db.SaveChangesAsync();

            ClearGRNCache();

            return ApiResponseHelper.SuccessRes(
                mapper.Map<GRNDTO>(grn),
                "Quality Check Rejected");

        }

        public async Task<ApiResponse<GRNDTO>> CloseGRN(int id)
        {

            var grn = await db.GRNs.FirstOrDefaultAsync(x => x.GRNId == id);

            if (grn == null)
            {
                return ApiResponseHelper.Failure<GRNDTO>(
                    "GRN Not Found",
                    "404",
                    "Record not found");
            }

            if (grn.IsActive == 0)
            {
                return ApiResponseHelper.Failure<GRNDTO>(
                    "GRN Already Deleted",
                    "400",
                    "Record is already deleted");
            }

            if (grn.GRNStatus != "APPROVED")
            {
                return ApiResponseHelper.Failure<GRNDTO>(
                    "Cannot Close GRN",
                    "400",
                    "Only approved GRN can be closed");
            }
            grn.GRNStatus = "CLOSED";
            grn.ModifiedAt = DateTime.Now;

            await db.SaveChangesAsync();

            ClearGRNCache();

            return ApiResponseHelper.SuccessRes(
                mapper.Map<GRNDTO>(grn),
                "GRN Closed Successfully");

        }
        public async Task<ApiResponse<List<GRNDTO>>> GetGRNByStatus(string status)
        {

            if (string.IsNullOrWhiteSpace(status))
            {
                return ApiResponseHelper.Failure<List<GRNDTO>>(
                    "Invalid Status",
                    "400",
                    "Status is required");
            }
            var data = await db.GRNs
                .Where(x =>
                    x.GRNStatus.ToLower() == status.ToLower()
                    &&
                    x.IsActive == 1)
                .ToListAsync();

            if (!data.Any())
            {
                return ApiResponseHelper.Failure<List<GRNDTO>>(
                    "GRN Not Found",
                    "404",
                    "No active GRN found with this status");
            }

            return ApiResponseHelper.SuccessRes(
                mapper.Map<List<GRNDTO>>(data),
                "GRN Retrieved Successfully");

        }

        public async Task<ApiResponse<List<GRNDTO>>> GetGRNByVendor(int vendorId)
        {

            if (vendorId <= 0)
            {
                return ApiResponseHelper.Failure<List<GRNDTO>>(
                    "Invalid Vendor Id",
                    "400",
                    "Vendor Id must be greater than zero");
            }

            var vendorExists =
                await db.Vendors.AnyAsync(x =>
                x.VendorId == vendorId &&
                x.IsActive == 1);

            if (!vendorExists)
            {
                return ApiResponseHelper.Failure<List<GRNDTO>>(
                    "Vendor Not Found",
                    "404",
                    "Vendor does not exist or deleted");
            }
            var data = await db.GRNs
                .Where(x =>
                    x.VendorId == vendorId &&
                    x.IsActive == 1)
                .OrderByDescending(x => x.GRNId)
                .ToListAsync();

            if (!data.Any())
            {
                return ApiResponseHelper.Failure<List<GRNDTO>>(
                    "GRN Not Found",
                    "404",
                    "No GRN found for this vendor");
            }

            return ApiResponseHelper.SuccessRes(
                mapper.Map<List<GRNDTO>>(data),
                "Vendor GRN Retrieved Successfully");

        }

        public async Task<ApiResponse<List<GRNDTO>>> GetGRNByPurchaseOrder(int poId)
        {

            if (poId <= 0)
            {
                return ApiResponseHelper.Failure<List<GRNDTO>>(
                    "Invalid PO Id",
                    "400",
                    "Purchase Order Id must be greater than zero");
            }

            var poExists =
                await db.PurchaseOrders.AnyAsync(x =>
                x.POId == poId &&
                x.IsActive == 1);

            if (!poExists)
            {
                return ApiResponseHelper.Failure<List<GRNDTO>>(
                    "Purchase Order Not Found",
                    "404",
                    "Purchase Order does not exist or deleted");
            }

            var data = await db.GRNs
                .Where(x =>
                    x.POId == poId &&
                    x.IsActive == 1)
                .OrderByDescending(x => x.GRNId)
                .ToListAsync();

            if (!data.Any())
            {
                return ApiResponseHelper.Failure<List<GRNDTO>>(
                    "GRN Not Found",
                    "404",
                    "No active GRN found for this PO");
            }

            return ApiResponseHelper.SuccessRes(
                mapper.Map<List<GRNDTO>>(data),
                "Purchase Order GRN Retrieved Successfully");

        }
        public async Task<ApiResponse<GRNDTO>> ReceiveGoods(GRNDTO dto)
        {

            if (dto == null)
            {
                return ApiResponseHelper.Failure<GRNDTO>(
                    "Invalid Request",
                    "400",
                    "GRN data required");
            }

            var duplicate =
                await db.GRNs.AnyAsync(x =>
                x.GRNCode == dto.GRNCode);

            if (duplicate)
            {
                return ApiResponseHelper.Failure<GRNDTO>(
                    "Duplicate GRN",
                    "400",
                    "GRN Code already exists");
            }

            var po = await db.PurchaseOrders
                .FirstOrDefaultAsync(x =>
                    x.POId == dto.POId &&
                    x.IsActive == 1);


            if (po == null)
            {
                return ApiResponseHelper.Failure<GRNDTO>(
                    "PO Not Found",
                    "404",
                    "Purchase Order not found");
            }

            if (po.VendorId != dto.VendorId)
            {
                return ApiResponseHelper.Failure<GRNDTO>(
                    "Vendor Mismatch",
                    "400",
                    "Vendor does not match with PO");
            }

            var grn = mapper.Map<GRN>(dto);


            grn.IsActive = 1;
            grn.GRNStatus = "RECEIVED";
            grn.QualityCheckStatus = "PENDING";
            grn.ReceivedDate = DateTime.Now;
            grn.CreatedAt = DateTime.Now;

            await db.GRNs.AddAsync(grn);

            await db.SaveChangesAsync();

            ClearGRNCache();

            return ApiResponseHelper.SuccessRes(
                mapper.Map<GRNDTO>(grn),
                "Goods Received Successfully");

        }
        public async Task<ApiResponse<List<GRNDTO>>> GetGRNHistory(int id)
        {

            if (id <= 0)
            {
                return ApiResponseHelper.Failure<List<GRNDTO>>(
                    "Invalid GRN Id",
                    "400",
                    "GRN Id required");
            }

            var data = await db.GRNs
                .Where(x =>
                    x.GRNId == id &&
                    x.IsActive == 1)
                .ToListAsync();

            if (!data.Any())
            {
                return ApiResponseHelper.Failure<List<GRNDTO>>(
                    "History Not Found",
                    "404",
                    "No history available");
            }

            return ApiResponseHelper.SuccessRes(
                mapper.Map<List<GRNDTO>>(data),
                "GRN History Retrieved Successfully");

        }
        public async Task<ApiResponse<object>> GetGRNDropdown()
        {

            var purchaseOrders =
                await db.PurchaseOrders
                .Where(x => x.IsActive == 1)
                .Select(x => new
                {
                    id = x.POId,
                    code = x.POCode
                })
                .ToListAsync();

            var vendors =
                await db.Vendors
                .Where(x => x.IsActive == 1)
                .Select(x => new
                {
                    id = x.VendorId,
                    code = x.VendorCode
                })
                .ToListAsync();
            var result = new
            {
                purchaseOrders,
                vendors
            };
            return ApiResponseHelper.SuccessRes<object>(
                result,
                "GRN Dropdown Retrieved Successfully");

        }
    }
}