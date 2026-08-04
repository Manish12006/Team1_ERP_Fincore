using AutoMapper;
using Fincore.Application.DTO;
using Fincore.Application.DTO.Capex;
using Fincore.Application.DTO.Capex.GRN;
using Fincore.Application.Interfaces.ICapex;
using Fincore.Domain.Enums;
using Fincore.Domain.Models;
using Fincore.Infrastructure.CommonHelper;
using Fincore.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace Fincore.Infrastructure.Services.Capex
{
    public class GRNService : IGRNService
    {
        private readonly AppDbContext db;
        private readonly IMapper mapper;
        

        public GRNService(AppDbContext db,IMapper mapper)
        {
            this.db = db;
            this.mapper = mapper;
           
        }

        public async Task Create(GRNCreateDTO dto)
        {
            var data = mapper.Map<GRN>(dto);
            data.CreatedBy = 1;
            data.CreatedAt = DateTime.Now;
            await db.GRNs.AddAsync(data);
            await db.SaveChangesAsync();
        }

        public async Task Update(GRNUpdateDTO dto)
        {
            var dataCheck = await db.GRNs.FirstOrDefaultAsync(x => x.GRNId == dto.GRNId && x.IsActive == 1);
            if (dataCheck == null) throw new Exception("Data not found");

            var data = mapper.Map<GRN>(dto);
            //data.ModifiedBy = 1;
            data.ModifiedAt = DateTime.Now;

            db.GRNs.Update(data);
            await db.SaveChangesAsync();
        }

        public async Task Delete(int id)
        {
            var dataCheck = await db.GRNs.FirstOrDefaultAsync(x => x.GRNId == id && x.IsActive == 1);
            if (dataCheck == null) throw new Exception("Data not found");

            dataCheck.IsActive = 0;
           // dataCheck.ModifiedBy = 1;
            dataCheck.ModifiedAt = DateTime.Now;

            await db.SaveChangesAsync();
        }

        public async Task<GRNItemDTO> ReadById(int id)
        {
            var dataCheck = await db.GRNs.FirstOrDefaultAsync(x => x.GRNId == id && x.IsActive == 1);
            if(dataCheck==null) throw new Exception("Data not found");

            var data = mapper.Map<GRNItemDTO>(dataCheck);
            return data;
        }

        public async Task<List<GRNItemDTO>> ReadAll()
        {
            var data = await db.GRNs.AsNoTracking().Where(x => x.IsActive == 1).ToListAsync();
            var data2 = mapper.Map<List<GRNItemDTO>>(data);
            return data2;
        }
    }
}