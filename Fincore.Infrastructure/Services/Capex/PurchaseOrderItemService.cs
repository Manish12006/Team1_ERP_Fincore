using AutoMapper;
using Fincore.Application.Constants;
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
    public class PurchaseOrderItemService : IPurchaseOrderItemService
    {
        private readonly AppDbContext db;
        private readonly IMapper mapper;
        private readonly IMemoryCache cache;

        private const string PurchaseOrderItemCacheKey = "PurchaseOrderItem";

        public PurchaseOrderItemService(
            AppDbContext db,
            IMapper mapper,
            IMemoryCache cache)
        {
            this.db = db;
            this.mapper = mapper;
            this.cache = cache;
        }

        public async Task<ApiResponse<PurchaseOrderItemDTO>> AddPurchaseOrderItem(PurchaseOrderItemDTO dto)
        {

            // PO Exists Check
            var poExists = await db.PurchaseOrders
                .AnyAsync(x => x.POId == dto.POId);


            if (!poExists)
            {
                return ApiResponseHelper.Failure<PurchaseOrderItemDTO>(
                    "Invalid Purchase Order",
                    "400",
                    "Purchase Order does not exist"
                );
            }



            // PR Item Exists Check
            var itemExists = await db.PurchaseRequisitionItems
                .AnyAsync(x => x.PRItemId == dto.PRItemId);


            if (!itemExists)
            {
                return ApiResponseHelper.Failure<PurchaseOrderItemDTO>(
                    "Invalid Item",
                    "400",
                    "Purchase Requisition Item does not exist"
                );
            }



            // Quantity Validation
            if (dto.Quantity <= 0)
            {
                return ApiResponseHelper.Failure<PurchaseOrderItemDTO>(
                    "Invalid Quantity",
                    "400",
                    "Quantity must be greater than zero"
                );
            }



            // Price Validation
            if (dto.UnitPrice <= 0)
            {
                return ApiResponseHelper.Failure<PurchaseOrderItemDTO>(
                    "Invalid Price",
                    "400",
                    "Unit Price must be greater than zero"
                );
            }



            // Tax Validation
            if (dto.TaxPercentage < 0)
            {
                return ApiResponseHelper.Failure<PurchaseOrderItemDTO>(
                    "Invalid Tax",
                    "400",
                    "Tax percentage cannot be negative"
                );
            }



            // Calculate Tax Amount
            dto.TaxAmount =
                (dto.Quantity * dto.UnitPrice)
                * dto.TaxPercentage / 100;



            // Calculate Line Total

            dto.LineTotal =
                (dto.Quantity * dto.UnitPrice)
                + dto.TaxAmount;



            var data = mapper.Map<PurchaseOrderItem>(dto);



            data.ItemStatus = "Open";



            await db.PurchaseOrderItems.AddAsync(data);



            var result = await db.SaveChangesAsync();



            if (result > 0)
            {

                cache.Remove(PurchaseOrderItemCacheKey);


                return ApiResponseHelper.SuccessRes(
                    mapper.Map<PurchaseOrderItemDTO>(data),
                    "Purchase Order Item Created Successfully"
                );
            }



            return ApiResponseHelper.Failure<PurchaseOrderItemDTO>(
                "Purchase Order Item Not Created",
                "500",
                "Error while saving data"
            );

        }

        public async Task<ApiResponse<PurchaseOrderItemDTO>> GetPurchaseOrderItem(int id)
        {
            var data = await db.PurchaseOrderItems
                               .FirstOrDefaultAsync(x => x.POItemId == id);

            if (data == null)
            {
                return ApiResponseHelper.Failure<PurchaseOrderItemDTO>(
                    "Purchase Order Item Not Found",
                    "404",
                    "Record not found"
                );
            }

            return ApiResponseHelper.SuccessRes(
                mapper.Map<PurchaseOrderItemDTO>(data),
                "Purchase Order Item Retrieved Successfully"
            );
        }

        public async Task<ApiResponse<List<PurchaseOrderItemDTO>>> GetAllPurchaseOrderItems(
            int page,
            int pageSize)
        {
            string cacheKey = $"{PurchaseOrderItemCacheKey}_{page}_{pageSize}";

            if (cache.TryGetValue(cacheKey, out List<PurchaseOrderItemDTO> purchaseOrderItems))
            {
                var totalRecords = await db.PurchaseOrderItems.CountAsync();

                return ApiResponseHelper.SuccessRes(
                    purchaseOrderItems,
                    "Purchase Order Items Retrieved Successfully",
                    totalRecords,
                    new
                    {
                        page,
                        pageSize
                    }
                );
            }

            var data = await db.PurchaseOrderItems
                               .Skip((page - 1) * pageSize)
                               .Take(pageSize)
                               .ToListAsync();

            purchaseOrderItems = mapper.Map<List<PurchaseOrderItemDTO>>(data);

            if (!purchaseOrderItems.Any())
            {
                return ApiResponseHelper.Failure<List<PurchaseOrderItemDTO>>(
                    "Purchase Order Items Not Found",
                    "POITEM_NOT_FOUND",
                    "No Purchase Order Item records found"
                );
            }

            cache.Set(
                cacheKey,
                purchaseOrderItems,
                TimeSpan.FromMinutes(5));

            var totalRecordsCount = await db.PurchaseOrderItems.CountAsync();

            return ApiResponseHelper.SuccessRes(
                purchaseOrderItems,
                "Purchase Order Items Retrieved Successfully",
                totalRecordsCount,
                new
                {
                    page,
                    pageSize
                }
            );
        }

        public async Task<ApiResponse<PurchaseOrderItemDTO>> UpdatePurchaseOrderItem(
    int id,
    PurchaseOrderItemDTO dto)
        {


            var data = await db.PurchaseOrderItems
                .FirstOrDefaultAsync(x => x.POItemId == id);



            if (data == null)
            {
                return ApiResponseHelper.Failure<PurchaseOrderItemDTO>(
                    "Purchase Order Item Not Found",
                    "404",
                    "Record not found"
                );
            }



            // Quantity Check

            if (dto.Quantity <= 0)
            {
                return ApiResponseHelper.Failure<PurchaseOrderItemDTO>(
                    "Invalid Quantity",
                    "400",
                    "Quantity must be greater than zero"
                );
            }



            // Price Check

            if (dto.UnitPrice <= 0)
            {
                return ApiResponseHelper.Failure<PurchaseOrderItemDTO>(
                    "Invalid Price",
                    "400",
                    "Unit price must be greater than zero"
                );
            }



            // Tax Check

            if (dto.TaxPercentage < 0)
            {
                return ApiResponseHelper.Failure<PurchaseOrderItemDTO>(
                    "Invalid Tax",
                    "400",
                    "Tax cannot be negative"
                );
            }



            dto.TaxAmount =
                (dto.Quantity * dto.UnitPrice)
                * dto.TaxPercentage / 100;



            dto.LineTotal =
                (dto.Quantity * dto.UnitPrice)
                + dto.TaxAmount;



            mapper.Map(dto, data);



            await db.SaveChangesAsync();



            cache.Remove(PurchaseOrderItemCacheKey);



            return ApiResponseHelper.SuccessRes(
                mapper.Map<PurchaseOrderItemDTO>(data),
                "Purchase Order Item Updated Successfully"
            );

        }

        public async Task<ApiResponse<PurchaseOrderItemDTO>> DeletePurchaseOrderItem(int id)
        {
           
            var data = await db.PurchaseOrderItems
                               .FirstOrDefaultAsync(x => x.POItemId == id);


            if (data == null)
            {
                return ApiResponseHelper.Failure<PurchaseOrderItemDTO>(
                    "Purchase Order Item Not Found",
                    "404",
                    "Record not found"
                );
            }



            
            var poExists = await db.PurchaseOrders
                                   .AnyAsync(x => x.POId == data.POId);


            if (!poExists)
            {
                return ApiResponseHelper.Failure<PurchaseOrderItemDTO>(
                    "Purchase Order Not Found",
                    "400",
                    "Related Purchase Order does not exist"
                );
            }



            
            if (data.Quantity <= 0)
            {
                return ApiResponseHelper.Failure<PurchaseOrderItemDTO>(
                    "Invalid Quantity",
                    "400",
                    "Quantity must be greater than zero"
                );
            }



            
            if (data.UnitPrice <= 0)
            {
                return ApiResponseHelper.Failure<PurchaseOrderItemDTO>(
                    "Invalid Price",
                    "400",
                    "Unit Price must be greater than zero"
                );
            }



            
            if (data.TaxPercentage < 0)
            {
                return ApiResponseHelper.Failure<PurchaseOrderItemDTO>(
                    "Invalid Tax",
                    "400",
                    "Tax percentage cannot be negative"
                );
            }



            
            var purchaseOrder = await db.PurchaseOrders
                                        .FirstOrDefaultAsync(x => x.POId == data.POId);


           



         
            var result = mapper.Map<PurchaseOrderItemDTO>(data);



            
            db.PurchaseOrderItems.Remove(data);


            await db.SaveChangesAsync();



            
            cache.Remove(PurchaseOrderItemCacheKey);



            return ApiResponseHelper.SuccessRes(
                result,
                "Purchase Order Item Deleted Successfully"
            );
        }

        public async Task<ApiResponse<List<PurchaseOrderItemDTO>>> GetItemsByPOId(int poId)
        {

            var poExists = await db.PurchaseOrders
                .AnyAsync(x => x.POId == poId);


            if (!poExists)
            {
                return ApiResponseHelper.Failure<List<PurchaseOrderItemDTO>>(
                    "Purchase Order Not Found",
                    "404",
                    "Invalid PO Id"
                );
            }



            var data = await db.PurchaseOrderItems
                .Where(x => x.POId == poId)
                .ToListAsync();



            if (!data.Any())
            {
                return ApiResponseHelper.Failure<List<PurchaseOrderItemDTO>>(
                    "Items Not Found",
                    "404",
                    "No items available for this PO"
                );
            }



            return ApiResponseHelper.SuccessRes(
                mapper.Map<List<PurchaseOrderItemDTO>>(data),
                "Purchase Order Items Retrieved Successfully"
            );

        }


        public async Task<ApiResponse<PurchaseOrderItemDTO>> UpdateItemStatus(
    int id,
    string status)
        {

            var data = await db.PurchaseOrderItems
                .FirstOrDefaultAsync(x => x.POItemId == id);



            if (data == null)
            {
                return ApiResponseHelper.Failure<PurchaseOrderItemDTO>(
                    "Item Not Found",
                    "404",
                    "Purchase Order Item not found"
                );
            }



            data.ItemStatus = status;



            await db.SaveChangesAsync();



            cache.Remove(PurchaseOrderItemCacheKey);



            return ApiResponseHelper.SuccessRes(
                mapper.Map<PurchaseOrderItemDTO>(data),
                "Item Status Updated Successfully"
            );

        }

        public async Task<ApiResponse<POTotalDTO>> GetPOTotal(int poId)
        {

            var items = await db.PurchaseOrderItems
                .Where(x => x.POId == poId)
                .ToListAsync();


            if (!items.Any())
            {
                return ApiResponseHelper.Failure<POTotalDTO>(
                    "Items Not Found",
                    "404",
                    "No items found for this PO"
                );
            }



            var result = new POTotalDTO
            {
                POId = poId,

                TotalQuantity = items.Sum(x => x.Quantity),

                TotalAmount = items.Sum(x => x.LineTotal)
            };



            return ApiResponseHelper.SuccessRes(
                result,
                "PO Total Calculated Successfully"
            );

        }
    }
}