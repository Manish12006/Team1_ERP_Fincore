using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Fincore.Application.DTO;
using Fincore.Application.DTO.Payment;
using Fincore.Application.Interfaces.IPayment;
using Fincore.Domain.Enums;
using Fincore.Domain.Models;
using Fincore.Infrastructure.CommonHelper;
using Fincore.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using Microsoft.Extensions.Caching.Memory;

namespace Fincore.Infrastructure.Services.PaymentModule
{
    public class PaymentService : IPaymentService
    {
        IMemoryCache cache;
        AppDbContext db;
        IMapper mapper;

        public PaymentService(AppDbContext db, IMapper mapper, IMemoryCache cache)
        {
            this.cache = cache;
            this.db = db;
            this.mapper = mapper;
        }

        public async Task AddPaymentAsync(PaymentPostDTO dto)
        {
            
            dto.PaymentMethod = dto.PaymentMethod?.Trim();

            if (string.IsNullOrWhiteSpace(dto.PaymentMethod))
            {
                throw new Exception("Payment Method is required.");
            }

            if (dto.PaymentType == PaymentType.AP)
            {
                if (!dto.APInvoiceId.HasValue || dto.APInvoiceId <= 0)
                {
                    throw new Exception("Please select an AP Invoice.");
                }

                if (dto.ARInvoiceId.HasValue && dto.ARInvoiceId > 0)
                {
                    throw new Exception("AR Invoice should not be selected for AP Payment.");
                }
            }
            else
            {
                if (!dto.ARInvoiceId.HasValue || dto.ARInvoiceId <= 0)
                {
                    throw new Exception("Please select an AR Invoice.");
                }

                if (dto.APInvoiceId.HasValue && dto.APInvoiceId > 0)
                {
                    throw new Exception("AP Invoice should not be selected for AR Payment.");
                }
            }

            var lastPayment = await db.Payments
                .OrderByDescending(x => x.PaymentId)
                .FirstOrDefaultAsync();

            int nextId = 1;

            if (lastPayment != null)
            {
                nextId = lastPayment.PaymentId + 1;
            }

            string paymentNumber = "PAY-" + DateTime.Now.Year + "-" + nextId.ToString("D5");

            var payment = mapper.Map<Domain.Models.Payment>(dto);

            payment.PaymentNumber = paymentNumber;
            payment.ApprovalStatus = "Pending";
            payment.ReconciledFlag = false;
            payment.CreatedAt = DateTime.Now;
            payment.ModifiedAt = DateTime.Now;

            if (dto.PaymentType == PaymentType.AP)
            {
                var invoice = await db.APInvoices
                    .FirstOrDefaultAsync(x => x.APInvoiceId == dto.APInvoiceId);

                if (invoice == null)
                {
                    throw new Exception("AP Invoice not found.");
                }

                if (invoice.ApprovalStatus != "Approved")
                {
                    throw new Exception("Payment can only be made for approved AP Invoices.");
                }

                if (invoice.PaymentStatus == "Paid")
                {
                    throw new Exception("This AP Invoice is already paid.");
                }

                payment.APInvoiceId = invoice.APInvoiceId;
                payment.ARInvoiceId = null;
                payment.VendorId = invoice.VendorId;
                payment.CustomerId = null;
                payment.Amount = invoice.Amount;
            }
            else
            {
                var invoice = await db.ARInvoices
                    .FirstOrDefaultAsync(x => x.ARInvoiceId == dto.ARInvoiceId);

                if (invoice == null)
                {
                    throw new Exception("AR Invoice not found.");
                }

                if (invoice.PaymentStatus == "Paid")
                {
                    throw new Exception("This AR Invoice is already paid.");
                }

                payment.ARInvoiceId = invoice.ARInvoiceId;
                payment.APInvoiceId = null;
                payment.CustomerId = invoice.CustomerId;
                payment.VendorId = null;
                payment.Amount = invoice.Amount;
            }

            payment.PaymentType = dto.PaymentType.ToString();

            await db.Payments.AddAsync(payment);
            await db.SaveChangesAsync();

            ClearPaymentCache();
        }

