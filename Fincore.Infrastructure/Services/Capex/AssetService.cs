using AutoMapper;
using Fincore.Application.DTO;
using Fincore.Application.DTO.Capex;
using Fincore.Application.DTO.Capex.Assets;
using Fincore.Application.DTO.Capex.PurchaseOrder;
using Fincore.Application.Interfaces.ICapex;
using Fincore.Domain.Models;
using Fincore.Infrastructure.CommonHelper;
using Fincore.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace Fincore.Infrastructure.Services.Capex
{
    public class AssetService : IAssetService
    {
        private readonly AppDbContext db;
        private readonly IMapper mapper;
       

        private const string AssetCacheKey = "Asset";


        public AssetService(AppDbContext db,IMapper mapper)
        {
            this.db = db;
            this.mapper = mapper;
        }

        public async Task Create(AssetsCreateDTO dto)
        {
            var data = mapper.Map<Asset>(dto);
            //data.CreatedBy = 1;
            //data.CreatedAt = DateTime.Now;
            await db.Assets.AddAsync(data);
            await db.SaveChangesAsync();

        }

        public async Task Update(AssetsUpdateDTO dto)
        {
            // var dataCheck = await db.Assets.FirstOrDefaultAsync(x => x.AssetId == dto.AssetId && x.IsActive == 1);
            var dataCheck = await db.Assets.FirstOrDefaultAsync(x => x.AssetId == dto.AssetId);
            if (dataCheck == null) throw new Exception("Data not found");
            var data = mapper.Map<Asset>(dto);
            //data.ModifiedBy = 1;
            data.ModifiedAt = DateTime.Now;
            db.Assets.Update(data);
            await db.SaveChangesAsync();
        }

        public async Task Delete(int id)
        {
            //var dataCheck = await db.Assets.FirstOrDefaultAsync(x => x.AssetId == id && x.IsActive == 1);
            var dataCheck = await db.Assets.FirstOrDefaultAsync(x => x.AssetId == id);
            if (dataCheck == null) throw new Exception("Data not found");
            //dataCheck.IsActive = 0;
            //dataCheck.ModifiedBy = 1;
            dataCheck.ModifiedAt = DateTime.Now;

            await db.SaveChangesAsync();
        }

        public async Task<AssetsItemDTO> ReadById(int id)
        {
            //var dataCheck = await db.Assets.FirstOrDefaultAsync(x => x.AssetId == id && x.IsActive == 1);
            var dataCheck = await db.Assets.FirstOrDefaultAsync(x => x.AssetId == id);
            if (dataCheck == null) throw new Exception("Data not found");

            var data = mapper.Map<AssetsItemDTO>(dataCheck);
            return data;

        }

        public async Task<List<AssetsItemDTO>> ReadAll()
        {
            //var data = await db.Assets.AsNoTracking().Where(x => x.IsActive == 1).ToListAsync();
            var data = await db.Assets.AsNoTracking().ToListAsync();
            var data2 = mapper.Map<List<AssetsItemDTO>>(data);
            return data2;
        }
    }
}