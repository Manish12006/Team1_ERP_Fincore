using AutoMapper;
using Fincore.Application.DTO;
using Fincore.Application.DTO.Capex;
using Fincore.Application.DTO.Capex.PurchaseOrder;
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
using System.CodeDom;
using Document = QuestPDF.Fluent.Document;

namespace Fincore.Infrastructure.Services.Capex
{
    public class PurchaseOrderService : IPurchaseOrderService
    {
        private readonly AppDbContext db;
        private readonly IMapper mapper;

        public PurchaseOrderService(AppDbContext db, IMapper mapper, IMemoryCache cache)
        {
            this.db = db;
            this.mapper = mapper;
        }

        public async Task Create(PMCreateDTO dto)
        {
            var data = mapper.Map<PurchaseOrder>(dto);
            data.CreatedBy = 1;
            data.CreatedAt = DateTime.Now;
            await db.PurchaseOrders.AddAsync(data);
            await db.SaveChangesAsync();
        }

        public async Task Update(PMUpdateDTO dto)
        {

            var dataCheck = await db.PurchaseOrders.FirstOrDefaultAsync(x => x.POId == dto.POId && x.IsActive == 1);
            if(dataCheck == null)
            {
                throw new Exception("Data not found");
            }

            var data = mapper.Map<PurchaseOrder>(dto);
            data.ModifiedBy = 1;
            data.ModifiedAt = DateTime.Now;

            db.PurchaseOrders.Update(data);
            await db.SaveChangesAsync();

        }

        public async Task Delete(int id)
        {
            var dataCheck = await db.PurchaseOrders.FirstOrDefaultAsync(x => x.POId == id && x.IsActive == 1);
            if (dataCheck == null)
            {
                throw new Exception("Data not found");
            }

            dataCheck.IsActive = 0;
            dataCheck.ModifiedBy = 1;
            dataCheck.ModifiedAt = DateTime.Now;

            await db.SaveChangesAsync();

        }

        public async Task<PMItemDTO> ReadById(int id)
        {
            var dataCheck = await db.PurchaseOrders.FirstOrDefaultAsync(x => x.POId == id && x.IsActive == 1);
            if (dataCheck == null)
            {
                throw new Exception("Data not found");
            }

            var data = mapper.Map<PMItemDTO>(dataCheck);
            return data;

        }

        public async Task<List<PMItemDTO>> ReadAll()
        {

            var data = await db.PurchaseOrders.AsNoTracking().Where(x => x.IsActive == 1).ToListAsync();
            var data2 = mapper.Map<List<PMItemDTO>>(data);
            return data2;

        }


















        public Task DropDownQuotation()
        {
            throw new NotImplementedException();
        }

    }
}