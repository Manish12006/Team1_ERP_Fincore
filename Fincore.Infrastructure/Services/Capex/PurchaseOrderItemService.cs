using AutoMapper;
using Fincore.Application.Constants;
using Fincore.Application.DTO;
using Fincore.Application.DTO.Capex;
using Fincore.Application.DTO.Capex.PurchaseOrderItems;
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

        

        public PurchaseOrderItemService( AppDbContext db, IMapper mapper)
        {
            this.db = db;
            this.mapper = mapper;
        }

        async Task IPurchaseOrderItemService.Create(POICreateDTO dto)
        {
            var data = mapper.Map<PurchaseOrderItem>(dto);
            //data.CreatedBy = 1;
            //data.CreatedAt = DateTime.Now;
            await db.PurchaseOrderItems.AddAsync(data);
            await db.SaveChangesAsync();
        }
        async Task IPurchaseOrderItemService.Update(POIUpdateDTO dto)
        {
            var datacheck =await db.PurchaseOrderItems.FirstOrDefaultAsync(x => x.POItemId == dto.POItemId);
            if (datacheck == null) throw new Exception("Data Not Found");

            var data = mapper.Map<PurchaseOrderItem>(dto);
            //data.ModifiedBy = 1;
            //data.ModifiedAt = DateTime.Now;
            db.PurchaseOrderItems.Update(data);
            await db.SaveChangesAsync();

        }
        async Task IPurchaseOrderItemService.Delete(int id)
        {
            var dataCheck = await db.PurchaseOrderItems.FirstOrDefaultAsync(x => x.POItemId == id);
            if (dataCheck == null) throw new Exception("Data Not Found");

            //dataCheck.IsActive = 0;
            //dataCheck.ModifiedBy = 1;
            //dataCheck.ModifiedAt = DateTime.Now;

            await db.SaveChangesAsync();
        }

        async Task<POIItemDTO> IPurchaseOrderItemService.ReadById(int id)
        {
            var dataCheck = await db.PurchaseOrderItems.FirstOrDefaultAsync(x => x.POItemId == id);
            if (dataCheck == null) throw new Exception("Data Not Found");

            var data = mapper.Map<POIItemDTO>(dataCheck);
            return data;
        }
        async Task<List<POIItemDTO>> IPurchaseOrderItemService.ReadAll()
        {
            //var data = await db.PurchaseOrderItems.AsNoTracking().Where(x => x.IsActive == 1).ToListAsync();
            var data = await db.PurchaseOrderItems.AsNoTracking().ToListAsync();
            var data2 = mapper.Map<List<POIItemDTO>>(data);
            return data2;
        }
        Task IPurchaseOrderItemService.DropDownQuotationId()
        {
            throw new NotImplementedException();
        }
    }
}