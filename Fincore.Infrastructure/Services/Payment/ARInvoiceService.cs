using AutoMapper;
using Fincore.Application.DTO;
using Fincore.Application.DTO.Payment.AccountsReceivable.Requests;
using Fincore.Application.DTO.Payment.AccountsReceivable.Responses;
using Fincore.Application.Interfaces.IPayment;
using Fincore.Domain.Models;
using Fincore.Infrastructure.CommonHelper;
using Fincore.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace Fincore.Infrastructure.Services.PaymentModule
{
    public class ARInvoiceService : IARInvoiceService
    {
        private readonly AppDbContext db;
        private readonly IMapper mapper;
        private readonly IMemoryCache cache;

        private const string ARInvoiceCacheKey = "ARInvoices";

        public ARInvoiceService(
            AppDbContext db,
            IMapper mapper,
            IMemoryCache cache)
        {
            this.db = db;
            this.mapper = mapper;
            this.cache = cache;
        }

        public async Task<ApiResponse<ARInvoiceResponseDto>> CreateInvoiceAsync(
    CreateARInvoiceRequestDto request)
        {
            bool customerExists = await db.Customers
                .AnyAsync(x => x.CustomerId == request.CustomerId);

            if (!customerExists)
                throw new Exception("Customer not found.");

            bool revenueExists = await db.RevenueEntries
                .AnyAsync(x => x.RevenueEntryId == request.RevenueEntryId);

            if (!revenueExists)
                throw new Exception("Revenue Entry not found.");

            var lastInvoice = await db.ARInvoices
                .OrderByDescending(x => x.ARInvoiceId)
                .FirstOrDefaultAsync();

            int nextId = (lastInvoice?.ARInvoiceId ?? 0) + 1;

            string invoiceNumber = $"ARINV-{DateTime.Now.Year}-{nextId:D5}";

            var invoice = mapper.Map<ARInvoice>(request);

            invoice.InvoiceNumber = invoiceNumber;
            invoice.AmountReceived = 0;
            invoice.AmountOutstanding = request.Amount;
            invoice.PaymentStatus = "Unpaid";
            invoice.CreatedAt = DateTime.Now;
            invoice.ModifiedAt = DateTime.Now;

            await db.ARInvoices.AddAsync(invoice);
            await db.SaveChangesAsync();

            cache.Remove(ARInvoiceCacheKey);

            var result = await db.ARInvoices
                .AsNoTracking()
                .Include(x => x.Customer)
                    .ThenInclude(x => x.Company)
                .Include(x => x.RevenueEntry)
                .FirstOrDefaultAsync(x => x.ARInvoiceId == invoice.ARInvoiceId);

            var response = mapper.Map<ARInvoiceResponseDto>(result);

            return ApiResponseHelper.SuccessRes(
                response,
                "AR Invoice created successfully.");
        }


        public async Task<ApiResponse<List<ARInvoiceResponseDto>>> GetAllInvoicesAsync(
    int page,
    int pageSize)
        {
            if (page <= 0)
                page = 1;

            if (pageSize <= 0)
                pageSize = 10;

            if (pageSize > 100)
                pageSize = 100;

            string cacheKey = $"ARInvoices_Page_{page}_Size_{pageSize}";

            if (cache.TryGetValue(cacheKey, out List<ARInvoiceResponseDto> cachedData))
            {
                int total = await db.ARInvoices.CountAsync();

                return ApiResponseHelper.SuccessRes(
                    cachedData,
                    "AR Invoices fetched successfully.",
                    total);
            }

            var query = db.ARInvoices
                .AsNoTracking()
                .Include(x => x.Customer)
                    .ThenInclude(x => x.Company)
                .Include(x => x.RevenueEntry);

            int totalRecords = await query.CountAsync();

            var invoices = await query
                .OrderBy(x => x.ARInvoiceId)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var result = mapper.Map<List<ARInvoiceResponseDto>>(invoices);

            cache.Set(
                cacheKey,
                result,
                new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow =
                        TimeSpan.FromMinutes(5)
                });

            return ApiResponseHelper.SuccessRes(
                result,
                "AR Invoices fetched successfully.",
                totalRecords);
        }



        public async Task<ApiResponse<ARInvoiceResponseDto>> GetInvoiceByIdAsync(
    int arInvoiceId)
        {
            string cacheKey = $"ARInvoice_{arInvoiceId}";

            if (cache.TryGetValue(cacheKey, out ARInvoiceResponseDto cachedInvoice))
            {
                return ApiResponseHelper.SuccessRes(
                    cachedInvoice,
                    "AR Invoice fetched successfully.");
            }

            var invoice = await db.ARInvoices
                .AsNoTracking()
                .Include(x => x.Customer)
                    .ThenInclude(x => x.Company)
                .Include(x => x.RevenueEntry)
                .FirstOrDefaultAsync(x => x.ARInvoiceId == arInvoiceId);

            if (invoice == null)
                throw new Exception("AR Invoice not found.");

            var response = mapper.Map<ARInvoiceResponseDto>(invoice);

            cache.Set(
                cacheKey,
                response,
                new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow =
                        TimeSpan.FromMinutes(5)
                });

            return ApiResponseHelper.SuccessRes(
                response,
                "AR Invoice fetched successfully.");
        }


        public async Task<ApiResponse<ARInvoiceResponseDto>> UpdateInvoiceAsync(
    int arInvoiceId,
    UpdateARInvoiceRequestDto request)
        {
            var invoice = await db.ARInvoices
                .FirstOrDefaultAsync(x => x.ARInvoiceId == arInvoiceId);

            if (invoice == null)
                throw new Exception("AR Invoice not found.");

            // Paid invoices cannot be modified
            if (invoice.PaymentStatus == "Paid")
                throw new Exception("Paid invoice cannot be updated.");

            bool customerExists = await db.Customers
                .AnyAsync(x => x.CustomerId == request.CustomerId);

            if (!customerExists)
                throw new Exception("Customer not found.");

            bool revenueExists = await db.RevenueEntries
                .AnyAsync(x => x.RevenueEntryId == request.RevenueEntryId);

            if (!revenueExists)
                throw new Exception("Revenue Entry not found.");

            invoice.CustomerId = request.CustomerId;
            invoice.RevenueEntryId = request.RevenueEntryId;
            invoice.InvoiceDate = request.InvoiceDate;
            invoice.DueDate = request.DueDate;
            invoice.Amount = request.Amount;

            // Recalculate Outstanding Amount
            invoice.AmountOutstanding =
                request.Amount - (invoice.AmountReceived ?? 0);

            invoice.ModifiedAt = DateTime.Now;

            await db.SaveChangesAsync();

            cache.Remove(ARInvoiceCacheKey);
            cache.Remove($"ARInvoice_{arInvoiceId}");

            var updatedInvoice = await db.ARInvoices
                .AsNoTracking()
                .Include(x => x.Customer)
                    .ThenInclude(x => x.Company)
                .Include(x => x.RevenueEntry)
                .FirstOrDefaultAsync(x => x.ARInvoiceId == arInvoiceId);

            var response = mapper.Map<ARInvoiceResponseDto>(updatedInvoice);

            return ApiResponseHelper.SuccessRes(
                response,
                "AR Invoice updated successfully.");
        }

        public async Task<ApiResponse<string>> DeleteInvoiceAsync(
    int arInvoiceId)
        {
            var invoice = await db.ARInvoices
                .FirstOrDefaultAsync(x => x.ARInvoiceId == arInvoiceId);

            if (invoice == null)
                throw new Exception("AR Invoice not found.");

            // Do not allow deletion if any payment has been received
            if ((invoice.AmountReceived ?? 0) > 0)
                throw new Exception("Invoice cannot be deleted because payment has already been received.");

            db.ARInvoices.Remove(invoice);

            await db.SaveChangesAsync();

            cache.Remove(ARInvoiceCacheKey);
            cache.Remove($"ARInvoice_{arInvoiceId}");

            return ApiResponseHelper.SuccessRes(
                "AR Invoice deleted successfully.");
        }

        public async Task<ApiResponse<ARPaymentResponseDto>> ReceivePaymentAsync(
    CreateARPaymentRequestDto request)
        {
            var invoice = await db.ARInvoices
                .FirstOrDefaultAsync(x => x.ARInvoiceId == request.ARInvoiceId);

            if (invoice == null)
                throw new Exception("AR Invoice not found.");

            if (request.Amount <= 0)
                throw new Exception("Payment amount should be greater than zero.");

            decimal outstanding = invoice.AmountOutstanding ?? invoice.Amount;

            if (request.Amount > outstanding)
                throw new Exception("Payment amount cannot be greater than outstanding amount.");

            var lastPayment = await db.Payments
                .OrderByDescending(x => x.PaymentId)
                .FirstOrDefaultAsync();

            int nextId = (lastPayment?.PaymentId ?? 0) + 1;

            string paymentNumber = $"PAY-{DateTime.Now.Year}-{nextId:D5}";

            var payment = new Fincore.Domain.Models.Payment
            {
                PaymentNumber = paymentNumber,
                PaymentType = "AR",
                ARInvoiceId = invoice.ARInvoiceId,
                CustomerId = invoice.CustomerId,
                Amount = request.Amount,
                PaymentDate = request.PaymentDate,
                PaymentMethod = request.PaymentMethod,
                ApprovalStatus = "Approved",
                ReconciledFlag = false,
                CreatedAt = DateTime.Now,
                ModifiedAt = DateTime.Now
            };

            await db.Payments.AddAsync(payment);

            invoice.AmountReceived =
                (invoice.AmountReceived ?? 0) + request.Amount;

            invoice.AmountOutstanding =
                invoice.Amount - invoice.AmountReceived;

            if (invoice.AmountOutstanding == 0)
                invoice.PaymentStatus = "Paid";
            else
                invoice.PaymentStatus = "PartiallyPaid";

            invoice.ModifiedAt = DateTime.Now;

            await db.SaveChangesAsync();

            cache.Remove(ARInvoiceCacheKey);
            cache.Remove($"ARInvoice_{invoice.ARInvoiceId}");

            var response = new ARPaymentResponseDto
            {
                ARInvoiceId = invoice.ARInvoiceId,
                InvoiceNumber = invoice.InvoiceNumber,
                PaidAmount = invoice.AmountReceived ?? 0,
                OutstandingAmount = invoice.AmountOutstanding ?? 0,
                PaymentStatus = invoice.PaymentStatus,
                PaymentDate = payment.PaymentDate
            };

            return ApiResponseHelper.SuccessRes(
                response,
                "Payment received successfully.");
        }

        public async Task<ApiResponse<List<AROutstandingResponseDto>>> GetOutstandingInvoicesAsync()
        {
            var invoices = await db.ARInvoices
                .AsNoTracking()
                .Include(x => x.Customer)
                    .ThenInclude(x => x.Company)
                .Include(x => x.RevenueEntry)
                .Where(x => (x.AmountOutstanding ?? 0) > 0)
                .OrderBy(x => x.DueDate)
                .ToListAsync();

            var result = mapper.Map<List<AROutstandingResponseDto>>(invoices);

            return ApiResponseHelper.SuccessRes(
                result,
                "Outstanding invoices fetched successfully.",
                result.Count);
        }

        public async Task<ApiResponse<ARAgingResponseDto>> GetAgingReportAsync()
        {
            var invoices = await db.ARInvoices
                .AsNoTracking()
                .Where(x => (x.AmountOutstanding ?? 0) > 0)
                .ToListAsync();

            var report = new ARAgingResponseDto();

            foreach (var invoice in invoices)
            {
                int days = (DateTime.Today - invoice.DueDate.Date).Days;

                decimal amount = invoice.AmountOutstanding ?? 0;

                if (days <= 0)
                {
                    report.Current++;
                    report.CurrentAmount += amount;
                }
                else if (days <= 30)
                {
                    report.Days1To30++;
                    report.Days1To30Amount += amount;
                }
                else if (days <= 60)
                {
                    report.Days31To60++;
                    report.Days31To60Amount += amount;
                }
                else if (days <= 90)
                {
                    report.Days61To90++;
                    report.Days61To90Amount += amount;
                }
                else
                {
                    report.Above90Days++;
                    report.Above90DaysAmount += amount;
                }
            }

            return ApiResponseHelper.SuccessRes(
                report,
                "AR Aging report fetched successfully.");
        }



    }
}








