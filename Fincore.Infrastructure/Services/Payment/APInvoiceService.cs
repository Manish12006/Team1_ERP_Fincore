using AutoMapper;
using AutoMapper.QueryableExtensions;
using Fincore.Application.DTO;
using Fincore.Application.DTO.Payment.APInvoice.Requests;
using Fincore.Application.DTO.Payment.APInvoice.Responses;
using Fincore.Application.Interfaces.Payment;
using Fincore.Domain.Models;
using Fincore.Infrastructure.CommonHelper;
using Fincore.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace Fincore.Infrastructure.Services.Payment
{
    public class APInvoiceService : IAPInvoiceService
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;
        private readonly IMemoryCache _cache;

        private const string CacheKey = "AP_INVOICE_LIST";

        public APInvoiceService(
            AppDbContext context,
            IMapper mapper,
            IMemoryCache cache)
        {
            _context = context;
            _mapper = mapper;
            _cache = cache;
        }

        #region Create Invoice

        public async Task<ApiResponse<APInvoiceResponseDto>> CreateAsync(CreateAPInvoiceRequestDto request)
        {
            try
            {
                var vendor = await _context.Vendors
                    .FirstOrDefaultAsync(x => x.VendorId == request.VendorId);

                if (vendor == null)
                {
                    return ApiResponseHelper.Failure<APInvoiceResponseDto>(
                        "Vendor not found",
                        "NOT_FOUND",
                        "Invalid Vendor Id");
                }

                var po = await _context.PurchaseOrders
                    .FirstOrDefaultAsync(x => x.POId == request.PurchaseOrderId);

                if (po == null)
                {
                    return ApiResponseHelper.Failure<APInvoiceResponseDto>(
                        "Purchase Order not found",
                        "NOT_FOUND",
                        "Invalid Purchase Order");
                }

                var grn = await _context.GRNs
                    .FirstOrDefaultAsync(x => x.GRNId == request.GRNId);

                if (grn == null)
                {
                    return ApiResponseHelper.Failure<APInvoiceResponseDto>(
                        "GRN not found",
                        "NOT_FOUND",
                        "Invalid GRN");
                }

                if (request.WorkOrderId.HasValue)
                {
                    var workOrder = await _context.WorkOrders
                        .FirstOrDefaultAsync(x => x.WorkOrderId == request.WorkOrderId);

                    if (workOrder == null)
                    {
                        return ApiResponseHelper.Failure<APInvoiceResponseDto>(
                            "Work Order not found",
                            "NOT_FOUND",
                            "Invalid Work Order");
                    }
                }

                var totalInvoice = await _context.APInvoices.CountAsync();

                string invoiceNumber = $"APINV{(totalInvoice + 1):D6}";

                var invoice = _mapper.Map<APInvoice>(request);

                invoice.InvoiceNumber = invoiceNumber;
                invoice.ApprovalStatus = "Pending";
                invoice.PaymentStatus = "Unpaid";
                invoice.CreatedAt = DateTime.UtcNow;
                invoice.ModifiedAt = DateTime.UtcNow;

                _context.APInvoices.Add(invoice);

                await _context.SaveChangesAsync();

                _cache.Remove(CacheKey);

                var response = await _context.APInvoices
                    .Include(x => x.Vendor)
                    .Include(x => x.PurchaseOrder)
                    .Include(x => x.GRN)
                    .Include(x => x.WorkOrder)
                    .Where(x => x.APInvoiceId == invoice.APInvoiceId)
                    .ProjectTo<APInvoiceResponseDto>(_mapper.ConfigurationProvider)
                    .FirstAsync();

                return ApiResponseHelper.SuccessRes(
                    response,
                    "AP Invoice Created Successfully");
            }
            catch (Exception ex)
            {
                return ApiResponseHelper.Failure<APInvoiceResponseDto>(
                    "Unable to create invoice",
                    "CREATE_ERROR",
                    ex.Message);
            }
        }
        #endregion


        #region Get All Invoices

        public async Task<ApiResponse<List<APInvoiceResponseDto>>> GetAllAsync(APInvoiceFilterDto filter)
        {
            try
            {
                if (filter.Page <= 0)
                    filter.Page = 1;

                if (filter.PageSize <= 0)
                    filter.PageSize = 10;

                if (filter.PageSize > 100)
                    filter.PageSize = 100;

                // Cache Key (Includes Filters + Sorting + Search + Pagination)
                string cacheKey =
                    $"APInvoice_" +
                    $"P{filter.Page}_" +
                    $"PS{filter.PageSize}_" +
                    $"V{filter.VendorId}_" +
                    $"A{filter.ApprovalStatus}_" +
                    $"Pay{filter.PaymentStatus}_" +
                    $"S{filter.Search}_" +
                    $"SB{filter.SortBy}_" +
                    $"SO{filter.SortOrder}";

                // Check Cache
                if (_cache.TryGetValue(cacheKey, out List<APInvoiceResponseDto>? cachedData))
                {
                    int cachedTotalRecords = cachedData!.Count;

                    return ApiResponseHelper.SuccessRes(
                        cachedData,
                        "AP Invoice List fetched successfully.",
                        cachedTotalRecords);
                }

                var query = _context.APInvoices
                    .AsNoTracking()
                    .Include(x => x.Vendor)
                        .ThenInclude(v => v.Company)
                    .Include(x => x.PurchaseOrder)
                    .Include(x => x.GRN)
                    .Include(x => x.WorkOrder)
                    .AsQueryable();

                // Vendor Filter
                if (filter.VendorId.HasValue)
                {
                    query = query.Where(x => x.VendorId == filter.VendorId.Value);
                }

                // Approval Status
                if (!string.IsNullOrWhiteSpace(filter.ApprovalStatus))
                {
                    query = query.Where(x => x.ApprovalStatus == filter.ApprovalStatus);
                }

                // Payment Status
                if (!string.IsNullOrWhiteSpace(filter.PaymentStatus))
                {
                    query = query.Where(x => x.PaymentStatus == filter.PaymentStatus);
                }

                // Search
                if (!string.IsNullOrWhiteSpace(filter.Search))
                {
                    string search = filter.Search.Trim().ToLower();

                    query = query.Where(x =>
                        x.InvoiceNumber.ToLower().Contains(search) ||
                        x.Vendor.Company.CompanyName.ToLower().Contains(search));
                }

                // Sorting
                switch (filter.SortBy?.ToLower())
                {
                    case "amount":
                        query = filter.SortOrder?.ToLower() == "asc"
                            ? query.OrderBy(x => x.Amount)
                            : query.OrderByDescending(x => x.Amount);
                        break;

                    case "invoicedate":
                        query = filter.SortOrder?.ToLower() == "asc"
                            ? query.OrderBy(x => x.InvoiceDate)
                            : query.OrderByDescending(x => x.InvoiceDate);
                        break;

                    case "duedate":
                        query = filter.SortOrder?.ToLower() == "asc"
                            ? query.OrderBy(x => x.DueDate)
                            : query.OrderByDescending(x => x.DueDate);
                        break;

                    default:
                        query = filter.SortOrder?.ToLower() == "asc"
                            ? query.OrderBy(x => x.CreatedAt)
                            : query.OrderByDescending(x => x.CreatedAt);
                        break;
                }

                int totalRecords = await query.CountAsync();

                var invoices = await query
                    .Skip((filter.Page - 1) * filter.PageSize)
                    .Take(filter.PageSize)
                    .ToListAsync();

                var result = _mapper.Map<List<APInvoiceResponseDto>>(invoices);

                // Save Cache
                _cache.Set(
                    cacheKey,
                    result,
                    new MemoryCacheEntryOptions
                    {
                        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
                    });

                return ApiResponseHelper.SuccessRes(
                    result,
                    "AP Invoice List fetched successfully.",
                    totalRecords);
            }
            catch (Exception ex)
            {
                return ApiResponseHelper.Failure<List<APInvoiceResponseDto>>(
                    "Unable to fetch invoices",
                    "GET_ERROR",
                    ex.Message);
            }
        }

        #endregion


        #region Approve Invoice

        public async Task<ApiResponse<APInvoiceResponseDto>> ApproveAsync(int id)
        {
            try
            {
                var invoice = await _context.APInvoices
                    .Include(x => x.Vendor)
                    .Include(x => x.PurchaseOrder)
                    .Include(x => x.GRN)
                    .Include(x => x.WorkOrder)
                    .FirstOrDefaultAsync(x => x.APInvoiceId == id);

                if (invoice == null)
                {
                    return ApiResponseHelper.Failure<APInvoiceResponseDto>(
                        "AP Invoice not found",
                        "NOT_FOUND",
                        "Invalid Invoice Id");
                }

                if (invoice.ApprovalStatus == "Approved")
                {
                    return ApiResponseHelper.Failure<APInvoiceResponseDto>(
                        "Invoice is already approved",
                        "ALREADY_APPROVED",
                        "Duplicate approval is not allowed");
                }

                // TODO:
                // Replace this with Logged-in User Id after Authentication is implemented.
                invoice.ApprovedBy = 1;

                invoice.ApprovalStatus = "Approved";
                invoice.ModifiedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                _cache.Remove(CacheKey);

                var response = _mapper.Map<APInvoiceResponseDto>(invoice);

                return ApiResponseHelper.SuccessRes(
                    response,
                    "Invoice Approved Successfully");
            }
            catch (Exception ex)
            {
                return ApiResponseHelper.Failure<APInvoiceResponseDto>(
                    "Unable to approve invoice",
                    "APPROVE_ERROR",
                    ex.Message);
            }
        }

        #endregion
        #region Record Payment

        public async Task<ApiResponse<APInvoiceResponseDto>> RecordPaymentAsync(CreatePaymentRequestDto request)
        {
            try
            {
                var invoice = await _context.APInvoices
                    .Include(x => x.Payments)
                    .Include(x => x.Vendor)
                    .Include(x => x.PurchaseOrder)
                    .Include(x => x.GRN)
                    .Include(x => x.WorkOrder)
                    .FirstOrDefaultAsync(x => x.APInvoiceId == request.APInvoiceId);

                if (invoice == null)
                {
                    return ApiResponseHelper.Failure<APInvoiceResponseDto>(
                        "Invoice not found",
                        "NOT_FOUND",
                        "Invalid Invoice Id");
                }

                if (invoice.ApprovalStatus != "Approved")
                {
                    return ApiResponseHelper.Failure<APInvoiceResponseDto>(
                        "Invoice must be approved before payment",
                        "INVALID_OPERATION",
                        "Approve invoice first");
                }

                if (invoice.PaymentStatus == "Paid")
                {
                    return ApiResponseHelper.Failure<APInvoiceResponseDto>(
                        "Invoice is already fully paid",
                        "ALREADY_PAID",
                        "No further payment allowed");
                }

                var vendor = await _context.Vendors
                    .FirstOrDefaultAsync(x => x.VendorId == request.VendorId);

                if (vendor == null)
                {
                    return ApiResponseHelper.Failure<APInvoiceResponseDto>(
                        "Vendor not found",
                        "NOT_FOUND",
                        "Invalid Vendor Id");
                }

                var totalPaymentCount = await _context.Payments.CountAsync();

                string paymentNumber = $"PAY{(totalPaymentCount + 1):D6}";

                var payment = new Fincore.Domain.Models.Payment
                {
                    PaymentNumber = paymentNumber,
                    PaymentType = "Accounts Payable",
                    APInvoiceId = request.APInvoiceId,
                    VendorId = request.VendorId,
                    Amount = request.Amount,
                    PaymentDate = request.PaymentDate,
                    PaymentMethod = request.PaymentMethod,
                    ApprovalStatus = "Approved",
                    ApprovedBy = 1, // TODO: Replace with Logged-in User Id
                    ReconciledFlag = false,
                    CreatedAt = DateTime.UtcNow,
                    ModifiedAt = DateTime.UtcNow
                };

                _context.Payments.Add(payment);

                await _context.SaveChangesAsync();

                var totalPaid = await _context.Payments
                    .Where(x => x.APInvoiceId == request.APInvoiceId)
                    .SumAsync(x => x.Amount);

                if (totalPaid <= 0)
                {
                    invoice.PaymentStatus = "Unpaid";
                }
                else if (totalPaid < invoice.Amount)
                {
                    invoice.PaymentStatus = "PartiallyPaid";
                }
                else
                {
                    invoice.PaymentStatus = "Paid";
                }

                invoice.ModifiedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                _cache.Remove(CacheKey);

                var response = await _context.APInvoices
                    .Include(x => x.Vendor)
                    .Include(x => x.PurchaseOrder)
                    .Include(x => x.GRN)
                    .Include(x => x.WorkOrder)
                    .Where(x => x.APInvoiceId == invoice.APInvoiceId)
                    .ProjectTo<APInvoiceResponseDto>(_mapper.ConfigurationProvider)
                    .FirstAsync();

                return ApiResponseHelper.SuccessRes(
                    response,
                    "Payment Recorded Successfully");
            }
            catch (Exception ex)
            {
                return ApiResponseHelper.Failure<APInvoiceResponseDto>(
                    "Unable to record payment",
                    "PAYMENT_ERROR",
                    ex.Message);
            }
        }

        #endregion
        #region Outstanding Report

        public async Task<ApiResponse<List<APOutstandingDto>>> GetOutstandingAsync(APOutstandingFilterDto filter)
        {
            try
            {
                if (filter.Page <= 0)
                    filter.Page = 1;

                if (filter.PageSize <= 0)
                    filter.PageSize = 10;

                if (filter.PageSize > 100)
                    filter.PageSize = 100;

                string cacheKey = $"APOutstanding_Page_{filter.Page}_Size_{filter.PageSize}";

                // Check Cache
                if (_cache.TryGetValue(cacheKey, out List<APOutstandingDto>? cachedData))
                {
                    int cachedTotalRecords = await _context.APInvoices
                        .Where(x => x.PaymentStatus != "Paid")
                        .CountAsync();

                    return ApiResponseHelper.SuccessRes(
                        cachedData!,
                        "Outstanding Report fetched successfully.",
                        cachedTotalRecords);
                }

                var query = _context.APInvoices
                    .AsNoTracking()
                    .Include(x => x.Vendor)
                        .ThenInclude(v => v.Company)
                    .Include(x => x.Payments)
                    .Where(x => x.PaymentStatus != "Paid")
                    .AsQueryable();

                // Vendor Filter
                if (filter.VendorId.HasValue)
                {
                    query = query.Where(x => x.VendorId == filter.VendorId.Value);
                }

                // Search
                if (!string.IsNullOrWhiteSpace(filter.Search))
                {
                    string search = filter.Search.Trim().ToLower();

                    query = query.Where(x =>
                        x.InvoiceNumber.ToLower().Contains(search) ||
                        x.Vendor.Company.CompanyName.ToLower().Contains(search));
                }

                int totalRecords = await query.CountAsync();

                var invoices = await query
                    .OrderBy(x => x.DueDate)
                    .Skip((filter.Page - 1) * filter.PageSize)
                    .Take(filter.PageSize)
                    .ToListAsync();

                var result = invoices.Select(x => new APOutstandingDto
                {
                    APInvoiceId = x.APInvoiceId,
                    InvoiceNumber = x.InvoiceNumber,
                    VendorName = x.Vendor.Company.CompanyName,
                    InvoiceAmount = x.Amount,
                    OutstandingAmount = x.Amount - x.Payments.Sum(p => p.Amount),
                    DueDate = x.DueDate,
                    PaymentStatus = x.PaymentStatus
                }).ToList();

                _cache.Set(
                    cacheKey,
                    result,
                    new MemoryCacheEntryOptions
                    {
                        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
                    });

                return ApiResponseHelper.SuccessRes(
                    result,
                    "Outstanding Report Generated Successfully",
                    totalRecords);
            }
            catch (Exception ex)
            {
                return ApiResponseHelper.Failure<List<APOutstandingDto>>(
                    "Unable to fetch outstanding report",
                    "OUTSTANDING_ERROR",
                    ex.Message);
            }
        }

        #endregion
        #region Aging Report

        public async Task<ApiResponse<APAgingReportDto>> GetAgingReportAsync(APAgingFilterDto filter)
        {
            try
            {
                var query = _context.APInvoices
                    .Include(x => x.Payments)
                    .Where(x => x.PaymentStatus != "Paid");

                if (filter.VendorId.HasValue)
                {
                    query = query.Where(x => x.VendorId == filter.VendorId.Value);
                }

                var invoices = await query.ToListAsync();

                decimal current = 0;
                decimal days1To30 = 0;
                decimal days31To60 = 0;
                decimal days61To90 = 0;
                decimal above90 = 0;

                var today = DateTime.UtcNow.Date;

                foreach (var invoice in invoices)
                {
                    var paidAmount = invoice.Payments.Sum(x => x.Amount);
                    var outstanding = invoice.Amount - paidAmount;

                    if (outstanding <= 0)
                        continue;

                    var overdueDays = (today - invoice.DueDate.Date).Days;

                    if (overdueDays <= 0)
                    {
                        current += outstanding;
                    }
                    else if (overdueDays <= 30)
                    {
                        days1To30 += outstanding;
                    }
                    else if (overdueDays <= 60)
                    {
                        days31To60 += outstanding;
                    }
                    else if (overdueDays <= 90)
                    {
                        days61To90 += outstanding;
                    }
                    else
                    {
                        above90 += outstanding;
                    }
                }

                var report = new APAgingReportDto
                {
                    Current = (int)current,
                    Days1To30 = (int)days1To30,
                    Days31To60 = (int)days31To60,
                    Days61To90 = (int)days61To90,
                    Above90Days = (int)above90,
                    TotalOutstandingAmount = (int)(current +
                                    days1To30 +
                                    days31To60 +
                                    days61To90 +
                                    above90)
                };

                return ApiResponseHelper.SuccessRes(
                    report,
                    "Aging Report Generated Successfully");
            }
            catch (Exception ex)
            {
                return ApiResponseHelper.Failure<APAgingReportDto>(
                    "Unable to generate aging report",
                    "AGING_ERROR",
                    ex.Message);
            }
        }

        #endregion


        #region Update Invoice

        public async Task<ApiResponse<APInvoiceResponseDto>> UpdateAsync(
            int id,
            UpdateAPInvoiceRequestDto request)
        {
            try
            {
                var invoice = await _context.APInvoices
                    .Include(x => x.Vendor)
                    .Include(x => x.PurchaseOrder)
                    .Include(x => x.GRN)
                    .Include(x => x.WorkOrder)
                    .FirstOrDefaultAsync(x => x.APInvoiceId == id);

                if (invoice == null)
                {
                    return ApiResponseHelper.Failure<APInvoiceResponseDto>(
                        "AP Invoice not found",
                        "NOT_FOUND",
                        "Invalid Invoice Id");
                }

                if (invoice.ApprovalStatus == "Approved")
                {
                    return ApiResponseHelper.Failure<APInvoiceResponseDto>(
                        "Approved AP Invoice cannot be updated",
                        "INVALID_OPERATION",
                        "Invoice already approved");
                }

                if (invoice.PaymentStatus == "Paid")
                {
                    return ApiResponseHelper.Failure<APInvoiceResponseDto>(
                        "Paid AP Invoice cannot be updated",
                        "INVALID_OPERATION",
                        "Invoice already paid");
                }

                var vendor = await _context.Vendors
                    .FirstOrDefaultAsync(x => x.VendorId == request.VendorId);

                if (vendor == null)
                {
                    return ApiResponseHelper.Failure<APInvoiceResponseDto>(
                        "Vendor not found",
                        "NOT_FOUND",
                        "Invalid Vendor Id");
                }

                var po = await _context.PurchaseOrders
                    .FirstOrDefaultAsync(x => x.POId == request.PurchaseOrderId);

                if (po == null)
                {
                    return ApiResponseHelper.Failure<APInvoiceResponseDto>(
                        "Purchase Order not found",
                        "NOT_FOUND",
                        "Invalid Purchase Order");
                }

                var grn = await _context.GRNs
                    .FirstOrDefaultAsync(x => x.GRNId == request.GRNId);

                if (grn == null)
                {
                    return ApiResponseHelper.Failure<APInvoiceResponseDto>(
                        "GRN not found",
                        "NOT_FOUND",
                        "Invalid GRN");
                }

                if (request.WorkOrderId.HasValue)
                {
                    var workOrder = await _context.WorkOrders
                        .FirstOrDefaultAsync(x => x.WorkOrderId == request.WorkOrderId);

                    if (workOrder == null)
                    {
                        return ApiResponseHelper.Failure<APInvoiceResponseDto>(
                            "Work Order not found",
                            "NOT_FOUND",
                            "Invalid Work Order");
                    }
                }

                _mapper.Map(request, invoice);

                invoice.ModifiedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();


                _cache.Remove(CacheKey);

                var response = await _context.APInvoices
    .Include(x => x.Vendor)
        .ThenInclude(v => v.Company)
    .Include(x => x.PurchaseOrder)
    .Include(x => x.GRN)
    .Include(x => x.WorkOrder)
    .Where(x => x.APInvoiceId == invoice.APInvoiceId)
    .ProjectTo<APInvoiceResponseDto>(_mapper.ConfigurationProvider)
    .FirstAsync();

                return ApiResponseHelper.SuccessRes(
                    response,
                    "AP Invoice Updated Successfully");
            }
            catch (Exception ex)
            {
                return ApiResponseHelper.Failure<APInvoiceResponseDto>(
                    "Unable to update invoice",
                    "UPDATE_ERROR",
                    ex.Message);
            }
        }

        #endregion

        #region Delete Invoice

        public async Task<ApiResponse<string>> DeleteAsync(int id)
        {
            try
            {
                var invoice = await _context.APInvoices
                    .Include(x => x.Payments)
                    .FirstOrDefaultAsync(x => x.APInvoiceId == id);

                if (invoice == null)
                {
                    return ApiResponseHelper.Failure<string>(
                        "AP Invoice not found",
                        "NOT_FOUND",
                        "Invalid Invoice Id");
                }

                if (invoice.ApprovalStatus == "Approved")
                {
                    return ApiResponseHelper.Failure<string>(
                        "Approved AP Invoice cannot be deleted",
                        "INVALID_OPERATION",
                        "Invoice already approved");
                }

                if (invoice.PaymentStatus == "Paid")
                {
                    return ApiResponseHelper.Failure<string>(
                        "Paid AP Invoice cannot be deleted",
                        "INVALID_OPERATION",
                        "Invoice already paid");
                }

                if (invoice.Payments != null && invoice.Payments.Any())
                {
                    return ApiResponseHelper.Failure<string>(
                        "Invoice cannot be deleted because payment has already been recorded",
                        "INVALID_OPERATION",
                        "Payment exists");
                }

                _context.APInvoices.Remove(invoice);

                await _context.SaveChangesAsync();

                _cache.Remove(CacheKey);

                return ApiResponseHelper.SuccessRes(
                    "AP Invoice Deleted Successfully");
            }
            catch (Exception ex)
            {
                return ApiResponseHelper.Failure<string>(
                    "Unable to delete invoice",
                    "DELETE_ERROR",
                    ex.Message);
            }
        }

        #endregion
    }

}