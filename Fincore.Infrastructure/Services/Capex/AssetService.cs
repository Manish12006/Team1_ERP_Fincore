using AutoMapper;
using Fincore.Application.DTO;
using Fincore.Application.DTO.Capex;
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
        private readonly IMemoryCache cache;

        private const string AssetCacheKey = "Asset";


        public AssetService(
            AppDbContext db,
            IMapper mapper,
            IMemoryCache cache)
        {
            this.db = db;
            this.mapper = mapper;
            this.cache = cache;
        }


        private void ClearAssetCache()
        {
            for (int page = 1; page <= 50; page++)
            {
                for (int size = 10; size <= 100; size += 10)
                {
                    cache.Remove($"{AssetCacheKey}_{page}_{size}");
                }
            }

            ClearAssetCache();
        }



        public async Task<ApiResponse<AssetDTO>> AddAsset(AssetDTO dto)
        {

            if (dto == null)
            {
                return ApiResponseHelper.Failure<AssetDTO>(
                    "Invalid Request",
                    "400",
                    "Asset data is required");
            }


            if (string.IsNullOrWhiteSpace(dto.AssetCode))
            {
                return ApiResponseHelper.Failure<AssetDTO>(
                    "Invalid Asset Code",
                    "400",
                    "Asset Code is required");
            }


            if (string.IsNullOrWhiteSpace(dto.AssetName))
            {
                return ApiResponseHelper.Failure<AssetDTO>(
                    "Invalid Asset Name",
                    "400",
                    "Asset Name is required");
            }


            if (dto.PurchaseCost <= 0)
            {
                return ApiResponseHelper.Failure<AssetDTO>(
                    "Invalid Purchase Cost",
                    "400",
                    "Purchase cost must be greater than zero");
            }



            var exists = await db.Assets
                .AnyAsync(x => x.AssetCode == dto.AssetCode);



            if (exists)
            {
                return ApiResponseHelper.Failure<AssetDTO>(
                    "Asset Code Already Exists",
                    "400",
                    "Duplicate Asset Code");
            }



            if (dto.GRNId != null)
            {
                var grnExists = await db.GRNs
                    .AnyAsync(x => x.GRNId == dto.GRNId);


                if (!grnExists)
                {
                    return ApiResponseHelper.Failure<AssetDTO>(
                        "GRN Not Found",
                        "404",
                        "Invalid GRN");
                }
            }



            if (dto.VendorId != null)
            {
                var vendorExists = await db.Vendors
                    .AnyAsync(x =>
                    x.VendorId == dto.VendorId &&
                    x.IsActive == 1);


                if (!vendorExists)
                {
                    return ApiResponseHelper.Failure<AssetDTO>(
                    "Vendor Not Found",
                    "404",
                    "Invalid Vendor");
                }
            }



            if (dto.DepartmentId != null)
            {
                var deptExists = await db.Departments
                    .AnyAsync(x =>
                    x.DepartmentId == dto.DepartmentId);


                if (!deptExists)
                {
                    return ApiResponseHelper.Failure<AssetDTO>(
                    "Department Not Found",
                    "404",
                    "Invalid Department");
                }
            }



            var asset = mapper.Map<Asset>(dto);


            asset.Status = "AVAILABLE";
            asset.CreatedAt = DateTime.Now;



            await db.Assets.AddAsync(asset);


            await db.SaveChangesAsync();



            ClearAssetCache();



            return ApiResponseHelper.SuccessRes(
                mapper.Map<AssetDTO>(asset),
                "Asset Created Successfully");
        }




        public async Task<ApiResponse<AssetDTO>> GetAsset(int id)
        {

            if (id <= 0)
            {
                return ApiResponseHelper.Failure<AssetDTO>(
                    "Invalid Asset Id",
                    "400",
                    "Asset Id must be greater than zero");
            }



            var data = await db.Assets
                .FirstOrDefaultAsync(x => x.AssetId == id);



            if (data == null)
            {
                return ApiResponseHelper.Failure<AssetDTO>(
                    "Asset Not Found",
                    "404",
                    "Record not found");
            }



            if (data.Status == "DELETED")
            {
                return ApiResponseHelper.Failure<AssetDTO>(
                    "Asset Already Deleted",
                    "400",
                    "Record is already deleted");
            }



            return ApiResponseHelper.SuccessRes(
                mapper.Map<AssetDTO>(data),
                "Asset Retrieved Successfully");
        }





        public async Task<ApiResponse<List<AssetDTO>>> GetAllAssets(
            int page,
            int pageSize)
        {

            if (page <= 0)
                page = 1;


            if (pageSize <= 0)
                pageSize = 10;



            string cacheKey = $"{AssetCacheKey}_{page}_{pageSize}";



            if (cache.TryGetValue(cacheKey, out List<AssetDTO> cachedAssets))
            {

                var total = await db.Assets
                    .Where(x => x.Status != "DELETED")
                    .CountAsync();


                return ApiResponseHelper.SuccessRes(
                    cachedAssets,
                    "Assets Retrieved Successfully",
                    total,
                    new
                    {
                        page,
                        pageSize
                    });
            }




            var query = db.Assets
                .Where(x => x.Status != "DELETED");



            var totalRecords = await query.CountAsync();



            var data = await query
                .OrderByDescending(x => x.AssetId)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();



            if (!data.Any())
            {
                return ApiResponseHelper.Failure<List<AssetDTO>>(
                    "Assets Not Found",
                    "404",
                    "No asset records found");
            }



            var assets = mapper.Map<List<AssetDTO>>(data);



            cache.Set(
                cacheKey,
                assets,
                TimeSpan.FromMinutes(5));



            return ApiResponseHelper.SuccessRes(assets,"Assets Retrieved Successfully",
                totalRecords,new{page,pageSize});
        }
        public async Task<ApiResponse<AssetDTO>> UpdateAsset(int id,AssetDTO dto)
        {

            if (dto == null)
            {
                return ApiResponseHelper.Failure<AssetDTO>(
                    "Invalid Request",
                    "400",
                    "Asset data is required");
            }

           

            var asset = await db.Assets.FirstOrDefaultAsync(x => x.AssetId == id);



            if (asset == null)
            {
                return ApiResponseHelper.Failure<AssetDTO>(
                    "Asset Not Found",
                    "404",
                    "Record not found");
            }

            if (asset.Status == "DISPOSED")
            {
                return ApiResponseHelper.Failure<AssetDTO>(
                "Cannot Update",
                "400",
                "Disposed asset cannot be updated");
            }

            if (asset.Status == "DELETED")
            {
                return ApiResponseHelper.Failure<AssetDTO>(
                    "Asset Already Deleted",
                    "400",
                    "Record is already deleted");
            }



            if (dto.PurchaseCost <= 0)
            {
                return ApiResponseHelper.Failure<AssetDTO>(
                    "Invalid Purchase Cost",
                    "400",
                    "Purchase cost must be greater than zero");
            }



            asset.AssetName = dto.AssetName;
            asset.PurchaseCost = dto.PurchaseCost;
            asset.PurchaseDate = dto.PurchaseDate;
            asset.DepartmentId = dto.DepartmentId;
            asset.ModifiedAt = DateTime.Now;



            await db.SaveChangesAsync();



            ClearAssetCache();



            return ApiResponseHelper.SuccessRes(
                mapper.Map<AssetDTO>(asset),
                "Asset Updated Successfully");
        }





        public async Task<ApiResponse<AssetDTO>> DeleteAsset(int id)
        {

            var asset = await db.Assets
                .FirstOrDefaultAsync(x => x.AssetId == id);



            if (asset == null)
            {
                return ApiResponseHelper.Failure<AssetDTO>(
                    "Asset Not Found",
                    "404",
                    "Record not found");
            }



            if (asset.Status == "DELETED")
            {
                return ApiResponseHelper.Failure<AssetDTO>(
                    "Asset Already Deleted",
                    "400",
                    "Record is already deleted");
            }



            if (asset.Status == "ASSIGNED")
            {
                return ApiResponseHelper.Failure<AssetDTO>(
                    "Cannot Delete",
                    "400",
                    "Assigned Asset cannot be deleted");
            }



            asset.Status = "DELETED";
            asset.ModifiedAt = DateTime.Now;



            await db.SaveChangesAsync();



            ClearAssetCache();



            return ApiResponseHelper.SuccessRes(
                mapper.Map<AssetDTO>(asset),
                "Asset Deleted Successfully");
        }





        public async Task<ApiResponse<AssetDTO>> AssignAsset(int id, int userId)
        {

            var asset = await db.Assets
            .FirstOrDefaultAsync(x => x.AssetId == id);


            if (asset == null)
                return ApiResponseHelper.Failure<AssetDTO>(
                "Asset Not Found", "404", "Invalid Asset");


            if (asset.Status == "DISPOSED")
                return ApiResponseHelper.Failure<AssetDTO>(
                "Cannot Assign", "400",
                "Disposed asset cannot be assigned");


            if (asset.Status == "ASSIGNED")
                return ApiResponseHelper.Failure<AssetDTO>(
                "Already Assigned", "400",
                "Asset already assigned");


            var userExists = await db.Users
            .AnyAsync(x => x.UserId == userId);


            if (!userExists)
                return ApiResponseHelper.Failure<AssetDTO>(
                "User Not Found", "404",
                "Invalid User");


            asset.Status = "ASSIGNED";
            asset.ModifiedAt = DateTime.Now;


            await db.SaveChangesAsync();

            ClearAssetCache();


            return ApiResponseHelper.SuccessRes(
            mapper.Map<AssetDTO>(asset),
            "Asset Assigned Successfully");

        }





        public async Task<ApiResponse<AssetDTO>> TransferAsset(int id,int departmentId)
        {

            var asset = await db.Assets.FirstOrDefaultAsync(x => x.AssetId == id);


            if (asset == null)
            {
                return ApiResponseHelper.Failure<AssetDTO>(
                "Asset Not Found",
                "404",
                "Invalid Asset");
            }


            if (asset.Status != "ASSIGNED")
            {
                return ApiResponseHelper.Failure<AssetDTO>(
                "Cannot Transfer",
                "400",
                "Only assigned asset can be transferred");
            }



            if (asset.Status == "DELETED")
            {
                return ApiResponseHelper.Failure<AssetDTO>(
                    "Asset Already Deleted",
                    "400",
                    "Deleted asset cannot be transferred");
            }



            var deptExists = await db.Departments
                .AnyAsync(x => x.DepartmentId == departmentId);



            if (!deptExists)
            {
                return ApiResponseHelper.Failure<AssetDTO>(
                    "Department Not Found",
                    "404",
                    "Invalid Department");
            }



            asset.DepartmentId = departmentId;
            asset.ModifiedAt = DateTime.Now;



            await db.SaveChangesAsync();



            return ApiResponseHelper.SuccessRes(
                mapper.Map<AssetDTO>(asset),
                "Asset Transferred Successfully");
        }





        public async Task<ApiResponse<AssetDTO>> DisposeAsset(int id)
        {

            var asset = await db.Assets
                .FirstOrDefaultAsync(x => x.AssetId == id);

            if (asset.Status == "DISPOSED")
            {
                return ApiResponseHelper.Failure<AssetDTO>(
                "Already Disposed",
                "400",
                "Asset already disposed");
            }

            if (asset == null)
            {
                return ApiResponseHelper.Failure<AssetDTO>(
                    "Asset Not Found",
                    "404",
                    "Invalid Asset");
            }



            if (asset.Status == "DELETED")
            {
                return ApiResponseHelper.Failure<AssetDTO>(
                    "Asset Already Deleted",
                    "400",
                    "Record is already deleted");
            }



            asset.Status = "DISPOSED";
            asset.ModifiedAt = DateTime.Now;



            await db.SaveChangesAsync();


            ClearAssetCache();
            return ApiResponseHelper.SuccessRes(
                mapper.Map<AssetDTO>(asset),
                "Asset Disposed Successfully");
        }





        public async Task<ApiResponse<AssetDTO>> RepairAsset(int id)
        {

            var asset = await db.Assets
                .FirstOrDefaultAsync(x => x.AssetId == id);

            if (asset.Status != "ASSIGNED")
            {
                return ApiResponseHelper.Failure<AssetDTO>(
                "Cannot Return",
                "400",
                "Only assigned asset can be returned");
            }

            if (asset == null)
            {
                return ApiResponseHelper.Failure<AssetDTO>(
                    "Asset Not Found",
                    "404",
                    "Invalid Asset");
            }



            if (asset.Status == "DELETED")
            {
                return ApiResponseHelper.Failure<AssetDTO>(
                    "Asset Already Deleted",
                    "400",
                    "Record is already deleted");
            }



            asset.Status = "REPAIR";
            asset.ModifiedAt = DateTime.Now;



            await db.SaveChangesAsync();



            return ApiResponseHelper.SuccessRes(
                mapper.Map<AssetDTO>(asset),
                "Asset Sent For Repair");
        }





        public async Task<ApiResponse<AssetDTO>> ReturnAsset(int id)
        {

            var asset = await db.Assets
                .FirstOrDefaultAsync(x => x.AssetId == id);



            if (asset == null)
            {
                return ApiResponseHelper.Failure<AssetDTO>(
                    "Asset Not Found",
                    "404",
                    "Invalid Asset");
            }



            if (asset.Status == "DELETED")
            {
                return ApiResponseHelper.Failure<AssetDTO>(
                    "Asset Already Deleted",
                    "400",
                    "Record is already deleted");
            }



            asset.Status = "AVAILABLE";
            asset.ModifiedAt = DateTime.Now;



            await db.SaveChangesAsync();



            ClearAssetCache();



            return ApiResponseHelper.SuccessRes(
                mapper.Map<AssetDTO>(asset),
                "Asset Returned Successfully");
        }
        public async Task<ApiResponse<List<AssetDTO>>> GetAssetByStatus(string status)
        {

            if (string.IsNullOrWhiteSpace(status))
            {
                return ApiResponseHelper.Failure<List<AssetDTO>>(
                    "Invalid Status",
                    "400",
                    "Status is required");
            }



            var data = await db.Assets
                .Where(x =>
                    x.Status == status &&
                    x.Status != "DELETED")
                .OrderByDescending(x => x.AssetId)
                .ToListAsync();



            if (!data.Any())
            {
                return ApiResponseHelper.Failure<List<AssetDTO>>(
                    "Asset Not Found",
                    "404",
                    "No asset found with this status");
            }



            return ApiResponseHelper.SuccessRes(
                mapper.Map<List<AssetDTO>>(data),
                "Asset Retrieved Successfully");
        }
        public async Task<ApiResponse<object>> GetAssetDropdown()
        {

            var purchaseOrders = await db.PurchaseOrders
                .Where(x => x.IsActive == 1)
                .Select(x => new
                {
                    id = x.POId,
                    code = x.POCode
                })
                .ToListAsync();



            var grns = await db.GRNs
                .Where(x => x.IsActive == 1)
                .Select(x => new
                {
                    id = x.GRNId,
                    code = x.GRNCode
                })
                .ToListAsync();




            var vendors = await db.Vendors
                .Where(x => x.IsActive == 1)
                .Select(x => new
                {
                    id = x.VendorId,
                    code = x.VendorCode
                })
                .ToListAsync();




            var departments = await db.Departments
                .Select(x => new
                {
                    id = x.DepartmentId,
                    code = x.DepartmentName
                })
                .ToListAsync();




            object result = new
            {
                purchaseOrders,
                grns,
                vendors,
                departments
            };



            return ApiResponseHelper.SuccessRes<object>(
                result,
                "Asset Dropdown Retrieved Successfully");
        }

        public async Task<ApiResponse<object>> GetVendorDropdown()
        {
            var vendors = await db.Vendors
                .Where(x => x.IsActive == 1)
                .Select(x => new
                {
                    id = x.VendorId,
                    code = x.VendorCode
                })
                .ToListAsync();


            return ApiResponseHelper.SuccessRes<object>(
                vendors,
                "Vendor Dropdown Retrieved Successfully"
            );
        }

        public async Task<ApiResponse<object>> GetDepartmentDropdown()
        {
            var departments = await db.Departments
                .Select(x => new
                {
                    id = x.DepartmentId,
                    code = x.DepartmentName
                })
                .ToListAsync();


            return ApiResponseHelper.SuccessRes<object>(
                departments,
                "Department Dropdown Retrieved Successfully"
            );
        }

        public async Task<ApiResponse<object>> GetGRNDropdown()
        {
            var grns = await db.GRNs
                .Where(x => x.IsActive == 1)
                .Select(x => new
                {
                    id = x.GRNId,
                    code = x.GRNCode
                })
                .ToListAsync();


            return ApiResponseHelper.SuccessRes<object>(
                grns,
                "GRN Dropdown Retrieved Successfully"
            );
        }

        public async Task<ApiResponse<object>> GetPurchaseOrderDropdown()
        {
            var purchaseOrders = await db.PurchaseOrders
                .Where(x => x.IsActive == 1)
                .Select(x => new
                {
                    id = x.POId,
                    code = x.POCode
                })
                .ToListAsync();


            return ApiResponseHelper.SuccessRes<object>(
                purchaseOrders,
                "Purchase Order Dropdown Retrieved Successfully"
            );
        }
    }
}