        public async Task DeletePaymentAsync(int id)
        {
           
            if (id <= 0)
            {
                throw new Exception("Invalid Payment Id.");
            }

            var payment = await db.Payments
                .FirstOrDefaultAsync(x => x.PaymentId == id);

            if (payment == null)
            {
                throw new Exception("Payment not found.");
            }

            
            if (payment.ApprovalStatus == "Approved")
            {
                throw new Exception("Approved payments cannot be deleted.");
            }

            if (payment.ReconciledFlag == true)
            {
                throw new Exception("Reconciled payments cannot be deleted.");
            }

            db.Payments.Remove(payment);

            await db.SaveChangesAsync();

            
            cache.Remove($"Payment_{id}");
            ClearPaymentCache();
        }

        public async Task<ApiResponse<List<PaymentGetDTO>>> GetAllPayment(int page, int pageSize)
        {
            
            if (page <= 0 || pageSize <= 0)
            {
                return ApiResponseHelper.Failure<List<PaymentGetDTO>>(
                    "Invalid Pagination.",
                    "INVALID_PAGINATION",
                    "Page and PageSize must be greater than zero."
                );
            }

            string cacheKey = $"Payments_{page}_{pageSize}";

           
            if (cache.TryGetValue(cacheKey, out List<PaymentGetDTO> payments))
            {
                return ApiResponseHelper.SuccessRes(
                    payments,
                    "Payments fetched successfully!",
                    await db.Payments.CountAsync()
                );
            }

            
            var data = await db.Payments
                .OrderBy(x => x.PaymentId)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            
            if (data.Count == 0)
            {
                return ApiResponseHelper.Failure<List<PaymentGetDTO>>(
                    "No Payments Found.",
                    "NO_DATA_FOUND",
                    "No payment records are available."
                );
            }

            var result = mapper.Map<List<PaymentGetDTO>>(data);


            cache.Set(cacheKey, result, TimeSpan.FromMinutes(5));

            return ApiResponseHelper.SuccessRes(
                result,
                "Payments fetched successfully!",
                await db.Payments.CountAsync()
            );
        }

        public async Task<ApiResponse<PaymentGetDTO>> GetPaymentById(int id)
        {
            
            if (id <= 0)
            {
                return ApiResponseHelper.Failure<PaymentGetDTO>(
                    "Invalid Payment Id.",
                    "INVALID_PAYMENT_ID",
                    "Payment Id must be greater than zero."
                );
            }

            string cacheKey = $"Payment_{id}";

            
            if (cache.TryGetValue(cacheKey, out PaymentGetDTO payment))
            {
                return ApiResponseHelper.SuccessRes(
                    payment,
                    "Payment fetched successfully!",
                    1
                );
            }

            
            var data = await db.Payments
                .FirstOrDefaultAsync(x => x.PaymentId == id);

            if (data == null)
            {
                return ApiResponseHelper.Failure<PaymentGetDTO>(
                    "Payment not found.",
                    "PAYMENT_NOT_FOUND",
                    $"No payment found with Id {id}."
                );
            }

            var result = mapper.Map<PaymentGetDTO>(data);

            
            cache.Set(cacheKey, result, TimeSpan.FromMinutes(5));

            return ApiResponseHelper.SuccessRes(
                result,
                "Payment fetched successfully!",
                1
            );
        }

