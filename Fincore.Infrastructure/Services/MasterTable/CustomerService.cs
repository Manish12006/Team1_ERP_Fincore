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
    public class CustomerService : ICustomerService
    {
        private readonly AppDbContext db;
        private readonly IMapper mapper;
        private readonly IMemoryCache cache;

        private static int customerCacheVersion = 1;

        public CustomerService(
            AppDbContext db,
            IMapper mapper,
            IMemoryCache cache)
        {
            this.db = db;
            this.mapper = mapper;
            this.cache = cache;
        }

        private IQueryable<Customer> GetCustomerQuery()
        {
            return db.Customers
                .Where(x => x.IsActive == (byte)IsActive.Active)
                .Include(x => x.User)
                .Include(x => x.Company);
        }

        public async Task<ApiResponse<List<CustomerDto>>> GetAllCustomersAsync(
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
                    $"customers_{customerCacheVersion}_page_{pageNumber}_size_{pageSize}_search_{search}";

                if (cache.TryGetValue(
                    cacheKey,
                    out ApiResponse<List<CustomerDto>> cachedData))
                {
                    Console.WriteLine(
                        "GET ALL CUSTOMERS - Data returned from CACHE");

                    return cachedData;
                }

                Console.WriteLine(
                    "GET ALL CUSTOMERS - Data returned from DATABASE");

                IQueryable<Customer> query = GetCustomerQuery();

                if (!string.IsNullOrEmpty(search))
                {
                    query = query.Where(x =>
                        x.CustomerCode.Contains(search) ||
                        x.User.FullName.Contains(search) ||
                        x.Company.CompanyName.Contains(search));
                }

                int totalRecords = await query.CountAsync();

                List<Customer> customers = await query
                    .OrderBy(x => x.CustomerId)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                List<CustomerDto> customerDtos =
                    mapper.Map<List<CustomerDto>>(customers);

                var metadata = new
                {
                    CurrentPage = pageNumber,
                    PageSize = pageSize,
                    TotalPages = (int)Math.Ceiling(
                        (double)totalRecords / pageSize)
                };

                ApiResponse<List<CustomerDto>> response =
                    ApiResponseHelper.SuccessRes(
                        customerDtos,
                        "Customers fetched successfully.",
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
                return ApiResponseHelper.Failure<List<CustomerDto>>(
                    "Failed to retrieve customers.",
                    "CUSTOMER_GET_ERROR",
                    ex.Message);
            }
        }

        public async Task<ApiResponse<CustomerDto>> GetCustomerByIdAsync(
            int id)
        {
            try
            {
                string cacheKey = $"customer_{id}";

                if (cache.TryGetValue(
                    cacheKey,
                    out ApiResponse<CustomerDto> cachedData))
                {
                    Console.WriteLine(
                        "GET CUSTOMER BY ID - Data returned from CACHE");

                    return cachedData;
                }

                Console.WriteLine(
                    "GET CUSTOMER BY ID - Data returned from DATABASE");

                Customer customer = await GetCustomerQuery()
                    .FirstOrDefaultAsync(x => x.CustomerId == id);

                if (customer == null)
                {
                    return ApiResponseHelper.Failure<CustomerDto>(
                        "Customer not found.",
                        "CUSTOMER_NOT_FOUND",
                        $"Customer with ID {id} does not exist.");
                }

                CustomerDto customerDto =
                    mapper.Map<CustomerDto>(customer);

                ApiResponse<CustomerDto> response =
                    ApiResponseHelper.SuccessRes(
                        customerDto,
                        "Customer fetched successfully.");

                cache.Set(
                    cacheKey,
                    response,
                    TimeSpan.FromMinutes(5));

                return response;
            }
            catch (Exception ex)
            {
                return ApiResponseHelper.Failure<CustomerDto>(
                    "Failed to retrieve customer.",
                    "CUSTOMER_GET_ERROR",
                    ex.Message);
            }
        }

        public async Task<ApiResponse<CustomerDto>> CreateCustomerAsync(
            CreateCustomerDto createCustomerDto)
        {
            try
            {
                bool userExists = await db.Users
                    .AnyAsync(x =>
                        x.UserId == createCustomerDto.UserId);

                if (!userExists)
                {
                    return ApiResponseHelper.Failure<CustomerDto>(
                        "User not found.",
                        "USER_NOT_FOUND",
                        $"User with ID {createCustomerDto.UserId} does not exist.");
                }

                bool companyExists = await db.Companies
                    .AnyAsync(x =>
                        x.CompanyId == createCustomerDto.CompanyId &&
                        x.IsActive == (byte)IsActive.Active);

                if (!companyExists)
                {
                    return ApiResponseHelper.Failure<CustomerDto>(
                        "Company not found.",
                        "COMPANY_NOT_FOUND",
                        $"Active company with ID {createCustomerDto.CompanyId} does not exist.");
                }

                bool customerCodeExists = await db.Customers
                    .AnyAsync(x =>
                        x.CustomerCode == createCustomerDto.CustomerCode);

                if (customerCodeExists)
                {
                    return ApiResponseHelper.Failure<CustomerDto>(
                        "Customer code already exists.",
                        "DUPLICATE_CUSTOMER_CODE",
                        $"Customer with code {createCustomerDto.CustomerCode} already exists.");
                }

                Customer customer =
                    mapper.Map<Customer>(createCustomerDto);

                customer.CustomerId = 0;
                customer.IsActive = (byte)IsActive.Active;

                await db.Customers.AddAsync(customer);
                await db.SaveChangesAsync();

                customerCacheVersion++;

                Customer createdCustomer = await GetCustomerQuery()
                    .FirstOrDefaultAsync(
                        x => x.CustomerId == customer.CustomerId);

                CustomerDto result =
                    mapper.Map<CustomerDto>(createdCustomer);

                ApiResponse<CustomerDto> response =
                    ApiResponseHelper.SuccessRes(
                        result,
                        "Customer created successfully.");

                string cacheKey =
                    $"customer_{customer.CustomerId}";

                cache.Set(
                    cacheKey,
                    response,
                    TimeSpan.FromMinutes(5));

                return response;
            }
            catch (Exception ex)
            {
                return ApiResponseHelper.Failure<CustomerDto>(
                    "Failed to create customer.",
                    "CUSTOMER_CREATE_ERROR",
                    ex.Message);
            }
        }

        public async Task<ApiResponse<CustomerDto>> UpdateCustomerAsync(
            int id,
            UpdateCustomerDto updateCustomerDto)
        {
            try
            {
                Customer customer = await GetCustomerQuery()
                    .FirstOrDefaultAsync(
                        x => x.CustomerId == id);

                if (customer == null)
                {
                    return ApiResponseHelper.Failure<CustomerDto>(
                        "Customer not found.",
                        "CUSTOMER_NOT_FOUND",
                        $"Active customer with ID {id} does not exist.");
                }

                bool userExists = await db.Users
                    .AnyAsync(x =>
                        x.UserId == updateCustomerDto.UserId);

                if (!userExists)
                {
                    return ApiResponseHelper.Failure<CustomerDto>(
                        "User not found.",
                        "USER_NOT_FOUND",
                        $"User with ID {updateCustomerDto.UserId} does not exist.");
                }

                bool companyExists = await db.Companies
                    .AnyAsync(x =>
                        x.CompanyId == updateCustomerDto.CompanyId &&
                        x.IsActive == (byte)IsActive.Active);

                if (!companyExists)
                {
                    return ApiResponseHelper.Failure<CustomerDto>(
                        "Company not found.",
                        "COMPANY_NOT_FOUND",
                        $"Active company with ID {updateCustomerDto.CompanyId} does not exist.");
                }

                bool customerCodeExists = await db.Customers
                    .AnyAsync(x =>
                        x.CustomerCode == updateCustomerDto.CustomerCode &&
                        x.CustomerId != id);

                if (customerCodeExists)
                {
                    return ApiResponseHelper.Failure<CustomerDto>(
                        "Customer code already exists.",
                        "DUPLICATE_CUSTOMER_CODE",
                        $"Customer with code {updateCustomerDto.CustomerCode} already exists.");
                }

                customer.CustomerCode =
                    updateCustomerDto.CustomerCode;

                customer.UserId =
                    updateCustomerDto.UserId;

                customer.CompanyId =
                    updateCustomerDto.CompanyId;

                customer.IsActive =
                    (byte)IsActive.Active;

                await db.SaveChangesAsync();

                customerCacheVersion++;

                Customer updatedCustomer = await GetCustomerQuery()
                    .FirstOrDefaultAsync(
                        x => x.CustomerId == id);

                CustomerDto result =
                    mapper.Map<CustomerDto>(updatedCustomer);

                ApiResponse<CustomerDto> response =
                    ApiResponseHelper.SuccessRes(
                        result,
                        "Customer updated successfully.");

                string cacheKey = $"customer_{id}";

                cache.Remove(cacheKey);

                cache.Set(
                    cacheKey,
                    response,
                    TimeSpan.FromMinutes(5));

                return response;
            }
            catch (Exception ex)
            {
                return ApiResponseHelper.Failure<CustomerDto>(
                    "Failed to update customer.",
                    "CUSTOMER_UPDATE_ERROR",
                    ex.Message);
            }
        }

        public async Task<ApiResponse<bool>> DeleteCustomerAsync(int id)
        {
            try
            {
                Customer customer = await db.Customers
                    .FirstOrDefaultAsync(
                        x => x.CustomerId == id);

                if (customer == null)
                {
                    return ApiResponseHelper.Failure<bool>(
                        "Customer not found.",
                        "CUSTOMER_NOT_FOUND",
                        $"Customer with ID {id} does not exist.");
                }

                if (customer.IsActive ==
                    (byte)IsActive.Inactive)
                {
                    return ApiResponseHelper.Failure<bool>(
                        "Customer already deleted.",
                        "CUSTOMER_ALREADY_DELETED",
                        "Customer is already inactive.");
                }

                customer.IsActive =
                    (byte)IsActive.Inactive;

                await db.SaveChangesAsync();

                customerCacheVersion++;

                string cacheKey =
                    $"customer_{id}";

                cache.Remove(cacheKey);

                return ApiResponseHelper.SuccessRes(
                    true,
                    "Customer deleted successfully.");
            }
            catch (Exception ex)
            {
                return ApiResponseHelper.Failure<bool>(
                    "Failed to delete customer.",
                    "CUSTOMER_DELETE_ERROR",
                    ex.Message);
            }
        }
    }
}