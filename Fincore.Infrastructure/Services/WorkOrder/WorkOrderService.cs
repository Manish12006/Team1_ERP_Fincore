using AutoMapper;
using Fincore.Application.DTO;
using Fincore.Application.DTOs.WorkOrder;
using Fincore.Application.Interfaces.WorkOrder;
using Fincore.Domain.Enums;
using Fincore.Domain.Models;
using Fincore.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System.Net.NetworkInformation;

namespace Fincore.Infrastructure.Services.WorkOrder
{
    public class WorkOrderService : IWorkOrderService
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;
        private readonly IMemoryCache _cache;

        public WorkOrderService(
            AppDbContext context,
            IMapper mapper,
            IMemoryCache cache)
        {
            _context = context;
            _mapper = mapper;
            _cache = cache;
        }

        public async Task<ApiResponse<string>> AddWorkOrder(CreateWorkOrderDTO dto)
        {
            ApiResponse<string> response = new ApiResponse<string>();
            // Vendor Validation
            var vendor = await _context.Vendors
                .FirstOrDefaultAsync(x => x.VendorId == dto.VendorId);

            if (vendor == null)
            {
                response.success = false;
                response.message = "Vendor Not Found";
                return response;
            }

            // Opex Request Validation
            var opex = await _context.OpexRequests
                .FirstOrDefaultAsync(x => x.OpexRequestId == dto.OpexRequestId);

            if (opex == null)
            {
                response.success = false;
                response.message = "Opex Request Not Found";
                return response;
            }

            // Title Validation
            if (string.IsNullOrWhiteSpace(dto.Title))
            {
                response.success = false;
                response.message = "Title is required";
                return response;
            }

            if (dto.Title.Length > 30)
            {
                response.success = false;
                response.message = "Title should not exceed 30 characters";
                return response;
            }

            // Amount Validation
            if (dto.NetAmount <= 0)
            {
                response.success = false;
                response.message = "Net Amount must be greater than zero";
                return response;
            }
            var entity = _mapper.Map<Fincore.Domain.Models.WorkOrder>(dto);
            entity.Status = OpexApprovalStatus.Pending.ToString();


            entity.CreatedDate = DateTime.Now;

            await _context.WorkOrders.AddAsync(entity);
            await _context.SaveChangesAsync();

            _cache.Remove("WorkOrderList");

            response.success = true;
            response.message = "Work Order Added Successfully";
            response.data = "Success";

            return response;
        }
        public async Task<ApiResponse<List<WorkOrderResponseDTO>>> GetWorkOrders(
            string? title,
            int? vendorId,
            int? opexRequestId,
            string? status,
            int page,
            int pageSize)
        {
            ApiResponse<List<WorkOrderResponseDTO>> response =
                new ApiResponse<List<WorkOrderResponseDTO>>();
            string cacheKey =
            $"WorkOrderList_{title}_{vendorId}_{opexRequestId}_{status}_{page}_{pageSize}";

            if (!_cache.TryGetValue(cacheKey, out List<WorkOrderResponseDTO> data))
            {
                var query = _context.WorkOrders
       .Where(x => x.Status != "Deleted")
       .AsQueryable();

                if (!string.IsNullOrWhiteSpace(title))
                {
                    query = query.Where(x => x.Title.Contains(title));
                }

                if (vendorId.HasValue)
                {
                    query = query.Where(x => x.VendorId == vendorId);
                }

                if (opexRequestId.HasValue)
                {
                    query = query.Where(x => x.OpexRequestId == opexRequestId);
                }

                if (!string.IsNullOrWhiteSpace(status))
                {
                    query = query.Where(x => x.Status == status);
                }

                var list = await query
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                data = _mapper.Map<List<WorkOrderResponseDTO>>(list);

                _cache.Set(cacheKey, data, TimeSpan.FromMinutes(5));
            }

            response.success = true;
            response.message = "Work Orders Fetched Successfully";
            response.data = data;
            response.totalNumberRecord = data.Count;

            return response;
        }
        public async Task<ApiResponse<WorkOrderResponseDTO>> GetWorkOrderById(int id)
        {
            ApiResponse<WorkOrderResponseDTO> response =
                new ApiResponse<WorkOrderResponseDTO>();

            string cacheKey = $"WorkOrder_{id}";

            if (!_cache.TryGetValue(cacheKey, out WorkOrderResponseDTO dto))
            {
                var entity = await _context.WorkOrders
                 .FirstOrDefaultAsync(x =>   x.WorkOrderId == id &&
    x.Status != "Deleted");

                if (entity == null)
                {
                    response.success = false;
                    response.message = "Work Order Not Found";
                    return response;
                }

                dto = _mapper.Map<WorkOrderResponseDTO>(entity);

                _cache.Set(cacheKey, dto, TimeSpan.FromMinutes(5));
            }

            response.success = true;
            response.message = "Work Order Found Successfully";
            response.data = dto;

            return response;
        }
        public async Task<ApiResponse<string>> UpdateWorkOrder(int id, UpdateWorkOrderDTO dto)
        {
            ApiResponse<string> response = new ApiResponse<string>();

            var entity = await _context.WorkOrders
                .FirstOrDefaultAsync(x => x.WorkOrderId == id);

            if (entity == null)
            {
                response.success = false;
                response.message = "Work Order Not Found";
                return response;
            }

            // Title Validation
            if (string.IsNullOrWhiteSpace(dto.Title))
            {
                response.success = false;
                response.message = "Title is required";
                return response;
            }

            if (dto.Title.Length > 30)
            {
                response.success = false;
                response.message = "Title should not exceed 30 characters";
                return response;
            }

            // Net Amount Validation
            if (dto.NetAmount <= 0)
            {
                response.success = false;
                response.message = "Net Amount must be greater than zero";
                return response;
            }

            // Vendor Validation
            var vendor = await _context.Vendors
                .FirstOrDefaultAsync(x => x.VendorId == dto.VendorId);

            if (vendor == null)
            {
                response.success = false;
                response.message = "Vendor Not Found";
                return response;
            }

            // Opex Request Validation
            var opex = await _context.OpexRequests
                .FirstOrDefaultAsync(x => x.OpexRequestId == dto.OpexRequestId);

            if (opex == null)
            {
                response.success = false;
                response.message = "Opex Request Not Found";
                return response;
            }

            // Don't allow updating deleted record
            if (entity.Status == "Deleted")
            {
                response.success = false;
                response.message = "Work Order Already Deleted";
                return response;
            }

            _mapper.Map(dto, entity);

            await _context.SaveChangesAsync();

            _cache.Remove("WorkOrderList");
            _cache.Remove($"WorkOrder_{id}");

            response.success = true;
            response.message = "Work Order Updated Successfully";
            response.data = "Success";

            return response;

       
        }

