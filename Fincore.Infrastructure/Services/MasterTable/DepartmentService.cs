using AutoMapper;
using Fincore.Application.DTO;
using Fincore.Application.DTO.MasterTable;
using Fincore.Application.Interfaces.IMasterTable;
using Fincore.Domain.Enums;
using Fincore.Domain.Models;
using Fincore.Infrastructure.CommonHelper;
using Fincore.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace Fincore.Infrastructure.Services.MasterTable
{
    public class DepartmentService : IDepartmentService
    {
        private readonly AppDbContext db;
        private readonly IMapper mapper;
        private readonly IMemoryCache cache;
        private static int departmentCacheVersion = 1;

        public DepartmentService(
            AppDbContext db,
            IMapper mapper,
            IMemoryCache cache)
        {
            this.db = db;
            this.mapper = mapper;
            this.cache = cache;
        }

        private IQueryable<Department> GetDepartmentQuery()
        {
            return db.Departments
                .Where(x => x.IsActive == (byte)IsActive.Active)
                .Include(x => x.Company)
                .Include(x => x.MasterType)
                .Include(x => x.Manager)
                    .ThenInclude(x => x.User);
        }

        public async Task<ApiResponse<IEnumerable<DepartmentDTO>>> GetAllDepartmentsAsync(
            int pageNumber,
            int pageSize,
            string? search)
        {
            try
            {
                if (pageNumber <= 0)
                {
                    pageNumber = 1;
                }

                if (pageSize <= 0)
                {
                    pageSize = 10;
                }

                if (!string.IsNullOrEmpty(search))
                {
                    search = search.Trim();
                }

                string cacheKey =
                    $"departments_{departmentCacheVersion}_page_{pageNumber}_size_{pageSize}_search_{search}";

                if (cache.TryGetValue(
                    cacheKey,
                    out ApiResponse<IEnumerable<DepartmentDTO>> cachedData))
                {
                    Console.WriteLine(
                        "GET ALL DEPARTMENTS - Data returned from CACHE");

                    return cachedData;
                }

                Console.WriteLine(
                    "GET ALL DEPARTMENTS - Data returned from DATABASE");

                IQueryable<Department> query = GetDepartmentQuery();

                if (!string.IsNullOrEmpty(search))
                {
                    query = query.Where(x =>
                        x.DepartmentCode.Contains(search) ||
                        x.DepartmentName.Contains(search));
                }

                int totalRecords = await query.CountAsync();

                List<Department> departments = await query
                    .OrderBy(x => x.DepartmentId)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                IEnumerable<DepartmentDTO> departmentDTOs =
                    mapper.Map<IEnumerable<DepartmentDTO>>(departments);

                var metadata = new
                {
                    CurrentPage = pageNumber,
                    PageSize = pageSize,
                    TotalPages = (int)Math.Ceiling(
                        (double)totalRecords / pageSize)
                };

                ApiResponse<IEnumerable<DepartmentDTO>> response =
                    ApiResponseHelper.SuccessRes(
                        departmentDTOs,
                        "Departments fetched successfully.",
                        totalRecords,
                        metadata);

                cache.Set(
                    cacheKey,
                    response,
                    TimeSpan.FromMinutes(5));

                return response;
            }
            catch (Exception ex)
            {
                return ApiResponseHelper.Failure<IEnumerable<DepartmentDTO>>(
                    "Failed to retrieve departments.",
                    "DEPARTMENT_GET_ERROR",
                    ex.Message);
            }
        }

        public async Task<ApiResponse<DepartmentDTO>> GetDepartmentByIdAsync(
            int id)
        {
            try
            {
                string cacheKey = $"department_{id}";

                if (cache.TryGetValue(
                    cacheKey,
                    out ApiResponse<DepartmentDTO> cachedData))
                {
                    Console.WriteLine(
                        "GET DEPARTMENT BY ID - Data returned from CACHE");

                    return cachedData;
                }

                Console.WriteLine(
                    "GET DEPARTMENT BY ID - Data returned from DATABASE");

                Department department = await GetDepartmentQuery()
                    .FirstOrDefaultAsync(x => x.DepartmentId == id);

                if (department == null)
                {
                    return ApiResponseHelper.Failure<DepartmentDTO>(
                        "Department not found.",
                        "404",
                        "Invalid Department Id.");
                }

                DepartmentDTO departmentDTO =
                    mapper.Map<DepartmentDTO>(department);

                ApiResponse<DepartmentDTO> response =
                    ApiResponseHelper.SuccessRes(
                        departmentDTO,
                        "Department fetched successfully.");

                cache.Set(
                    cacheKey,
                    response,
                    TimeSpan.FromMinutes(5));

                return response;
            }
            catch (Exception ex)
            {
                return ApiResponseHelper.Failure<DepartmentDTO>(
                    "Failed to retrieve department.",
                    "DEPARTMENT_GET_ERROR",
                    ex.Message);
            }
        }

        public async Task<ApiResponse<DepartmentDTO>> CreateDepartmentAsync(
            DepartmentDTO departmentDTO)
        {
            try
            {
                Company company = await db.Companies
                    .FirstOrDefaultAsync(x =>
                        x.CompanyId == departmentDTO.CompanyId &&
                        x.IsActive == (byte)IsActive.Active);

                if (company == null)
                {
                    return ApiResponseHelper.Failure<DepartmentDTO>(
                        "Company not found.",
                        "404",
                        "Invalid Company Id.");
                }

                Department existingDepartment = await db.Departments
                    .FirstOrDefaultAsync(x =>
                        x.CompanyId == departmentDTO.CompanyId &&
                        x.DepartmentName.ToLower() ==
                        departmentDTO.DepartmentName.ToLower() &&
                        x.IsActive == (byte)IsActive.Active);

                if (existingDepartment != null)
                {
                    return ApiResponseHelper.Failure<DepartmentDTO>(
                        "Department already exists.",
                        "409",
                        "Department name already exists for this company.");
                }

                Department existingCode = await db.Departments
                    .FirstOrDefaultAsync(x =>
                        x.CompanyId == departmentDTO.CompanyId &&
                        x.DepartmentCode.ToLower() ==
                        departmentDTO.DepartmentCode.ToLower() &&
                        x.IsActive == (byte)IsActive.Active);

                if (existingCode != null)
                {
                    return ApiResponseHelper.Failure<DepartmentDTO>(
                        "Department code already exists.",
                        "409",
                        "Department code already exists for this company.");
                }

                if (departmentDTO.MasterTypeId.HasValue)
                {
                    MasterType masterType = await db.MasterTypes
                        .FirstOrDefaultAsync(x =>
                            x.MasterTypeId ==
                            departmentDTO.MasterTypeId.Value);

                    if (masterType == null)
                    {
                        return ApiResponseHelper.Failure<DepartmentDTO>(
                            "Master type not found.",
                            "404",
                            "Invalid Master Type Id.");
                    }
                }

                if (departmentDTO.ManagerId.HasValue)
                {
                    Employee manager = await db.Employees
                        .FirstOrDefaultAsync(x =>
                            x.EmployeeId ==
                            departmentDTO.ManagerId.Value &&
                            x.IsActive == (byte)IsActive.Active);

                    if (manager == null)
                    {
                        return ApiResponseHelper.Failure<DepartmentDTO>(
                            "Manager not found.",
                            "404",
                            "Invalid Manager Id.");
                    }
                }

                Department department =
                    mapper.Map<Department>(departmentDTO);

                department.DepartmentId = 0;
                department.IsActive = (byte)IsActive.Active;
                department.CreatedAt = DateTime.Now;
                department.CreatedBy = 1;
                department.ModifiedAt = DateTime.Now;
                department.ModifiedBy = 1;

                await db.Departments.AddAsync(department);
                await db.SaveChangesAsync();

                departmentCacheVersion++;

                Department createdDepartment = await GetDepartmentQuery()
                    .FirstOrDefaultAsync(x =>
                        x.DepartmentId == department.DepartmentId);

                DepartmentDTO result =
                    mapper.Map<DepartmentDTO>(createdDepartment);

                ApiResponse<DepartmentDTO> response =
                    ApiResponseHelper.SuccessRes(
                        result,
                        "Department created successfully.");

                string cacheKey =
                    $"department_{department.DepartmentId}";

                cache.Set(
                    cacheKey,
                    response,
                    TimeSpan.FromMinutes(5));

                return response;
            }
            catch (Exception ex)
            {
                return ApiResponseHelper.Failure<DepartmentDTO>(
                    "Failed to create department.",
                    "DEPARTMENT_CREATE_ERROR",
                    ex.Message);
            }
        }

        public async Task<ApiResponse<DepartmentDTO>> UpdateDepartmentAsync(
            int id,
            DepartmentDTO departmentDTO)
        {
            try
            {
                Department department = await db.Departments
                    .FirstOrDefaultAsync(x =>
                        x.DepartmentId == id &&
                        x.IsActive == (byte)IsActive.Active);

                if (department == null)
                {
                    return ApiResponseHelper.Failure<DepartmentDTO>(
                        "Department not found.",
                        "404",
                        "Invalid Department Id.");
                }

                Company company = await db.Companies
                    .FirstOrDefaultAsync(x =>
                        x.CompanyId == departmentDTO.CompanyId &&
                        x.IsActive == (byte)IsActive.Active);

                if (company == null)
                {
                    return ApiResponseHelper.Failure<DepartmentDTO>(
                        "Company not found.",
                        "404",
                        "Invalid Company Id.");
                }

                Department existingDepartment = await db.Departments
                    .FirstOrDefaultAsync(x =>
                        x.CompanyId == departmentDTO.CompanyId &&
                        x.DepartmentName.ToLower() ==
                        departmentDTO.DepartmentName.ToLower() &&
                        x.DepartmentId != id &&
                        x.IsActive == (byte)IsActive.Active);

                if (existingDepartment != null)
                {
                    return ApiResponseHelper.Failure<DepartmentDTO>(
                        "Department already exists.",
                        "409",
                        "Department name already exists for this company.");
                }

                Department existingCode = await db.Departments
                    .FirstOrDefaultAsync(x =>
                        x.CompanyId == departmentDTO.CompanyId &&
                        x.DepartmentCode.ToLower() ==
                        departmentDTO.DepartmentCode.ToLower() &&
                        x.DepartmentId != id &&
                        x.IsActive == (byte)IsActive.Active);

                if (existingCode != null)
                {
                    return ApiResponseHelper.Failure<DepartmentDTO>(
                        "Department code already exists.",
                        "409",
                        "Department code already exists for this company.");
                }

                if (departmentDTO.MasterTypeId.HasValue)
                {
                    MasterType masterType = await db.MasterTypes
                        .FirstOrDefaultAsync(x =>
                            x.MasterTypeId ==
                            departmentDTO.MasterTypeId.Value);

                    if (masterType == null)
                    {
                        return ApiResponseHelper.Failure<DepartmentDTO>(
                            "Master type not found.",
                            "404",
                            "Invalid Master Type Id.");
                    }
                }

                if (departmentDTO.ManagerId.HasValue)
                {
                    Employee manager = await db.Employees
                        .FirstOrDefaultAsync(x =>
                            x.EmployeeId ==
                            departmentDTO.ManagerId.Value &&
                            x.IsActive == (byte)IsActive.Active);

                    if (manager == null)
                    {
                        return ApiResponseHelper.Failure<DepartmentDTO>(
                            "Manager not found.",
                            "404",
                            "Invalid Manager Id.");
                    }
                }

                department.CompanyId = departmentDTO.CompanyId;
                department.DepartmentName =
                    departmentDTO.DepartmentName;
                department.DepartmentCode =
                    departmentDTO.DepartmentCode;
                department.MasterTypeId =
                    departmentDTO.MasterTypeId;
                department.ManagerId =
                    departmentDTO.ManagerId;

                department.ModifiedAt = DateTime.Now;
                department.ModifiedBy = 1;

                await db.SaveChangesAsync();

                departmentCacheVersion++;

                string cacheKey = $"department_{id}";

                cache.Remove(cacheKey);

                Department updatedDepartment =
                    await GetDepartmentQuery()
                    .FirstOrDefaultAsync(x =>
                        x.DepartmentId == id);

                DepartmentDTO result =
                    mapper.Map<DepartmentDTO>(updatedDepartment);

                ApiResponse<DepartmentDTO> response =
                    ApiResponseHelper.SuccessRes(
                        result,
                        "Department updated successfully.");

                cache.Set(
                    cacheKey,
                    response,
                    TimeSpan.FromMinutes(5));

                return response;
            }
            catch (Exception ex)
            {
                return ApiResponseHelper.Failure<DepartmentDTO>(
                    "Failed to update department.",
                    "DEPARTMENT_UPDATE_ERROR",
                    ex.Message);
            }
        }

        public async Task<ApiResponse<bool>> DeleteDepartmentAsync(int id)
        {
            try
            {
                Department department = await db.Departments
                    .FirstOrDefaultAsync(x => x.DepartmentId == id);

                if (department == null)
                {
                    return ApiResponseHelper.Failure<bool>(
                        "Department not found.",
                        "404",
                        "Invalid Department Id.");
                }

                if (department.IsActive ==
                    (byte)IsActive.Inactive)
                {
                    return ApiResponseHelper.Failure<bool>(
                        "Department already deleted.",
                        "409",
                        "Department is already inactive.");
                }

                department.IsActive =
                    (byte)IsActive.Inactive;

                department.ModifiedAt = DateTime.Now;
                department.ModifiedBy = 1;

                await db.SaveChangesAsync();

                departmentCacheVersion++;

                string cacheKey = $"department_{id}";

                cache.Remove(cacheKey);

                return ApiResponseHelper.SuccessRes(
                    true,
                    "Department deleted successfully.");
            }
            catch (Exception ex)
            {
                return ApiResponseHelper.Failure<bool>(
                    "Failed to delete department.",
                    "DEPARTMENT_DELETE_ERROR",
                    ex.Message);
            }
        }
    }
}