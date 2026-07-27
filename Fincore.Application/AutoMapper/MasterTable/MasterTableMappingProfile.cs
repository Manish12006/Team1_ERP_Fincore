using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Fincore.Application.DTO.MasterTable;
using Fincore.Application.DTOs.MasterTable;
using Fincore.Domain.Models;

namespace Fincore.Application.AutoMapper.MasterTable
{
    public class MasterTableMappingProfile : Profile
    {
        public MasterTableMappingProfile()
        {
    
            CreateMap<CreateCompanyDto, Company>();

           
            CreateMap<UpdateCompanyDto, Company>();

            CreateMap<CountryDto, Country>();

            CreateMap<Country, CountryResponseDto>();

            CreateMap<CityDto, City>();

            CreateMap<City, CityResponseDto>()
                .ForMember(
                    dest => dest.StateName,
                    opt => opt.MapFrom(src => src.State.StateName));

            CreateMap<CurrencyDto, Currency>();

            CreateMap<Currency, CurrencyResponseDto>();

            CreateMap<StateDto, State>();

            CreateMap<State, StateResponseDto>()
                .ForMember(
                    dest => dest.CountryName,
                    opt => opt.MapFrom(src => src.Country.CountryName));



            CreateMap<Company, CompanyDto>()
                .ForMember(dest => dest.CountryName,
                    opt => opt.MapFrom(src => src.Country != null ? src.Country.CountryName : ""))

                .ForMember(dest => dest.MasterTypeName,
                    opt => opt.MapFrom(src => src.MasterType != null ? src.MasterType.MasterTypeName : ""));

            CreateMap<Department, DepartmentDTO>()
    .ForMember(dest => dest.CompanyName,
        opt => opt.MapFrom(src => src.Company.CompanyName))

    .ForMember(dest => dest.MasterTypeName,
        opt => opt.MapFrom(src => src.MasterType != null
            ? src.MasterType.MasterTypeName
            : null))

    .ForMember(dest => dest.ManagerName,
        opt => opt.MapFrom(src => src.Manager != null
            ? src.Manager.User.FullName
            : null));

            CreateMap<DepartmentDTO, Department>();


            // User Mapping
            CreateMap<User, UserDto>()
                .ForMember(dest => dest.RoleName,
                    opt => opt.MapFrom(src => src.Role != null
                        ? src.Role.RoleName
                        : null));

            CreateMap<CreateUserDto, User>()
                .ForMember(dest => dest.UserId,
                    opt => opt.Ignore())
                .ForMember(dest => dest.PasswordHash,
                    opt => opt.Ignore())
                .ForMember(dest => dest.RefreshToken,
                    opt => opt.Ignore())
                .ForMember(dest => dest.LastLogin,
                    opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt,
                    opt => opt.Ignore())
                .ForMember(dest => dest.ModifiedAt,
                    opt => opt.Ignore())
                .ForMember(dest => dest.ModifiedBy,
                    opt => opt.Ignore())
                .ForMember(dest => dest.Role,
                    opt => opt.Ignore());

            CreateMap<UpdateUserDto, User>()
                .ForMember(dest => dest.UserId,
                    opt => opt.Ignore())
                .ForMember(dest => dest.PasswordHash,
                    opt => opt.Ignore())
                .ForMember(dest => dest.RefreshToken,
                    opt => opt.Ignore())
                .ForMember(dest => dest.LastLogin,
                    opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt,
                    opt => opt.Ignore())
                .ForMember(dest => dest.CreatedBy,
                    opt => opt.Ignore())
                .ForMember(dest => dest.ModifiedAt,
                    opt => opt.Ignore())
                .ForMember(dest => dest.Role,
                    opt => opt.Ignore());


            // Permission Mapping
            CreateMap<Permission, PermissionDto>()
                .ForMember(dest => dest.RoleName,
                    opt => opt.MapFrom(src => src.Role != null
                        ? src.Role.RoleName
                        : null))
                .ForMember(dest => dest.MasterTypeName,
                    opt => opt.MapFrom(src => src.MasterType != null
                        ? src.MasterType.MasterTypeName
                        : null));


            CreateMap<CreatePermissionDto, Permission>()
                .ForMember(dest => dest.PermissionId,
                    opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt,
                    opt => opt.Ignore())
                .ForMember(dest => dest.ModifiedAt,
                    opt => opt.Ignore())
                .ForMember(dest => dest.ModifiedBy,
                    opt => opt.Ignore())
                .ForMember(dest => dest.Role,
                    opt => opt.Ignore())
                .ForMember(dest => dest.MasterType,
                    opt => opt.Ignore())
                .ForMember(dest => dest.CreatedByUser,
                    opt => opt.Ignore())
                .ForMember(dest => dest.ModifiedByUser,
                    opt => opt.Ignore());


            CreateMap<UpdatePermissionDto, Permission>()
                .ForMember(dest => dest.PermissionId,
                    opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt,
                    opt => opt.Ignore())
                .ForMember(dest => dest.CreatedBy,
                    opt => opt.Ignore())
                .ForMember(dest => dest.ModifiedAt,
                    opt => opt.Ignore())
                .ForMember(dest => dest.Role,
                    opt => opt.Ignore())
                .ForMember(dest => dest.MasterType,
                    opt => opt.Ignore())
                .ForMember(dest => dest.CreatedByUser,
                    opt => opt.Ignore())
                .ForMember(dest => dest.ModifiedByUser,
                    opt => opt.Ignore());


            // Role Mapping
            CreateMap<Role, RoleDto>();

            CreateMap<CreateRoleDto, Role>()
                .ForMember(dest => dest.RoleId,
                    opt => opt.Ignore())
                .ForMember(dest => dest.UserId,
                    opt => opt.Ignore())
                .ForMember(dest => dest.User,
                    opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt,
                    opt => opt.Ignore())
                .ForMember(dest => dest.ModifiedAt,
                    opt => opt.Ignore())
                .ForMember(dest => dest.ModifiedBy,
                    opt => opt.Ignore())
                .ForMember(dest => dest.CreatedByUser,
                    opt => opt.Ignore())
                .ForMember(dest => dest.ModifiedByUser,
                    opt => opt.Ignore())
                .ForMember(dest => dest.Users,
                    opt => opt.Ignore())
                .ForMember(dest => dest.Permissions,
                    opt => opt.Ignore())
                .ForMember(dest => dest.ApprovalFlows,
                    opt => opt.Ignore());


            CreateMap<UpdateRoleDto, Role>()
                .ForMember(dest => dest.RoleId,
                    opt => opt.Ignore())
                .ForMember(dest => dest.UserId,
                    opt => opt.Ignore())
                .ForMember(dest => dest.User,
                    opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt,
                    opt => opt.Ignore())
                .ForMember(dest => dest.CreatedBy,
                    opt => opt.Ignore())
                .ForMember(dest => dest.ModifiedAt,
                    opt => opt.Ignore())
                .ForMember(dest => dest.CreatedByUser,
                    opt => opt.Ignore())
                .ForMember(dest => dest.ModifiedByUser,
                    opt => opt.Ignore())
                .ForMember(dest => dest.Users,
                    opt => opt.Ignore())
                .ForMember(dest => dest.Permissions,
                    opt => opt.Ignore())
                .ForMember(dest => dest.ApprovalFlows,
                    opt => opt.Ignore());


            // Employee Mapping
            CreateMap<Employee, EmployeeDto>()

                .ForMember(dest => dest.UserName,
                    opt => opt.MapFrom(src =>
                        src.User != null
                            ? src.User.FullName
                            : null))

                .ForMember(dest => dest.DepartmentName,
                    opt => opt.MapFrom(src =>
                        src.Department != null
                            ? src.Department.DepartmentName
                            : null))

                .ForMember(dest => dest.DesignationName,
                    opt => opt.MapFrom(src =>
                        src.DesignationRole != null
                            ? src.DesignationRole.RoleName
                            : null))

                .ForMember(dest => dest.CompanyName,
                    opt => opt.MapFrom(src =>
                        src.Company != null
                            ? src.Company.CompanyName
                            : null))

                .ForMember(dest => dest.ReportingManagerName,
                    opt => opt.MapFrom(src =>
                        src.ReportingManagerEmployee != null &&
                        src.ReportingManagerEmployee.User != null
                            ? src.ReportingManagerEmployee.User.FullName
                            : null));


            CreateMap<CreateEmployeeDto, Employee>()
                .ForMember(dest => dest.EmployeeId,
                    opt => opt.Ignore())

                .ForMember(dest => dest.User,
                    opt => opt.Ignore())

                .ForMember(dest => dest.Department,
                    opt => opt.Ignore())

                .ForMember(dest => dest.DesignationRole,
                    opt => opt.Ignore())

                .ForMember(dest => dest.Company,
                    opt => opt.Ignore())

                .ForMember(dest => dest.ReportingManagerEmployee,
                    opt => opt.Ignore())

                .ForMember(dest => dest.Subordinates,
                    opt => opt.Ignore())

                .ForMember(dest => dest.RFQsCreated,
                    opt => opt.Ignore())

                .ForMember(dest => dest.GRNsCreated,
                    opt => opt.Ignore());


            CreateMap<UpdateEmployeeDto, Employee>()
                .ForMember(dest => dest.EmployeeId,
                    opt => opt.Ignore())

                .ForMember(dest => dest.User,
                    opt => opt.Ignore())

                .ForMember(dest => dest.Department,
                    opt => opt.Ignore())

                .ForMember(dest => dest.DesignationRole,
                    opt => opt.Ignore())

                .ForMember(dest => dest.Company,
                    opt => opt.Ignore())

                .ForMember(dest => dest.ReportingManagerEmployee,
                    opt => opt.Ignore())

                .ForMember(dest => dest.Subordinates,
                    opt => opt.Ignore())

                .ForMember(dest => dest.RFQsCreated,
                    opt => opt.Ignore())

                .ForMember(dest => dest.GRNsCreated,
                    opt => opt.Ignore());



            // Customer Mapping
            CreateMap<Customer, CustomerDto>()

                .ForMember(dest => dest.UserName,
                    opt => opt.MapFrom(src =>
                        src.User != null
                            ? src.User.FullName
                            : null))

                .ForMember(dest => dest.CompanyName,
                    opt => opt.MapFrom(src =>
                        src.Company != null
                            ? src.Company.CompanyName
                            : null));


            CreateMap<CreateCustomerDto, Customer>()
                .ForMember(dest => dest.CustomerId,
                    opt => opt.Ignore())

                .ForMember(dest => dest.User,
                    opt => opt.Ignore())

                .ForMember(dest => dest.Company,
                    opt => opt.Ignore())

                .ForMember(dest => dest.RevenueEntries,
                    opt => opt.Ignore())

                .ForMember(dest => dest.ARInvoices,
                    opt => opt.Ignore())

                .ForMember(dest => dest.Payments,
                    opt => opt.Ignore());


            CreateMap<UpdateCustomerDto, Customer>()
                .ForMember(dest => dest.CustomerId,
                    opt => opt.Ignore())

                .ForMember(dest => dest.User,
                    opt => opt.Ignore())

                .ForMember(dest => dest.Company,
                    opt => opt.Ignore())

                .ForMember(dest => dest.RevenueEntries,
                    opt => opt.Ignore())

                .ForMember(dest => dest.ARInvoices,
                    opt => opt.Ignore())

                .ForMember(dest => dest.Payments,
                    opt => opt.Ignore());


            // Vendor Mapping
            CreateMap<Vendor, VendorDto>()

                .ForMember(dest => dest.VendorCategoryName,
                    opt => opt.MapFrom(src =>
                        src.VendorCategory != null
                            ? src.VendorCategory.CategoryName
                            : null))

                .ForMember(dest => dest.CompanyName,
                    opt => opt.MapFrom(src =>
                        src.Company != null
                            ? src.Company.CompanyName
                            : null))

                .ForMember(dest => dest.CreatedByName,
                    opt => opt.MapFrom(src =>
                        src.CreatedByUser != null
                            ? src.CreatedByUser.FullName
                            : null))

                .ForMember(dest => dest.ModifiedByName,
                    opt => opt.MapFrom(src =>
                        src.ModifiedByUser != null
                            ? src.ModifiedByUser.FullName
                            : null));


            CreateMap<CreateVendorDto, Vendor>()
                .ForMember(dest => dest.VendorId,
                    opt => opt.Ignore())

                .ForMember(dest => dest.CreatedAt,
                    opt => opt.Ignore())

                .ForMember(dest => dest.ModifiedAt,
                    opt => opt.Ignore())

                .ForMember(dest => dest.ModifiedBy,
                    opt => opt.Ignore())

                .ForMember(dest => dest.VendorCategory,
                    opt => opt.Ignore())

                .ForMember(dest => dest.Company,
                    opt => opt.Ignore())

                .ForMember(dest => dest.CreatedByUser,
                    opt => opt.Ignore())

                .ForMember(dest => dest.ModifiedByUser,
                    opt => opt.Ignore())

                .ForMember(dest => dest.PurchaseRequisitions,
                    opt => opt.Ignore())

                .ForMember(dest => dest.RFQs,
                    opt => opt.Ignore())

                .ForMember(dest => dest.RFQVendors,
                    opt => opt.Ignore())

                .ForMember(dest => dest.Quotations,
                    opt => opt.Ignore())

                .ForMember(dest => dest.VendorSelections,
                    opt => opt.Ignore())

                .ForMember(dest => dest.GRNs,
                    opt => opt.Ignore())

                .ForMember(dest => dest.Assets,
                    opt => opt.Ignore())

                .ForMember(dest => dest.WorkOrders,
                    opt => opt.Ignore())

                .ForMember(dest => dest.APInvoices,
                    opt => opt.Ignore())

                .ForMember(dest => dest.Payments,
                    opt => opt.Ignore());


            CreateMap<UpdateVendorDto, Vendor>()
                .ForMember(dest => dest.VendorId,
                    opt => opt.Ignore())

                .ForMember(dest => dest.CreatedAt,
                    opt => opt.Ignore())

                .ForMember(dest => dest.CreatedBy,
                    opt => opt.Ignore())

                .ForMember(dest => dest.ModifiedAt,
                    opt => opt.Ignore())

                .ForMember(dest => dest.VendorCategory,
                    opt => opt.Ignore())

                .ForMember(dest => dest.Company,
                    opt => opt.Ignore())

                .ForMember(dest => dest.CreatedByUser,
                    opt => opt.Ignore())

                .ForMember(dest => dest.ModifiedByUser,
                    opt => opt.Ignore())

                .ForMember(dest => dest.PurchaseRequisitions,
                    opt => opt.Ignore())

                .ForMember(dest => dest.RFQs,
                    opt => opt.Ignore())

                .ForMember(dest => dest.RFQVendors,
                    opt => opt.Ignore())

                .ForMember(dest => dest.Quotations,
                    opt => opt.Ignore())

                .ForMember(dest => dest.VendorSelections,
                    opt => opt.Ignore())

                .ForMember(dest => dest.GRNs,
                    opt => opt.Ignore())

                .ForMember(dest => dest.Assets,
                    opt => opt.Ignore())

                .ForMember(dest => dest.WorkOrders,
                    opt => opt.Ignore())

                .ForMember(dest => dest.APInvoices,
                    opt => opt.Ignore())

                .ForMember(dest => dest.Payments,
                    opt => opt.Ignore());



            // VendorCategory Mapping
            CreateMap<CreateVendorCategoryDto, VendorCategory>();

            CreateMap<UpdateVendorCategoryDto, VendorCategory>();

            CreateMap<VendorCategory, VendorCategoryDto>()
                .ForMember(
                    dest => dest.CreatedByName,
                    opt => opt.MapFrom(src =>
                        src.CreatedByUser != null
                            ? src.CreatedByUser.FullName
                            : null))
                .ForMember(
                    dest => dest.ModifiedByName,
                    opt => opt.MapFrom(src =>
                        src.ModifiedByUser != null
                            ? src.ModifiedByUser.FullName
                            : null));


        }
    }
}