        public async Task<ApiResponse<List<PaymentGetDTO>>> GetPaymentStatus(PaymentStatus ps,int page,int pageSize)
        {
            
            if (page <= 0 || pageSize <= 0)
            {
                return ApiResponseHelper.Failure<List<PaymentGetDTO>>(
                    "Invalid Pagination.",
                    "INVALID_PAGINATION",
                    "Page and PageSize must be greater than zero."
                );
            }

            string paymentStatus = ps.ToString();

            string cacheKey = $"Payment_Status_{paymentStatus}_{page}_{pageSize}";

            
            if (cache.TryGetValue(cacheKey, out List<PaymentGetDTO> payments))
            {
                return ApiResponseHelper.SuccessRes(
                    payments,
                    $"Payments with status {paymentStatus} fetched successfully!",
                    await db.Payments.CountAsync(x => x.ApprovalStatus == paymentStatus)
                );
            }

            
            var data = await db.Payments
                .Where(x => x.ApprovalStatus == paymentStatus)
                .OrderBy(x => x.PaymentId)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            
            if (data.Count == 0)
            {
                return ApiResponseHelper.Failure<List<PaymentGetDTO>>(
                    "No Payments Found.",
                    "NO_DATA_FOUND",
                    $"No payments found with status '{paymentStatus}'."
                );
            }

            var result = mapper.Map<List<PaymentGetDTO>>(data);

            
            cache.Set(cacheKey, result, TimeSpan.FromMinutes(5));

            return ApiResponseHelper.SuccessRes(
                result,
                $"Payments with status {paymentStatus} fetched successfully!",
                await db.Payments.CountAsync(x => x.ApprovalStatus == paymentStatus)
            );
        }

        public async Task<ApiResponse<List<PaymentGetDTO>>> GetPaymentType(PaymentType pt,int page,int pageSize)
        {
            
            if (page <= 0 || pageSize <= 0)
            {
                return ApiResponseHelper.Failure<List<PaymentGetDTO>>(
                    "Invalid Pagination.",
                    "INVALID_PAGINATION",
                    "Page and PageSize must be greater than zero."
                );
            }
 

            string paymentType = pt.ToString();

            string cacheKey = $"Payment_{paymentType}_{page}_{pageSize}";

            
            if (cache.TryGetValue(cacheKey, out List<PaymentGetDTO> payments))
            {
                return ApiResponseHelper.SuccessRes(
                    payments,
                    $"Payments of type {paymentType} fetched successfully!",
                    await db.Payments.CountAsync(x => x.PaymentType == paymentType)
                );
            }

            
            var data = await db.Payments
                .Where(x => x.PaymentType == paymentType)
                .OrderBy(x => x.PaymentId)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            
            if (data.Count == 0)
            {
                return ApiResponseHelper.Failure<List<PaymentGetDTO>>(
                    "No Payments Found.",
                    "NO_DATA_FOUND",
                    $"No payments found for Payment Type '{paymentType}'."
                );
            }

            var result = mapper.Map<List<PaymentGetDTO>>(data);

            
            cache.Set(cacheKey, result, TimeSpan.FromMinutes(5));

            return ApiResponseHelper.SuccessRes(
                result,
                $"Payments of type {paymentType} fetched successfully!",
                await db.Payments.CountAsync(x => x.PaymentType == paymentType)
            );
        }

