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
    public class EmployeeService : IEmployeeService
    {
        private readonly AppDbContext db;
        private readonly IMapper mapper;
        private readonly IMemoryCache cache;
        private static int employeeCacheVersion = 1;

        public EmployeeService(
            AppDbContext db,
            IMapper mapper,
            IMemoryCache cache)
        {
            this.db = db;
            this.mapper = mapper;
            this.cache = cache;
        }

        private IQueryable<Employee> GetEmployeeQuery()
        {
            return db.Employees
                .Where(x => x.IsActive == (byte)IsActive.Active)
                .Include(x => x.User)
                .Include(x => x.Department)
                .Include(x => x.DesignationRole)
                .Include(x => x.Company)
                .Include(x => x.ReportingManagerEmployee)
                    .ThenInclude(x => x.User);
        }

        public async Task<ApiResponse<List<EmployeeDto>>> GetAllEmployeesAsync(
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
                    $"employees_{employeeCacheVersion}_page_{pageNumber}_size_{pageSize}_search_{search}";

                if (cache.TryGetValue(
                    cacheKey,
                    out ApiResponse<List<EmployeeDto>> cachedData))
                {
                    Console.WriteLine(
                        "GET ALL EMPLOYEES - Data returned from CACHE");

                    return cachedData;
                }

                Console.WriteLine(
                    "GET ALL EMPLOYEES - Data returned from DATABASE");

                IQueryable<Employee> query = GetEmployeeQuery();

                if (!string.IsNullOrEmpty(search))
                {
                    query = query.Where(x =>
                        x.EmployeeCode.Contains(search) ||
                        x.User.FullName.Contains(search));
                }

                int totalRecords = await query.CountAsync();

                List<Employee> employees = await query
                    .OrderBy(x => x.EmployeeId)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                List<EmployeeDto> employeeDtos =
                    mapper.Map<List<EmployeeDto>>(employees);

                var metadata = new
                {
                    CurrentPage = pageNumber,
                    PageSize = pageSize,
                    TotalPages = (int)Math.Ceiling(
                        (double)totalRecords / pageSize)
                };

                ApiResponse<List<EmployeeDto>> response =
                    ApiResponseHelper.SuccessRes(
                        employeeDtos,
                        "Employees fetched successfully.",
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
                return ApiResponseHelper.Failure<List<EmployeeDto>>(
                    "Failed to retrieve employees.",
                    "EMPLOYEE_GET_ERROR",
                    ex.Message);
            }
        }

        public async Task<ApiResponse<EmployeeDto>> GetEmployeeByIdAsync(
            int id)
        {
            try
            {
                string cacheKey = $"employee_{id}";

                if (cache.TryGetValue(
                    cacheKey,
                    out ApiResponse<EmployeeDto> cachedData))
                {
                    Console.WriteLine(
                        "GET EMPLOYEE BY ID - Data returned from CACHE");

                    return cachedData;
                }

                Console.WriteLine(
                    "GET EMPLOYEE BY ID - Data returned from DATABASE");

                Employee employee = await GetEmployeeQuery()
                    .FirstOrDefaultAsync(
                        x => x.EmployeeId == id);

                if (employee == null)
                {
                    return ApiResponseHelper.Failure<EmployeeDto>(
                        "Employee not found.",
                        "404",
                        "Invalid Employee Id.");
                }

                EmployeeDto employeeDto =
                    mapper.Map<EmployeeDto>(employee);

                ApiResponse<EmployeeDto> response =
                    ApiResponseHelper.SuccessRes(
                        employeeDto,
                        "Employee fetched successfully.");

                cache.Set(
                    cacheKey,
                    response,
                    TimeSpan.FromMinutes(5));

                return response;
            }
            catch (Exception ex)
            {
                return ApiResponseHelper.Failure<EmployeeDto>(
                    "Failed to retrieve employee.",
                    "EMPLOYEE_GET_ERROR",
                    ex.Message);
            }
        }

        public async Task<ApiResponse<EmployeeDto>> CreateEmployeeAsync(
            CreateEmployeeDto createEmployeeDto)
        {
            try
            {
                Employee existingEmployee = await db.Employees
                    .FirstOrDefaultAsync(x =>
                        x.EmployeeCode ==
                        createEmployeeDto.EmployeeCode);

                if (existingEmployee != null)
                {
                    return ApiResponseHelper.Failure<EmployeeDto>(
                        "Employee code already exists.",
                        "409",
                        "Employee code already exists.");
                }

                bool userExists = await db.Users
                    .AnyAsync(x =>
                        x.UserId == createEmployeeDto.UserId);

                if (!userExists)
                {
                    return ApiResponseHelper.Failure<EmployeeDto>(
                        "User not found.",
                        "404",
                        "Invalid User Id.");
                }

                bool userEmployeeExists = await db.Employees
                    .AnyAsync(x =>
                        x.UserId == createEmployeeDto.UserId);

                if (userEmployeeExists)
                {
                    return ApiResponseHelper.Failure<EmployeeDto>(
                        "User is already assigned to an employee.",
                        "409",
                        "User is already linked to an employee.");
                }

                Department department = await db.Departments
                    .FirstOrDefaultAsync(x =>
                        x.DepartmentId ==
                        createEmployeeDto.DepartmentId &&
                        x.IsActive == (byte)IsActive.Active);

                if (department == null)
                {
                    return ApiResponseHelper.Failure<EmployeeDto>(
                        "Department not found.",
                        "404",
                        "Invalid Department Id.");
                }

                bool designationExists = await db.Roles
                    .AnyAsync(x =>
                        x.RoleId == createEmployeeDto.Designation &&
                        x.IsActive == (byte)IsActive.Active);

                if (!designationExists)
                {
                    return ApiResponseHelper.Failure<EmployeeDto>(
                        "Designation not found.",
                        "404",
                        "Invalid Designation Role Id.");
                }

                bool companyExists = await db.Companies
                    .AnyAsync(x =>
                        x.CompanyId == createEmployeeDto.CompanyId &&
                        x.IsActive == (byte)IsActive.Active);

                if (!companyExists)
                {
                    return ApiResponseHelper.Failure<EmployeeDto>(
                        "Company not found.",
                        "404",
                        "Invalid Company Id.");
                }

                if (department.CompanyId !=
                    createEmployeeDto.CompanyId)
                {
                    return ApiResponseHelper.Failure<EmployeeDto>(
                        "Department does not belong to the selected company.",
                        "409",
                        "Department and Company do not match.");
                }

                if (createEmployeeDto.ReportingManager.HasValue)
                {
                    Employee reportingManager = await db.Employees
                        .FirstOrDefaultAsync(x =>
                            x.EmployeeId ==
                            createEmployeeDto.ReportingManager.Value &&
                            x.IsActive ==
                            (byte)IsActive.Active);

                    if (reportingManager == null)
                    {
                        return ApiResponseHelper.Failure<EmployeeDto>(
                            "Reporting manager not found.",
                            "404",
                            "Invalid Reporting Manager Id.");
                    }

                    if (reportingManager.CompanyId !=
                        createEmployeeDto.CompanyId)
                    {
                        return ApiResponseHelper.Failure<EmployeeDto>(
                            "Reporting manager belongs to another company.",
                            "409",
                            "Reporting manager must belong to the same company.");
                    }
                }

                Employee employee =
                    mapper.Map<Employee>(createEmployeeDto);

                employee.EmployeeId = 0;
                employee.IsActive =
                    (byte)IsActive.Active;

                await db.Employees.AddAsync(employee);
                await db.SaveChangesAsync();

                employeeCacheVersion++;

                Employee createdEmployee =
                    await GetEmployeeQuery()
                        .FirstOrDefaultAsync(x =>
                            x.EmployeeId == employee.EmployeeId);

                EmployeeDto employeeDto =
                    mapper.Map<EmployeeDto>(createdEmployee);

                ApiResponse<EmployeeDto> response =
                    ApiResponseHelper.SuccessRes(
                        employeeDto,
                        "Employee created successfully.");

                string cacheKey =
                    $"employee_{employee.EmployeeId}";

                cache.Set(
                    cacheKey,
                    response,
                    TimeSpan.FromMinutes(5));

                return response;
            }
            catch (Exception ex)
            {
                return ApiResponseHelper.Failure<EmployeeDto>(
                    "Failed to create employee.",
                    "EMPLOYEE_CREATE_ERROR",
                    ex.Message);
            }
        }

        public async Task<ApiResponse<EmployeeDto>> UpdateEmployeeAsync(
            int id,
            UpdateEmployeeDto updateEmployeeDto)
        {
            try
            {
                Employee employee = await GetEmployeeQuery()
                    .FirstOrDefaultAsync(x =>
                        x.EmployeeId == id);

                if (employee == null)
                {
                    return ApiResponseHelper.Failure<EmployeeDto>(
                        "Employee not found.",
                        "404",
                        "Invalid Employee Id.");
                }

                Employee existingEmployee =
                    await db.Employees
                        .FirstOrDefaultAsync(x =>
                            x.EmployeeCode ==
                            updateEmployeeDto.EmployeeCode &&
                            x.EmployeeId != id);

                if (existingEmployee != null)
                {
                    return ApiResponseHelper.Failure<EmployeeDto>(
                        "Employee code already exists.",
                        "409",
                        "Employee code already exists.");
                }

                bool userExists = await db.Users
                    .AnyAsync(x =>
                        x.UserId == updateEmployeeDto.UserId);

                if (!userExists)
                {
                    return ApiResponseHelper.Failure<EmployeeDto>(
                        "User not found.",
                        "404",
                        "Invalid User Id.");
                }

                bool userEmployeeExists = await db.Employees
                    .AnyAsync(x =>
                        x.UserId == updateEmployeeDto.UserId &&
                        x.EmployeeId != id);

                if (userEmployeeExists)
                {
                    return ApiResponseHelper.Failure<EmployeeDto>(
                        "User is already assigned to another employee.",
                        "409",
                        "User is already linked to another employee.");
                }

                Department department =
                    await db.Departments
                        .FirstOrDefaultAsync(x =>
                            x.DepartmentId ==
                            updateEmployeeDto.DepartmentId &&
                            x.IsActive ==
                            (byte)IsActive.Active);

                if (department == null)
                {
                    return ApiResponseHelper.Failure<EmployeeDto>(
                        "Department not found.",
                        "404",
                        "Invalid Department Id.");
                }

                bool designationExists = await db.Roles
                    .AnyAsync(x =>
                        x.RoleId ==
                        updateEmployeeDto.Designation &&
                        x.IsActive ==
                        (byte)IsActive.Active);

                if (!designationExists)
                {
                    return ApiResponseHelper.Failure<EmployeeDto>(
                        "Designation not found.",
                        "404",
                        "Invalid Designation Role Id.");
                }

                bool companyExists = await db.Companies
                    .AnyAsync(x =>
                        x.CompanyId ==
                        updateEmployeeDto.CompanyId &&
                        x.IsActive ==
                        (byte)IsActive.Active);

                if (!companyExists)
                {
                    return ApiResponseHelper.Failure<EmployeeDto>(
                        "Company not found.",
                        "404",
                        "Invalid Company Id.");
                }

                if (department.CompanyId !=
                    updateEmployeeDto.CompanyId)
                {
                    return ApiResponseHelper.Failure<EmployeeDto>(
                        "Department does not belong to the selected company.",
                        "409",
                        "Department and Company do not match.");
                }

                if (updateEmployeeDto.ReportingManager.HasValue &&
                    updateEmployeeDto.ReportingManager.Value == id)
                {
                    return ApiResponseHelper.Failure<EmployeeDto>(
                        "Employee cannot be their own reporting manager.",
                        "409",
                        "Invalid Reporting Manager Id.");
                }

                if (updateEmployeeDto.ReportingManager.HasValue)
                {
                    Employee reportingManager =
                        await db.Employees
                            .FirstOrDefaultAsync(x =>
                                x.EmployeeId ==
                                updateEmployeeDto.ReportingManager.Value &&
                                x.IsActive ==
                                (byte)IsActive.Active);

                    if (reportingManager == null)
                    {
                        return ApiResponseHelper.Failure<EmployeeDto>(
                            "Reporting manager not found.",
                            "404",
                            "Invalid Reporting Manager Id.");
                    }

                    if (reportingManager.CompanyId !=
                        updateEmployeeDto.CompanyId)
                    {
                        return ApiResponseHelper.Failure<EmployeeDto>(
                            "Reporting manager belongs to another company.",
                            "409",
                            "Reporting manager must belong to the same company.");
                    }
                }

                employee.EmployeeCode =
                    updateEmployeeDto.EmployeeCode;

                employee.UserId =
                    updateEmployeeDto.UserId;

                employee.DepartmentId =
                    updateEmployeeDto.DepartmentId;

                employee.Designation =
                    updateEmployeeDto.Designation;

                employee.JoiningDate =
                    updateEmployeeDto.JoiningDate;

                employee.CompanyId =
                    updateEmployeeDto.CompanyId;

                employee.ReportingManager =
                    updateEmployeeDto.ReportingManager;

                employee.PAN =
                    updateEmployeeDto.PAN;

                employee.IsActive =
                    (byte)IsActive.Active;

                await db.SaveChangesAsync();

                employeeCacheVersion++;

                Employee updatedEmployee =
                    await GetEmployeeQuery()
                        .FirstOrDefaultAsync(x =>
                            x.EmployeeId == id);

                EmployeeDto employeeDto =
                    mapper.Map<EmployeeDto>(updatedEmployee);

                ApiResponse<EmployeeDto> response =
                    ApiResponseHelper.SuccessRes(
                        employeeDto,
                        "Employee updated successfully.");

                string cacheKey = $"employee_{id}";

                cache.Remove(cacheKey);

                cache.Set(
                    cacheKey,
                    response,
                    TimeSpan.FromMinutes(5));

                return response;
            }
            catch (Exception ex)
            {
                return ApiResponseHelper.Failure<EmployeeDto>(
                    "Failed to update employee.",
                    "EMPLOYEE_UPDATE_ERROR",
                    ex.Message);
            }
        }

        public async Task<ApiResponse<string>> DeleteEmployeeAsync(
            int id)
        {
            try
            {
                Employee employee = await db.Employees
                    .FirstOrDefaultAsync(x =>
                        x.EmployeeId == id);

                if (employee == null)
                {
                    return ApiResponseHelper.Failure<string>(
                        "Employee not found.",
                        "404",
                        "Invalid Employee Id.");
                }

                if (employee.IsActive ==
                    (byte)IsActive.Inactive)
                {
                    return ApiResponseHelper.Failure<string>(
                        "Employee already deleted.",
                        "409",
                        "Employee is already inactive.");
                }

                employee.IsActive =
                    (byte)IsActive.Inactive;

                await db.SaveChangesAsync();

                employeeCacheVersion++;

                string cacheKey =
                    $"employee_{id}";

                cache.Remove(cacheKey);

                return ApiResponseHelper.SuccessRes(
                    "Deleted",
                    "Employee deleted successfully.");
            }
            catch (Exception ex)
            {
                return ApiResponseHelper.Failure<string>(
                    "Failed to delete employee.",
                    "EMPLOYEE_DELETE_ERROR",
                    ex.Message);
            }
        }
    }
}