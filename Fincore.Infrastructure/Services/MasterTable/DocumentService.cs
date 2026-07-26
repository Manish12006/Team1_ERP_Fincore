using AutoMapper;
using Fincore.Application.DTO;
using Fincore.Application.DTO.MasterTable;
using Fincore.Application.Interfaces.IMasterTable;
using Fincore.Domain.Models;
using Fincore.Infrastructure.CommonHelper;
using Fincore.Infrastructure.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;

namespace Fincore.Infrastructure.Services.MasterTable
{
    public class DocumentService : IDocumentService
    {
        private readonly AppDbContext db;
        private readonly IMapper mapper;
        private readonly IWebHostEnvironment env;

        public DocumentService(
            AppDbContext db,
            IMapper mapper,
            IWebHostEnvironment env)
        {
            this.db = db;
            this.mapper = mapper;
            this.env = env;
        }

        public async Task<ApiResponse<List<DocumentDto>>> GetAll(int page, int pageSize)
        {
            var totalRecords = await db.Documents.CountAsync();

            var data = await db.Documents
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            if (!data.Any())
            {
                return ApiResponseHelper.Failure<List<DocumentDto>>(
                    "Document Not Found",
                    "404",
                    "Invalid Document");
            }

            var result = mapper.Map<List<DocumentDto>>(data);

            return ApiResponseHelper.SuccessRes(
                result,
                "Documents Fetch Successfully",
                totalRecords,
                new
                {
                    page,
                    pageSize
                });
        }

        public async Task<ApiResponse<DocumentDto>> DocumentGetById(int id)
        {
            var data = await db.Documents
                .FirstOrDefaultAsync(x => x.DocumentsId == id);

            if (data == null)
            {
                return ApiResponseHelper.Failure<DocumentDto>(
                    "Document Not Found",
                    "404",
                    "Invalid Document Id");
            }

            var result = mapper.Map<DocumentDto>(data);

            return ApiResponseHelper.SuccessRes(
                result,
                "Document Get Successfully");
        }

        public async Task<ApiResponse<DocumentDto>> AddDocument(CreateDocumentDto dto)
        {
            // File Required Validation
            if (dto.FilePath == null || dto.FilePath.Length == 0)
            {
                return ApiResponseHelper.Failure<DocumentDto>(
                    "File Required",
                    "400",
                    "Please Select File");
            }

            // Maximum File Size = 5 MB
            const long maxFileSize = 5 * 1024 * 1024;

            if (dto.FilePath.Length > maxFileSize)
            {
                return ApiResponseHelper.Failure<DocumentDto>(
                    "Invalid File Size",
                    "400",
                    "File size should not exceed 5 MB");
            }

            // Allowed File Extensions
            string[] allowedExtensions =
            {
        ".pdf",
        ".xls",
        ".xlsx",
        ".png",
        ".jpg",
        ".jpeg"
    };

            string extension = Path.GetExtension(dto.FilePath.FileName).ToLower();

            if (!allowedExtensions.Contains(extension))
            {
                return ApiResponseHelper.Failure<DocumentDto>(
                    "Invalid File Format",
                    "400",
                    "Only PDF, XLS, XLSX, PNG, JPG and JPEG files are allowed");
            }

            string folderName = "";

            switch (dto.MasterTypeId)
            {
                case 1:
                    folderName = "Company";
                    break;

                case 2:
                    folderName = "Vendors";
                    break;

                case 3:
                    folderName = "Employee";
                    break;
                case 4:
                    folderName = "Customer";
                    break;

                default:
                    return ApiResponseHelper.Failure<DocumentDto>(
                        "Invalid Master Type",
                        "400",
                        "MasterTypeId is invalid");
            }

            int folderId = dto.EntityId ?? 0;

            string folderPath = Path.Combine(
                env.ContentRootPath,
                "Uploads",
                "Documents",
                folderName,
                folderId.ToString());

            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            string uniqueFileName = Guid.NewGuid().ToString() + extension;

            string physicalPath = Path.Combine(folderPath, uniqueFileName);

            using (FileStream stream = new FileStream(physicalPath, FileMode.Create))
            {
                await dto.FilePath.CopyToAsync(stream);
            }

            var data = mapper.Map<Document>(dto);

            data.FileName = dto.FilePath.FileName;
            data.FileType = dto.FilePath.ContentType;

            data.FilePath = Path.Combine(
                "Uploads",
                "Documents",
                folderName,
                folderId.ToString(),
                uniqueFileName).Replace("\\", "/");

            data.CreatedAt = DateTime.Now;
            data.ModifiedAt = DateTime.Now;

            db.Documents.Add(data);

            await db.SaveChangesAsync();

            var result = mapper.Map<DocumentDto>(data);

            return ApiResponseHelper.SuccessRes(
                result,
                "Document Added Successfully");
        }



