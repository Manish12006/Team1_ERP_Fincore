using AutoMapper;
using Azure;
using Fincore.Application.DTO;
using Fincore.Application.DTO.MasterTable;
using Fincore.Application.Interfaces.IMasterTable;
using Fincore.Domain.Models;
using Fincore.Infrastructure.CommonHelper;
using Fincore.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using NPOI.SS.UserModel;

namespace Fincore.Infrastructure.Services.MasterTable
{
    public class DocumentTypeService : IDocumentTypeService
    {
         AppDbContext db;
         IMapper mapper;

        public DocumentTypeService(AppDbContext db, IMapper mapper)
        {
            this.db = db;
            this.mapper = mapper;
        }

        public async Task<ApiResponse<DocumentTypeDto>> AddDocumentType(CreateDocumentTypeDto dto)
        {
            var data = mapper.Map<DocumentType>(dto);

            data.CreatedAt = DateTime.Now;
            data.ModifiedAt = DateTime.Now;

            data.ModifiedBy = dto.CreatedBy;

            db.DocumentTypes.Add(data);

            var result = await db.SaveChangesAsync();

            if (result <= 0)
            {
                return ApiResponseHelper.Failure<DocumentTypeDto>
                (
                    "Document Type Not Added",
                    "500",
                    "Failed to Add Document Type"
                );
            }

            var res = mapper.Map<DocumentTypeDto>(data);

            return ApiResponseHelper.SuccessRes
            (
                res,
                "Document Type Added Successfully"
            );
        }

        public async Task<ApiResponse<List<DocumentTypeDto>>> GetAllDocumentType(int page, int pageSize)
        {
            var totalRecords = await db.DocumentTypes.CountAsync();

            var data = await db.DocumentTypes
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            if (!data.Any())
            {
                return ApiResponseHelper.Failure<List<DocumentTypeDto>>
                (
                    "Document Type Not Found",
                    "404",
                    "Invalid Document Type"
                );
            }

            var res = mapper.Map<List<DocumentTypeDto>>(data);

            return ApiResponseHelper.SuccessRes
            (
                res,
                "Document Types Fetch Successfully",
                totalRecords,
                new
                {
                    page,
                    pageSize
                }
            );
        
        
            
        }
        public async Task<ApiResponse<DocumentTypeDto>> GetByIdDocumentType(int id)
        {
            var data = await db.DocumentTypes
                .FirstOrDefaultAsync(x => x.DocumentTypeId == id);

            if (data == null)
            {
                return ApiResponseHelper.Failure<DocumentTypeDto>
                (
                    "Document Type Not Found",
                    "404",
                    "Invalid Document Type"
                );
            }

            var res = mapper.Map<DocumentTypeDto>(data);

            return ApiResponseHelper.SuccessRes
            (
                res,
                "Document Type Fetch Successfully"
            );
        }

        public async Task<ApiResponse<DocumentTypeDto>> UpdateDocumentType(int id, UpdateDocumentTypeDto dto)
        {
            var data = await db.DocumentTypes
                .FirstOrDefaultAsync(x => x.DocumentTypeId == id);

            if (data == null)
            {
                return ApiResponseHelper.Failure<DocumentTypeDto>
                (
                    "Document Type Not Found",
                    "404",
                    "Invalid Document Type Id"
                );
            }

            mapper.Map(dto, data);

            data.ModifiedAt = DateTime.Now;
            data.ModifiedBy = dto.ModifiedBy;

            var result = await db.SaveChangesAsync();

            if (result <= 0)
            {
                return ApiResponseHelper.Failure<DocumentTypeDto>
                (
                    "Document Type Not Updated",
                    "500",
                    "Failed to Update Document Type"
                );
            }

            var res = mapper.Map<DocumentTypeDto>(data);

            return ApiResponseHelper.SuccessRes
            (
                res,
                "Document Type Updated Successfully"
            );
        }

        public async Task<ApiResponse<bool>> DeleteDocumentType(int id)
        {
            try
            {
                var data = await db.DocumentTypes
                    .FirstOrDefaultAsync(x => x.DocumentTypeId == id);

                if (data == null)
                {
                    return ApiResponseHelper.Failure<bool>(
                        "Document Type Not Found",
                        "404",
                        "Invalid Document Type Id");
                }

                db.DocumentTypes.Remove(data);

                await db.SaveChangesAsync();

                return ApiResponseHelper.SuccessRes(
                    true,
                    "Document Type Deleted Successfully");
            }
            catch (DbUpdateException ex)
            {
                if (ex.InnerException is Microsoft.Data.SqlClient.SqlException sqlEx &&
                    sqlEx.Number == 547)
                {
                    return ApiResponseHelper.Failure<bool>(
                        "Document Type Cannot Be Deleted",
                        "409",
                        "This Document Type is already used in Documents.");
                }

                throw;
            }
        }
    }


}