        public async Task<ApiResponse<string>> DeleteWorkOrder(int id)
        {
            ApiResponse<string> response = new ApiResponse<string>();

            var entity = await _context.WorkOrders
                .FirstOrDefaultAsync(x => x.WorkOrderId == id);

            if (entity == null)
            {
                response.success = false;
                response.message = "Work Order Not Found";
                return response;
            }

            entity.Status = "Deleted";

            await _context.SaveChangesAsync();

            _cache.Remove("WorkOrderList");
            _cache.Remove($"WorkOrder_{id}");

            response.success = true;
            response.message = "Work Order Deleted Successfully";
            response.data = "Success";

            return response;
        }
        public async Task<ApiResponse<string>> ApproveWorkOrder(int id)
        {
            ApiResponse<string> response = new ApiResponse<string>();

            var entity = await _context.WorkOrders
                .FirstOrDefaultAsync(x => x.WorkOrderId == id);

            if (entity == null)
            {
                response.success = false;
                response.message = "Work Order Not Found";
                return response;
            }
            if (entity.Status == "Deleted")
            {
                response.success = false;
                response.message = "Work Order Already Deleted";
                return response;
            }

            entity.Status = "Approved";

            await _context.SaveChangesAsync();

            _cache.Remove("WorkOrderList");
            _cache.Remove($"WorkOrder_{id}");

            response.success = true;
            response.message = "Work Order Approved Successfully";
            response.data = "Success";

            return response;
        }

        public async Task<ApiResponse<string>> RejectWorkOrder(int id)
        {
            ApiResponse<string> response = new ApiResponse<string>();

            var entity = await _context.WorkOrders
                .FirstOrDefaultAsync(x => x.WorkOrderId == id);

            if (entity == null)
            {
                response.success = false;
                response.message = "Work Order Not Found";
                return response;
            }

            if (entity.Status == "Deleted")
            {
                response.success = false;
                response.message = "Work Order Already Deleted";
                return response;
            }

            entity.Status = "Rejected";

            await _context.SaveChangesAsync();

            _cache.Remove("WorkOrderList");
            _cache.Remove($"WorkOrder_{id}");

            response.success = true;
            response.message = "Work Order Rejected Successfully";
            response.data = "Success";

            return response;
        }
        public async Task<ApiResponse<WorkOrderSummaryDTO>> GetWorkOrderSummary()
        {
            ApiResponse<WorkOrderSummaryDTO> response =
                new ApiResponse<WorkOrderSummaryDTO>();

            WorkOrderSummaryDTO summary = new WorkOrderSummaryDTO();

            summary.TotalWorkOrders =
            await _context.WorkOrders
                .CountAsync(x => x.Status != "Deleted");

            summary.PendingWorkOrders =
                await _context.WorkOrders
                    .CountAsync(x => x.Status == "Pending");

            summary.CompletedWorkOrders =
                await _context.WorkOrders
                    .CountAsync(x => x.Status == "Completed");
            summary.TotalNetAmount =
                await _context.WorkOrders
                    .Where(x => x.Status != "Deleted")
                    .SumAsync(x => x.NetAmount);

            response.success = true;
            response.message = "Work Order Summary Fetched Successfully";
            response.data = summary;

            return response;
        }
    }
}