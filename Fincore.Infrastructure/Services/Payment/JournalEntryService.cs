using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Fincore.Application.DTO;
using Fincore.Application.DTO.Payment;
using Fincore.Application.Interfaces.IPayment;
using Fincore.Domain.Models;
using Fincore.Infrastructure.CommonHelper;
using Fincore.Infrastructure.Data;
using MathNet.Numerics.Distributions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace Fincore.Infrastructure.Services.PaymentModule
{
    public class JournalEntryService : IJournalEntryService
    {
        IMemoryCache cache;
        AppDbContext db;
        IMapper mapper;

        public JournalEntryService(AppDbContext db, IMapper mapper, IMemoryCache cache)
        {
            this.cache = cache;
            this.db = db;
            this.mapper = mapper;
        }


        public async Task AddJournalEntryAsync(JournalEntryPostDTO dto)
        {
            
            dto.Description = dto.Description?.Trim();

            
            var account = await db.AccountMasters
                .FirstOrDefaultAsync(x => x.AccountId == dto.AccountId);

            if (account == null)
                throw new Exception("Account not found.");

            
            if (account.IsActive == 0)
                throw new Exception("Selected account is inactive.");

            
            

            
            bool hasDebit = dto.DebitAmount.HasValue && dto.DebitAmount > 0;
            bool hasCredit = dto.CreditAmount.HasValue && dto.CreditAmount > 0;

            if (!hasDebit && !hasCredit)
                throw new Exception("Either Debit or Credit amount is required.");

            if (hasDebit && hasCredit)
                throw new Exception("Only one of Debit or Credit can have a value.");

            var lastJournal = await db.JournalEntries
            .OrderByDescending(x => x.JournalEntryId)
            .FirstOrDefaultAsync();

            int nextId;

            if (lastJournal == null)
            {
                nextId = 1;
            }
            else
            {
                nextId = lastJournal.JournalEntryId + 1;
            }

            string journalNumber = "JV-" + DateTime.Now.Year + "-" + nextId.ToString("D5");

            
            var journal = mapper.Map<JournalEntry>(dto);

            journal.JournalNumber = journalNumber;
            journal.CreatedBy = 1;                                                              // Replace with JWT later
            journal.CreatedAt = DateTime.Now;
            journal.ModifiedAt = DateTime.Now;

            await db.JournalEntries.AddAsync(journal);
            await db.SaveChangesAsync();
            ClearJournalCache();

        }


        public async Task<ApiResponse<List<JournalEntryGetDTO>>> GetAllJournalEntries(int page, int pageSize)
        {
            
            if (page <= 0 || pageSize <= 0)
            {
                return ApiResponseHelper.Failure<List<JournalEntryGetDTO>>(
                    "Invalid pagination.",
                    "INVALID_PAGINATION",
                    "Page and PageSize must be greater than zero."
                );
            }

            string cacheKey = $"JournalEntries_{page}_{pageSize}";

            
            if (cache.TryGetValue(cacheKey, out List<JournalEntryGetDTO> journals))
            {
                return ApiResponseHelper.SuccessRes(
                    journals,
                    "Journal Entries fetched successfully!",
                    await db.JournalEntries.CountAsync()
                );
            }

            
            var data = await db.JournalEntries
                .OrderBy(x => x.JournalEntryId)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            
            if (data.Count == 0)
            {
                return ApiResponseHelper.Failure<List<JournalEntryGetDTO>>(
                    "No Journal Entries Found.",
                    "NO_DATA_FOUND",
                    "No Journal Entries are available."
                );
            }

            var result = mapper.Map<List<JournalEntryGetDTO>>(data);

           
            cache.Set(cacheKey, result, TimeSpan.FromMinutes(5));

            return ApiResponseHelper.SuccessRes(
                result,
                "Journal Entries fetched successfully!",
                await db.JournalEntries.CountAsync()
            );
        }


        public async Task DeleteJournalEntryAsync(int id)
        {
           
            var journal = await db.JournalEntries
                .FirstOrDefaultAsync(x => x.JournalEntryId == id);

            if (journal == null)
            {
                throw new Exception("Journal Entry not found.");
            }
            
            db.JournalEntries.Remove(journal);

            await db.SaveChangesAsync();

           
            ClearJournalCache();
            
        }



        public async Task<ApiResponse<JournalEntryGetDTO>> GetJournalEntryById(int id)
        {
            string cacheKey = $"JournalEntry_{id}";

            
            if (cache.TryGetValue(cacheKey, out JournalEntryGetDTO journal))
            {
                return ApiResponseHelper.SuccessRes(
                    journal,
                    "Journal Entry fetched successfully!",
                    1
                );
            }

            
            var data = await db.JournalEntries
                .FirstOrDefaultAsync(x => x.JournalEntryId == id);

            
            if (data == null)
            {
                return ApiResponseHelper.Failure<JournalEntryGetDTO>(
                    "Journal Entry not found.",
                    "JOURNAL_ENTRY_NOT_FOUND",
                    $"No Journal Entry found with Id : {id}"
                );
            }

            var result = mapper.Map<JournalEntryGetDTO>(data);

            
            cache.Set(cacheKey, result, TimeSpan.FromMinutes(5));

            return ApiResponseHelper.SuccessRes(
                result,
                "Journal Entry fetched successfully!",
                1
            );
        }



        public async Task UpdateJournalEntryAsync(int id, JournalEntryUpdateDTO dto)
        {
           
            dto.Description = dto.Description?.Trim();

            
            var journal = await db.JournalEntries
                .FirstOrDefaultAsync(x => x.JournalEntryId == id);

            if (journal == null)
            {
                throw new Exception("Journal Entry not found.");
            }

            
            var account = await db.AccountMasters
                .FirstOrDefaultAsync(x => x.AccountId == dto.AccountId);

            if (account == null)
            {
                throw new Exception("Account not found.");
            }

            
            if (account.IsActive == 0)
            {
                throw new Exception("Selected account is inactive.");
            }

            
            

            
            bool hasDebit = dto.DebitAmount.HasValue && dto.DebitAmount > 0;
            bool hasCredit = dto.CreditAmount.HasValue && dto.CreditAmount > 0;

            if (!hasDebit && !hasCredit)
            {
                throw new Exception("Either Debit or Credit amount is required.");
            }

            if (hasDebit && hasCredit)
            {
                throw new Exception("Only one of Debit or Credit can have a value.");
            }

            
            journal.EntryDate = dto.EntryDate;
            journal.AccountId = dto.AccountId;
            journal.DebitAmount = dto.DebitAmount;
            journal.CreditAmount = dto.CreditAmount;
            journal.Description = dto.Description;
            journal.ModifiedAt = DateTime.Now;

            await db.SaveChangesAsync();

            ClearJournalCache();
            cache.Remove($"JournalEntry_{id}");


        }

        private void ClearJournalCache()
        {
            for (int page = 1; page <= 20; page++)
            {
                for (int pageSize = 1; pageSize <= 100; pageSize++)
                {
                    cache.Remove($"JournalEntries_{page}_{pageSize}");
                }
            }
        }
    }
}