        public async Task<ApiResponse<DocumentDto>> UpdateDocument(int id, UpdateDocumentDto dto)
        {
            var data = await db.Documents
                .FirstOrDefaultAsync(x => x.DocumentsId == id);

            if (data == null)
            {
                return ApiResponseHelper.Failure<DocumentDto>(
                    "Document Not Found",
                    "404",
                    "Invalid Document Id");
            }

            data.DocumentTypeId = dto.DocumentTypeId;
            data.UserId = dto.UserId;
            data.EntityId = dto.EntityId;
            data.MasterTypeId = dto.MasterTypeId;

            if (dto.FilePath != null && dto.FilePath.Length > 0)
            {
                // Delete old file
                if (!string.IsNullOrEmpty(data.FilePath))
                {
                    string oldFile = Path.Combine(
                        env.ContentRootPath,
                        data.FilePath.Replace("/", Path.DirectorySeparatorChar.ToString()));

                    if (File.Exists(oldFile))
                    {
                        File.Delete(oldFile);
                    }
                }

                string folderName = "";

                switch (dto.MasterTypeId)
                {
                    case 1:
                        folderName = "Employee";
                        break;

                    case 2:
                        folderName = "Vendors";
                        break;

                    case 3:
                        folderName = "Company";
                        break;

                    case 4:
                        folderName = "Employee";
                        break;
                }

                int folderId = dto.EntityId ?? 0;

                string folderPath = Path.Combine(
                    env.ContentRootPath,
                    "Uploads",
                    "Documents",
                    folderName,
                    folderId.ToString());

                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }

                string extension = Path.GetExtension(dto.FilePath.FileName);

                string uniqueFileName = Guid.NewGuid() + extension;

                string physicalPath = Path.Combine(folderPath, uniqueFileName);

                using (var stream = new FileStream(physicalPath, FileMode.Create))
                {
                    await dto.FilePath.CopyToAsync(stream);
                }

                data.FileName = dto.FilePath.FileName;
                data.FileType = dto.FilePath.ContentType;

                data.FilePath = Path.Combine(
                    "Uploads",
                    "Documents",
                    folderName,
                    folderId.ToString(),
                    uniqueFileName).Replace("\\", "/");
            }

            data.ModifiedAt = DateTime.Now;

            await db.SaveChangesAsync();

            var result = mapper.Map<DocumentDto>(data);

            return ApiResponseHelper.SuccessRes(
                result,
                "Document Updated Successfully");
        }

        public async Task<ApiResponse<bool>> DeleteDocument(int id)
        {
            var data = await db.Documents
                .FirstOrDefaultAsync(x => x.DocumentsId == id);

            if (data == null)
            {
                return ApiResponseHelper.Failure<bool>(
                    "Document Not Found",
                    "404",
                    "Invalid Document Id");
            }

            // Delete Physical File
            if (!string.IsNullOrEmpty(data.FilePath))
            {
                string physicalPath = Path.Combine(
                    env.ContentRootPath,
                    data.FilePath.Replace("/", Path.DirectorySeparatorChar.ToString()));

                if (File.Exists(physicalPath))
                {
                    File.Delete(physicalPath);
                }
            }

            db.Documents.Remove(data);

            await db.SaveChangesAsync();

            return ApiResponseHelper.SuccessRes(
                true,
                "Document Deleted Successfully");
        }
    }
}
    
