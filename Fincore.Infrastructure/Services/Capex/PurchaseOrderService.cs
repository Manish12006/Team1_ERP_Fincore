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
            data.ApprovalStatus =ApprovalStatus.Draft.ToString();
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

            if (data.ApprovalStatus == ApprovalStatus.Approved.ToString())
            {
                return ApiResponseHelper.Failure<PurchaseOrderDTO>(
                    "Approved Purchase Order Cannot Be Updated",
                    "400",
                    "Approved Purchase Order cannot be updated");
            }

            if (data.ApprovalStatus == ApprovalStatus.Cancelled.ToString())
            {
                return ApiResponseHelper.Failure<PurchaseOrderDTO>(
                    "Cancelled Purchase Order Cannot Be Updated",
                    "400",
                    "Cancelled Purchase Order cannot be updated");
            }

            if (data.ApprovalStatus == ApprovalStatus.Closed.ToString())
            {
                return ApiResponseHelper.Failure<PurchaseOrderDTO>(
                    "Closed Purchase Order Cannot Be Updated",
                    "400",
                    "Closed Purchase Order cannot be updated");
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
            var oldApprovalStatus = data.ApprovalStatus;
            var oldIsActive = data.IsActive;
            var oldCreatedAt = data.CreatedAt;
            var oldApprovedAt = data.ApprovedAt;
            var oldApprovedBy = data.ApprovedBy;
            var oldCreatedBy = data.CreatedBy;

            mapper.Map(dto, data);

            data.POCode = oldPOCode;
            data.ApprovalStatus = oldApprovalStatus;
            data.IsActive = oldIsActive;
            data.CreatedAt = oldCreatedAt;
            data.ApprovedAt = oldApprovedAt;
            data.ApprovedBy = oldApprovedBy;
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

            if (data.ApprovalStatus == ApprovalStatus.Approved.ToString())
            {
                return ApiResponseHelper.Failure<PurchaseOrderDTO>(
                    "Cannot Delete Approved PO",
                    "400",
                    "Approved Purchase Order cannot be deleted");
            }

            if (data.ApprovalStatus == ApprovalStatus.Closed.ToString())
            {
                return ApiResponseHelper.Failure<PurchaseOrderDTO>(
                    "Cannot Delete Closed PO",
                    "400",
                    "Closed Purchase Order cannot be deleted");
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

        public async Task<ApiResponse<PurchaseOrderDTO>> ApprovePurchaseOrder(int id)
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

            if (data.ApprovalStatus == ApprovalStatus.Approved.ToString())
            {
                return ApiResponseHelper.Failure<PurchaseOrderDTO>(
                    "Already Approved",
                    "400",
                    "Purchase Order already approved");
            }

            if (data.ApprovalStatus == ApprovalStatus.Cancelled.ToString())
            {
                return ApiResponseHelper.Failure<PurchaseOrderDTO>(
                    "Cancelled PO Cannot Approve",
                    "400",
                    "Cancelled Purchase Order cannot be approved");
            }

            if (data.ApprovalStatus == ApprovalStatus.Closed.ToString())
            {
                return ApiResponseHelper.Failure<PurchaseOrderDTO>(
                    "Closed PO Cannot Approve",
                    "400",
                    "Closed Purchase Order cannot be approved");
            }

            var hasItems = await db.PurchaseOrderItems
                .AnyAsync(x => x.POId == id);

            if (!hasItems)
            {
                return ApiResponseHelper.Failure<PurchaseOrderDTO>(
                    "Items Missing",
                    "400",
                    "Purchase Order must contain minimum one item");
            }

            if (data.Amount <= 0)
            {
                return ApiResponseHelper.Failure<PurchaseOrderDTO>(
                    "Invalid Amount",
                    "400",
                    "Amount must be greater than zero");
            }

            data.ApprovalStatus = ApprovalStatus.Approved.ToString();
            data.ApprovedAt = DateTime.Now;
            data.ModifiedAt = DateTime.Now;

            await db.SaveChangesAsync();

            ClearPurchaseOrderCache();

            return ApiResponseHelper.SuccessRes(
                mapper.Map<PurchaseOrderDTO>(data),
                "Purchase Order Approved Successfully");
        }

        public async Task<ApiResponse<List<PurchaseOrderDTO>>> GetPurchaseOrderByStatus(string status, int page, int pageSize)
        {
            if (string.IsNullOrWhiteSpace(status))
            {
                return ApiResponseHelper.Failure<List<PurchaseOrderDTO>>(
                    "Invalid Status",
                    "400",
                    "Status is required");
            }

            if (!Enum.TryParse<ApprovalStatus>(status, true, out _))
            {
                return ApiResponseHelper.Failure<List<PurchaseOrderDTO>>(
                    "Invalid Status",
                    "400",
                    "Status value is not valid");
            }

            if (page <= 0 || pageSize <= 0)
            {
                return ApiResponseHelper.Failure<List<PurchaseOrderDTO>>(
                    "Invalid Pagination",
                    "400",
                    "Page and PageSize must be greater than zero");
            }

            var statusValue = Enum.Parse<ApprovalStatus>(
                    status,
                    true
                ).ToString();


            var query = db.PurchaseOrders
                .Where(x =>
                x.ApprovalStatus == statusValue &&
                x.IsActive == (byte)IsActive.Active);

            var totalRecords = await query.CountAsync();

            var data = await query
                .OrderByDescending(x => x.POId)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            if (!data.Any())
            {
                return ApiResponseHelper.Failure<List<PurchaseOrderDTO>>(
                    "Purchase Orders Not Found",
                    "404",
                    "No Purchase Orders Found");
            }

            return ApiResponseHelper.SuccessRes(
                mapper.Map<List<PurchaseOrderDTO>>(data),
                "Purchase Orders Retrieved Successfully",
                totalRecords,
                new
                {
                    page,
                    pageSize,
                    totalPages = (int)Math.Ceiling((double)totalRecords / pageSize),
                    hasNextPage = page * pageSize < totalRecords,
                    hasPreviousPage = page > 1
                });
        }

        public async Task<ApiResponse<PurchaseOrderDTO>> CancelPurchaseOrder(int id)
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

            if (data.ApprovalStatus == ApprovalStatus.Cancelled.ToString())
            {
                return ApiResponseHelper.Failure<PurchaseOrderDTO>(
                    "Purchase Order Already Cancelled",
                    "400",
                    "PO is already cancelled");
            }

            if (data.ApprovalStatus == ApprovalStatus.Closed.ToString())
            {
                return ApiResponseHelper.Failure<PurchaseOrderDTO>(
                    "Closed Purchase Order Cannot Be Cancelled",
                    "400",
                    "Closed PO cannot be cancelled");
            }

            data.ApprovalStatus = ApprovalStatus.Cancelled.ToString();
            data.ModifiedAt = DateTime.Now;

            await db.SaveChangesAsync();

            ClearPurchaseOrderCache();

            return ApiResponseHelper.SuccessRes(
                mapper.Map<PurchaseOrderDTO>(data),
                "Purchase Order Cancelled Successfully");
        }

        public async Task<ApiResponse<PurchaseOrderDTO>> ClosePurchaseOrder(int id)
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

            if (data.ApprovalStatus != ApprovalStatus.Approved.ToString())
            {
                return ApiResponseHelper.Failure<PurchaseOrderDTO>(
                    "Purchase Order Cannot Be Closed",
                    "400",
                    "Only Approved Purchase Order can be closed");
            }

            data.ApprovalStatus = ApprovalStatus.Closed.ToString();
            data.ModifiedAt = DateTime.Now;

            await db.SaveChangesAsync();

            ClearPurchaseOrderCache();

            return ApiResponseHelper.SuccessRes(
                mapper.Map<PurchaseOrderDTO>(data),
                "Purchase Order Closed Successfully");
        }

        public async Task<byte[]> GeneratePurchaseOrderPdf(int id)
        {
            var data = await db.PurchaseOrders
                .Include(x => x.Vendor)
                .Include(x => x.PurchaseOrderItems)
                .FirstOrDefaultAsync(x => x.POId == id);

            if (data == null || data.IsActive == (byte)IsActive.Inactive)
            {
                return null;
            }

            var pdf = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(30);

                    page.Header()
                        .Text("FINCORE ERP - PURCHASE ORDER")
                        .FontSize(20)
                        .Bold();

                    page.Content()
                        .Column(column =>
                        {
                            column.Item().Text($"PO Number : {data.POCode}");
                            column.Item().Text($"PO Status : {data.ApprovalStatus}");
                            column.Item().Text($"Vendor Id : {data.VendorId}");
                            column.Item().Text($"Vendor Code : {data.Vendor?.VendorCode ?? "N/A"}");
                            column.Item().Text($"Order Date : {data.OrderDate:dd-MM-yyyy}");
                            column.Item().Text($"Required Till Date : {data.RequiredTillDate:dd-MM-yyyy}");

                            column.Item()
                                .PaddingTop(20)
                                .Text("Purchase Order Items")
                                .Bold();

                            column.Item()
                                .Table(table =>
                                {
                                    table.ColumnsDefinition(columns =>
                                    {
                                        columns.RelativeColumn();
                                        columns.RelativeColumn();
                                        columns.RelativeColumn();
                                        columns.RelativeColumn();
                                        columns.RelativeColumn();
                                    });

                                    table.Header(header =>
                                    {
                                        header.Cell().Text("Item");
                                        header.Cell().Text("Description");
                                        header.Cell().Text("Qty");
                                        header.Cell().Text("Price");
                                        header.Cell().Text("Total");
                                    });

                                    foreach (var item in data.PurchaseOrderItems)
                                    {
                                        table.Cell().Text(item.ItemName);
                                        table.Cell().Text(item.ItemDescription);
                                        table.Cell().Text(item.Quantity.ToString());
                                        table.Cell().Text(item.UnitPrice.ToString());
                                        table.Cell().Text(item.LineTotal.ToString());
                                    }
                                });

                            column.Item()
                                .PaddingTop(20)
                                .Text($"Grand Total : {data.Amount}")
                                .Bold();
                        });

                    page.Footer()
                        .AlignCenter()
                        .Text("Generated By FINCORE ERP");
                });
            })
            .GeneratePdf();

            return pdf;
        }

        public async Task<ApiResponse<object>> GetPurchaseOrderDropdowns()
        {

            var vendors =
                await db.Vendors
                .Where(x => x.IsActive == 1)
                .Select(x => new DropdownDTO
                {
                    Id = x.VendorId,
                    Code = x.VendorCode
                })
                .ToListAsync();


            var requisitions =
                await db.PurchaseRequisitions
                .Where(x => x.IsActive == 1)
                .Select(x => new DropdownDTO
                {
                    Id = x.PurchaseRequisitionId,
                    Code = x.PRNumber,
                    Name = x.PRTitle
                })
                .ToListAsync();

            var quotations =
                await db.Quotations
                .Select(x => new DropdownDTO
                {
                    Id = x.QuotationId,
                    Code = x.QuotationNumber
                })
                .ToListAsync();

            var users =
                await db.Users
                .Where(x => x.IsActive == 1)
                .Select(x => new DropdownDTO
                {
                    Id = x.UserId
                })
                .ToListAsync();

            object result = new
            {
                vendors,
                requisitions,
                quotations,
                users
            };


            return ApiResponseHelper.SuccessRes<object>(
                result,
                "Purchase Order Dropdown Retrieved Successfully"
            );

        }

        public async Task<ApiResponse<List<PurchaseOrderDTO>>> GetPendingPurchaseOrders()
        {

            var data =
                await db.PurchaseOrders
                .Where(x =>
                    x.IsActive == 1 &&
                    x.ApprovalStatus ==
                    ApprovalStatus.Pending.ToString())
                .OrderByDescending(x => x.POId)
                .ToListAsync();

            return ApiResponseHelper.SuccessRes(
                mapper.Map<List<PurchaseOrderDTO>>(data),
                "Pending Purchase Orders Retrieved Successfully"
            );

        }
        public async Task<ApiResponse<List<PurchaseOrderDTO>>> FilterPurchaseOrders(PurchaseOrderFilterDTO filter)
        {

            if (filter == null)
            {
                return ApiResponseHelper
                    .Failure<List<PurchaseOrderDTO>>(
                    "Invalid Request",
                    "400",
                    "Filter data is required");
            }
            if (filter.Page <= 0 || filter.PageSize <= 0)
            {
                return ApiResponseHelper
                    .Failure<List<PurchaseOrderDTO>>(
                    "Invalid Pagination",
                    "400",
                    "Page and PageSize must be greater than zero");
            }
            if (filter.FromDate.HasValue &&
               filter.ToDate.HasValue &&
               filter.FromDate > filter.ToDate)
            {
                return ApiResponseHelper
                    .Failure<List<PurchaseOrderDTO>>(
                    "Invalid Date Range",
                    "400",
                    "FromDate cannot be greater than ToDate");
            }
            if (filter.MinAmount.HasValue &&
               filter.MaxAmount.HasValue &&
               filter.MinAmount > filter.MaxAmount)
            {
                return ApiResponseHelper
                    .Failure<List<PurchaseOrderDTO>>(
                    "Invalid Amount Range",
                    "400",
                    "Minimum amount cannot be greater than maximum amount");
            }
            var query =
                db.PurchaseOrders
                .Include(x => x.Vendor)
                .Where(x => x.IsActive == 1)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(filter.POCode))
            {
                query =
                query.Where(x =>
                x.POCode.Contains(filter.POCode));
            }
            if (filter.Status.HasValue)
            {

                query =
                query.Where(x =>
                x.ApprovalStatus ==
                filter.Status.Value.ToString());

            }

            if (filter.PurchaseRequisitionId.HasValue)
            {

                query =
                query.Where(x =>
                x.PurchaseRequisitionId ==
                filter.PurchaseRequisitionId.Value);

            }
            if (filter.QuotationId.HasValue)
            {

                query =
                query.Where(x =>
                x.QuotationId ==
                filter.QuotationId.Value);

            }

            if (filter.VendorId.HasValue)
            {

                query =
                query.Where(x =>
                x.VendorId ==
                filter.VendorId.Value);

            }
            if (filter.RequestedBy.HasValue)
            {

                query =
                query.Where(x =>
                x.RequestedBy ==
                filter.RequestedBy.Value);

            }

            if (filter.CreatedBy.HasValue)
            {

                query =
                query.Where(x =>
                x.CreatedBy ==
                filter.CreatedBy.Value);

            }

            if (filter.ApprovedBy.HasValue)
            {

                query =
                query.Where(x =>
                x.ApprovedBy ==
                filter.ApprovedBy.Value);

            }
            if (filter.FromDate.HasValue)
            {

                query =
                query.Where(x =>
                x.OrderDate >= filter.FromDate.Value);

            }
            if (filter.ToDate.HasValue)
            {
                query =
                query.Where(x =>
                x.OrderDate <= filter.ToDate.Value);

            }
            if (filter.MinAmount.HasValue)
            {

                query =
                query.Where(x =>
                x.Amount >= filter.MinAmount.Value);

            }

            if (filter.MaxAmount.HasValue)
            {

                query =
                query.Where(x =>
                x.Amount <= filter.MaxAmount.Value);

            }

            var totalRecords =
                await query.CountAsync();

            if (totalRecords == 0)
            {
                return ApiResponseHelper
                    .Failure<List<PurchaseOrderDTO>>(
                    "Purchase Orders Not Found",
                    "404",
                    "No matching records found");
            }


            var data =
                await query
                .OrderByDescending(x => x.POId)
                .Skip(
                (filter.Page - 1)
                *
                filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync();
            var result =
                mapper.Map<List<PurchaseOrderDTO>>(data);
            return ApiResponseHelper.SuccessRes(
                result,
                "Filtered Purchase Orders Retrieved Successfully",
                totalRecords,
                new
                {
                    page = filter.Page,
                    pageSize = filter.PageSize,
                    totalPages =
                    (int)Math.Ceiling(
                    (double)totalRecords /
                    filter.PageSize),
                    hasNextPage =
                    filter.Page *
                    filter.PageSize <
                    totalRecords,
                    hasPreviousPage =
                    filter.Page > 1
                });

        }
        public async Task<ApiResponse<List<DropdownDTO>>> GetVendorDropdown()
        {
            var data = await db.Vendors
                .Where(x => x.IsActive == (byte)IsActive.Active)
                .Select(x => new DropdownDTO
                {
                    Id = x.VendorId,
                    Code = x.VendorCode
                })
                .ToListAsync();


            return ApiResponseHelper.SuccessRes(
                data,
                "Vendor Dropdown Retrieved Successfully");
        }

        public async Task<ApiResponse<List<DropdownDTO>>> GetPurchaseRequisitionDropdown()
        {
            var data = await db.PurchaseRequisitions
                .Where(x => x.IsActive == (byte)IsActive.Active)
                .Select(x => new DropdownDTO
                {
                    Id = x.PurchaseRequisitionId,
                    Code = x.PRNumber,
                    Name = x.PRTitle
                })
                .ToListAsync();


            return ApiResponseHelper.SuccessRes(
                data,
                "Purchase Requisition Dropdown Retrieved Successfully");
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

        public async Task<ApiResponse<List<DropdownDTO>>> GetUserDropdown()
        {
            var data = await db.Users
                .Where(x => x.IsActive == (byte)IsActive.Active)
                .Select(x => new DropdownDTO
                {
                    Id = x.UserId
                })
                .ToListAsync();


            return ApiResponseHelper.SuccessRes(
                data,
                "User Dropdown Retrieved Successfully");
        }

        public async Task<ApiResponse<List<DropdownDTO>>> GetApprovalStatusDropdown()
        {
            var data = Enum
                .GetValues(typeof(ApprovalStatus))
                .Cast<ApprovalStatus>()
                .Select(x => new DropdownDTO
                {
                    Id = (int)x,
                    Code = x.ToString(),
                    Name = x.ToString()
                })
                .ToList();

            return ApiResponseHelper.SuccessRes(
                data,
                "Approval Status Dropdown Retrieved Successfully");
        }
    }
}