        public async Task UpdateApproval(int id)
        {
            if (id <= 0)
            {
                throw new Exception("Invalid Payment Id.");
            }

            var payment = await db.Payments
                .FirstOrDefaultAsync(x => x.PaymentId == id);

            if (payment == null)
            {
                throw new Exception("Payment not found.");
            }

            
            if (payment.ApprovalStatus == "Approved")
            {
                throw new Exception("Payment is already approved.");
            }

            
            if (payment.ReconciledFlag == true)
            {
                throw new Exception("Reconciled payment cannot be approved again.");
            }

           
            if (payment.PaymentType == "AP")
            {
                var invoice = await db.APInvoices
                    .FirstOrDefaultAsync(x => x.APInvoiceId == payment.APInvoiceId);

                if (invoice == null)
                {
                    throw new Exception("Associated AP Invoice not found.");
                }

                if (invoice.ApprovalStatus != "Approved")
                {
                    throw new Exception("Associated AP Invoice is not approved.");
                }
            }
 
            if (payment.PaymentType == "AR")
            {
                var invoice = await db.ARInvoices
                    .FirstOrDefaultAsync(x => x.ARInvoiceId == payment.ARInvoiceId);

                if (invoice == null)
                {
                    throw new Exception("Associated AR Invoice not found.");
                }
            }

            //JWT
            payment.ApprovedBy = 1;

            payment.ApprovalStatus = "Approved";
            payment.ModifiedAt = DateTime.Now;

            await db.SaveChangesAsync();

           
            cache.Remove($"Payment_{id}");
            ClearPaymentCache();
        }
        public async Task UpdatePaymentAsync(int id, PaymentUpdateDTO dto)
        {
            
            if (id <= 0)
            {
                throw new Exception("Invalid Payment Id.");
            }

            dto.PaymentMethod = dto.PaymentMethod?.Trim();

            
            if (string.IsNullOrWhiteSpace(dto.PaymentMethod))
            {
                throw new Exception("Payment Method is required.");
            }

            
            

            var payment = await db.Payments
                .FirstOrDefaultAsync(x => x.PaymentId == id);

            if (payment == null)
            {
                throw new Exception("Payment not found.");
            }

            
            if (payment.ApprovalStatus == "Approved")
            {
                throw new Exception("Approved payments cannot be updated.");
            }

            if (payment.ReconciledFlag == true)
            {
                throw new Exception("Reconciled payments cannot be updated.");
            }

            payment.PaymentMethod = dto.PaymentMethod;
            payment.PaymentDate = dto.PaymentDate;
            payment.ModifiedAt = DateTime.Now;

            await db.SaveChangesAsync();

            cache.Remove($"Payment_{id}");
            ClearPaymentCache();
        }

        public async Task UpdateReconcile(int id)
        {
            
            if (id <= 0)
            {
                throw new Exception("Invalid Payment Id.");
            }

            var payment = await db.Payments
                .FirstOrDefaultAsync(x => x.PaymentId == id);

            if (payment == null)
            {
                throw new Exception("Payment not found.");
            }

            
            if (payment.ApprovalStatus != "Approved")
            {
                throw new Exception("Payment must be approved before reconciliation.");
            }

            
            if (payment.ReconciledFlag == true)
            {
                throw new Exception("Payment is already reconciled.");
            }

            
            if (payment.PaymentType == "AP")
            {
                var invoice = await db.APInvoices
                    .FirstOrDefaultAsync(x => x.APInvoiceId == payment.APInvoiceId);

                if (invoice == null)
                {
                    throw new Exception("Associated AP Invoice not found.");
                }

                invoice.PaymentStatus = "Paid";
            }
            else
            {
                var invoice = await db.ARInvoices
                    .FirstOrDefaultAsync(x => x.ARInvoiceId == payment.ARInvoiceId);

                if (invoice == null)
                {
                    throw new Exception("Associated AR Invoice not found.");
                }

                invoice.PaymentStatus = "Paid";

                invoice.AmountReceived = payment.Amount;
                invoice.AmountOutstanding = invoice.Amount - invoice.AmountReceived;
            }

            payment.ReconciledFlag = true;
            payment.ModifiedAt = DateTime.Now;

            await db.SaveChangesAsync();

            
            cache.Remove($"Payment_{id}");
            ClearPaymentCache();
        }


        private const int MaxPage = 20;
        private const int MaxPageSize = 100;

        private void ClearPaymentCache()
        {
            
            for (int page = 1; page <= MaxPage; page++)
            {
                for (int pageSize = 1; pageSize <= MaxPageSize; pageSize++)
                {
                    cache.Remove($"Payments_{page}_{pageSize}");

                    foreach (var paymentType in Enum.GetNames(typeof(PaymentType)))
                    {
                        cache.Remove($"Payment_{paymentType}_{page}_{pageSize}");
                    }

                    foreach (var status in Enum.GetNames(typeof(PaymentStatus)))
                    {
                        cache.Remove($"Payment_Status_{status}_{page}_{pageSize}");
                    }
                }
            }
        }
    }
}

