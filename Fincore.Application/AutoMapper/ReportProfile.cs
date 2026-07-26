using AutoMapper;
using Fincore.Application.DTO.Reports;
using Fincore.Domain.Models;



namespace Fincore.Application.AutoMapper
{
     public  class ReportProfile : Profile
    {
        public ReportProfile()
        {
            // Revenue
            CreateMap<RevenueEntry, RevenueReportDTO>()
            .ForMember(dest => dest.CustomerCode, opt => opt.MapFrom(src => src.Customer.CustomerCode))
            .ForMember(dest => dest.DepartmentName, opt => opt.MapFrom(src => src.Department.DepartmentName))
            .ForMember(dest => dest.DepartmentCode, opt => opt.MapFrom(src => src.Department.DepartmentCode))
            .ForMember(dest => dest.AccountCode,opt => opt.MapFrom(src => src.AccountMaster.AccountCode))
            .ForMember(dest => dest.AccountName,opt => opt.MapFrom(src => src.AccountMaster.AccountName));

            // Expense
            CreateMap<ExpenseClaim, ExpenseReportDTO>()
            .ForMember(dest => dest.OpexTitle,opt => opt.MapFrom(src => src.OpexRequest.Title))
            .ForMember(dest => dest.ClaimedBy,opt => opt.MapFrom(src => src.ClaimByUser.FullName))
            .ForMember(dest => dest.Department,opt => opt.MapFrom(src => src.OpexRequest.BudgetLine.BudgetCategory.Department.DepartmentName));

            // Vendpr
            CreateMap<Fincore.Domain.Models.Payment, VendorSpendDTO>()
                 .ForMember(d => d.PaymentId,
                     o => o.MapFrom(s => s.PaymentId))
                 .ForMember(d => d.PaymentNumber,
                     o => o.MapFrom(s => s.PaymentNumber))
                 .ForMember(d => d.Amount,
                     o => o.MapFrom(s => s.Amount))
                 .ForMember(d => d.PaymentDate,
                     o => o.MapFrom(s => s.PaymentDate))
                 .ForMember(d => d.PaymentMethod,
                     o => o.MapFrom(s => s.PaymentMethod))
                 .ForMember(d => d.VendorCode,
                     o => o.MapFrom(s => s.Vendor.VendorCode))
                 .ForMember(d => d.CompanyName,
                     o => o.MapFrom(s => s.Vendor.Company.CompanyName))
                 .ForMember(d => d.APInvoiceNumber,
                     o => o.MapFrom(s => s.APInvoice.InvoiceNumber))
                 .ForMember(d => d.InvoiceDate,
                     o => o.MapFrom(s => s.APInvoice.InvoiceDate))
                 .ForMember(d => d.DueDate,
                     o => o.MapFrom(s => s.APInvoice.DueDate))
                 .ForMember(d => d.PaymentStatus,
                     o => o.MapFrom(s => s.APInvoice.PaymentStatus))
                 .ForMember(d => d.ApprovalStatus,
                     o => o.MapFrom(s => s.ApprovalStatus));

            //Capex
            CreateMap<CapexRequest, CapexReportDTO>()
           .ForMember(dest => dest.DepartmentName,opt => opt.MapFrom(src => src.Department.DepartmentName))
           .ForMember(dest => dest.RequestedBy,opt => opt.MapFrom(src => src.RequestedByUser.FullName))
           .ForMember(dest => dest.BudgetAllocated,opt => opt.MapFrom(src => src.BudgetLine.AllocatedAmount))
           .ForMember(dest => dest.BudgetUtilized,opt => opt.MapFrom(src => src.BudgetLine.UtilizedAmount));

            // Opex
            CreateMap<OpexRequest, OpexReportDTO>()
            .ForMember(dest => dest.RequestedBy,opt => opt.MapFrom(src => src.RequestedByUser.FullName))
            .ForMember(dest => dest.BudgetAllocated,opt => opt.MapFrom(src => src.BudgetLine.AllocatedAmount))
            .ForMember(dest => dest.BudgetUtilized,opt => opt.MapFrom(src => src.BudgetLine.UtilizedAmount))
            .ForMember(dest => dest.TotalExpenseClaims,opt => opt.MapFrom(src => src.ExpenseClaims.Count))
            .ForMember(dest => dest.TotalExpenseAmount, opt => opt.MapFrom(src => src.ExpenseClaims.Sum(x => x.ExpenseAmount)));

            // budget variamce
            CreateMap<BudgetLine, BudgetVarianceReportDTO>()
                .ForMember(dest => dest.BudgetCode,opt => opt.MapFrom(src => src.Budget.BudgetCode))
                .ForMember(dest => dest.BudgetName, opt => opt.MapFrom(src => src.Budget.BudgetName))
                .ForMember(dest => dest.FinancialYear,opt => opt.MapFrom(src => src.Budget.FinancialYear))
                .ForMember(dest => dest.BudgetAmount,opt => opt.MapFrom(src => src.Budget.BudgetAmount))
                .ForMember(dest => dest.CategoryName,opt => opt.MapFrom(src => src.BudgetCategory.CategoryName))
                .ForMember(dest => dest.DepartmentName,opt => opt.MapFrom(src => src.BudgetCategory.Department.DepartmentName))
                .ForMember(dest => dest.UtilizedAmount,opt => opt.MapFrom(src => src.UtilizedAmount.GetValueOrDefault()))
                .ForMember(dest => dest.RemainingAmount, opt => opt.MapFrom(src => src.AllocatedAmount - (src.UtilizedAmount.GetValueOrDefault())));


            CreateMap<JournalEntry, BalanceSheetReportDTO>()
              .ForMember(dest => dest.AccountCode,opt => opt.MapFrom(src => src.AccountMaster.AccountCode))
              .ForMember(dest => dest.AccountName,opt => opt.MapFrom(src => src.AccountMaster.AccountName))
              .ForMember(dest => dest.AccountType,opt => opt.MapFrom(src => src.AccountMaster.AccountType))
              .ForMember(dest => dest.DebitAmount,opt => opt.MapFrom(src => src.DebitAmount ?? 0))
              .ForMember(dest => dest.CreditAmount,opt => opt.MapFrom(src => src.CreditAmount ?? 0))
              .ForMember(dest => dest.Balance,opt => opt.MapFrom(src => (src.DebitAmount ?? 0) - (src.CreditAmount ?? 0)));
        }
    }
}
