using Fincore.Domain.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fincore.Infrastructure.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
            
        }

        // DbSet Properties for all entities
        public DbSet<MasterType> MasterTypes { get; set; }
        public DbSet<Currency> Currencies { get; set; }
        public DbSet<Country> Countries { get; set; }
        public DbSet<State> States { get; set; }
        public DbSet<City> Cities { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Company> Companies { get; set; }
        public DbSet<Department> Departments { get; set; }
        public DbSet<Permission> Permissions { get; set; }
        public DbSet<Employee> Employees { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<VendorCategory> VendorCategories { get; set; }
        public DbSet<Vendor> Vendors { get; set; }
        public DbSet<DocumentType> DocumentTypes { get; set; }
        public DbSet<Document> Documents { get; set; }
        public DbSet<BudgetCategory> BudgetCategories { get; set; }
        public DbSet<Budget> Budgets { get; set; }
        public DbSet<BudgetLine> BudgetLines { get; set; }
        public DbSet<ApprovalFlow> ApprovalFlows { get; set; }
        public DbSet<CapexRequest> CapexRequests { get; set; }
        public DbSet<PurchaseRequisition> PurchaseRequisitions { get; set; }
        public DbSet<PurchaseRequisitionItem> PurchaseRequisitionItems { get; set; }
        public DbSet<RFQ> RFQs { get; set; }
        public DbSet<RFQVendor> RFQVendors { get; set; }
        public DbSet<Quotation> Quotations { get; set; }
        public DbSet<QuotationItem> QuotationItems { get; set; }
        public DbSet<VendorSelection> VendorSelections { get; set; }
        public DbSet<PurchaseOrder> PurchaseOrders { get; set; }
        public DbSet<PurchaseOrderItem> PurchaseOrderItems { get; set; }
        public DbSet<GRN> GRNs { get; set; }
        public DbSet<Asset> Assets { get; set; }
        public DbSet<OpexRequest> OpexRequests { get; set; }
        public DbSet<ExpenseClaim> ExpenseClaims { get; set; }
        public DbSet<WorkOrder> WorkOrders { get; set; }
        public DbSet<AccountMaster> AccountMasters { get; set; }
        public DbSet<RevenueEntry> RevenueEntries { get; set; }
        public DbSet<APInvoice> APInvoices { get; set; }
        public DbSet<ARInvoice> ARInvoices { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<JournalEntry> JournalEntries { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }
        public DbSet<UserActivityLog> UserActivityLogs { get; set; }
        public DbSet<NotificationLog> NotificationLogs { get; set; }
        public DbSet<ApprovalLog> ApprovalLogs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Currency Configuration
            modelBuilder.Entity<Currency>(entity =>
            {
                entity.HasKey(e => e.CurrencyId);
                entity.Property(e => e.CurrencyName).IsRequired().HasMaxLength(20);
            });

            // Country Configuration
            modelBuilder.Entity<Country>(entity =>
            {
                entity.HasKey(e => e.CountryId);
                entity.HasOne(e => e.Currency)
                    .WithMany(c => c.Countries)
                    .HasForeignKey(e => e.CurrencyId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // State Configuration
            modelBuilder.Entity<State>(entity =>
            {
                entity.HasKey(e => e.StateId);
                entity.HasOne(e => e.Country)
                    .WithMany(c => c.States)
                    .HasForeignKey(e => e.CountryId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // City Configuration
            modelBuilder.Entity<City>(entity =>
            {
                entity.HasKey(e => e.CityId);
                entity.HasOne(e => e.State)
                    .WithMany(s => s.Cities)
                    .HasForeignKey(e => e.StateId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // MasterType Configuration
            modelBuilder.Entity<MasterType>(entity =>
            {
                entity.HasKey(e => e.MasterTypeId);
                entity.Property(e => e.MasterTypeName).IsRequired().HasMaxLength(25);
            });

            // User Configuration
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.UserId);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(30);
                entity.HasIndex(e => e.Email).IsUnique();

                entity.HasOne(e => e.Role)
                    .WithMany(r => r.Users)
                    .HasForeignKey(e => e.RoleId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.CreatedByUser)
                    .WithMany()
                    .HasForeignKey(e => e.CreatedBy)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.ModifiedByUser)
                    .WithMany()
                    .HasForeignKey(e => e.ModifiedBy)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Role Configuration
            modelBuilder.Entity<Role>(entity =>
            {
                entity.HasKey(e => e.RoleId);
                entity.Property(e => e.RoleName).IsRequired().HasMaxLength(30);
                entity.HasIndex(e => e.RoleName).IsUnique();

                entity.HasOne(e => e.CreatedByUser)
                    .WithMany(u => u.RolesCreated)
                    .HasForeignKey(e => e.CreatedBy)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.ModifiedByUser)
                    .WithMany(u => u.RolesModified)
                    .HasForeignKey(e => e.ModifiedBy)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Company Configuration
            modelBuilder.Entity<Company>(entity =>
            {
                entity.HasKey(e => e.CompanyId);
                entity.Property(e => e.CompanyCode).IsRequired().HasMaxLength(10);
                entity.HasIndex(e => e.CompanyCode).IsUnique();

                entity.HasOne(e => e.Country)
                    .WithMany(c => c.Companies)
                    .HasForeignKey(e => e.CountryId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.MasterType)
                    .WithMany(m => m.Companies)
                    .HasForeignKey(e => e.MasterTypeId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.CreatedByUser)
                    .WithMany(u => u.CompaniesCreated)
                    .HasForeignKey(e => e.CreatedBy)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.ModifiedByUser)
                    .WithMany(u => u.CompaniesModified)
                    .HasForeignKey(e => e.ModifiedBy)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Department Configuration
            modelBuilder.Entity<Department>(entity =>
            {
                entity.HasKey(e => e.DepartmentId);

                entity.Property(e => e.DepartmentCode)
                    .IsRequired()
                    .HasMaxLength(30);

                entity.HasIndex(e => new
                {
                    e.CompanyId,
                    e.DepartmentCode
                })
                .IsUnique();

                entity.HasOne(e => e.Company)
                    .WithMany(c => c.Departments)
                    .HasForeignKey(e => e.CompanyId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.MasterType)
                    .WithMany(m => m.Departments)
                    .HasForeignKey(e => e.MasterTypeId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Manager)
                    .WithMany()
                    .HasForeignKey(e => e.ManagerId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.CreatedByUser)
                    .WithMany(u => u.DepartmentsCreated)
                    .HasForeignKey(e => e.CreatedBy)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.ModifiedByUser)
                    .WithMany(u => u.DepartmentsModified)
                    .HasForeignKey(e => e.ModifiedBy)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Permission Configuration
            modelBuilder.Entity<Permission>(entity =>
            {
                entity.HasKey(e => e.PermissionId);

                entity.HasOne(e => e.Role)
                    .WithMany(r => r.Permissions)
                    .HasForeignKey(e => e.RoleId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.MasterType)
                    .WithMany(m => m.Permissions)
                    .HasForeignKey(e => e.MasterTypeId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.CreatedByUser)
                    .WithMany(u => u.PermissionsCreated)
                    .HasForeignKey(e => e.CreatedBy)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.ModifiedByUser)
                    .WithMany(u => u.PermissionsModified)
                    .HasForeignKey(e => e.ModifiedBy)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Employee Configuration
            modelBuilder.Entity<Employee>(entity =>
            {
                entity.HasKey(e => e.EmployeeId);

                entity.Property(e => e.EmployeeCode)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.HasIndex(e => e.EmployeeCode)
                    .IsUnique();


                // Employee -> User
                entity.HasOne(e => e.User)
                    .WithMany()
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Restrict);


                // Employee -> Department
                entity.HasOne(e => e.Department)
                    .WithMany(d => d.Employees)
                    .HasForeignKey(e => e.DepartmentId)
                    .OnDelete(DeleteBehavior.Restrict);


                // Employee -> Role (Designation)
                entity.HasOne(e => e.DesignationRole)
                    .WithMany()
                    .HasForeignKey(e => e.Designation)
                    .OnDelete(DeleteBehavior.Restrict);


                // Employee -> Company
                entity.HasOne(e => e.Company)
                    .WithMany(c => c.Employees)
                    .HasForeignKey(e => e.CompanyId)
                    .OnDelete(DeleteBehavior.Restrict);


                // Employee -> Reporting Manager
                entity.HasOne(e => e.ReportingManagerEmployee)
                    .WithMany(m => m.Subordinates)
                    .HasForeignKey(e => e.ReportingManager)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Customer Configuration
            modelBuilder.Entity<Customer>(entity =>
            {
                entity.HasKey(e => e.CustomerId);
                entity.Property(e => e.CustomerCode).IsRequired().HasMaxLength(30);
                entity.HasIndex(e => e.CustomerCode).IsUnique();

                entity.HasOne(e => e.User)
                    .WithMany()
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Company)
                    .WithMany(c => c.Customers)
                    .HasForeignKey(e => e.CompanyId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // VendorCategory Configuration
            modelBuilder.Entity<VendorCategory>(entity =>
            {
                entity.HasKey(e => e.VendorCategoryId);

                entity.HasOne(e => e.CreatedByUser)
                    .WithMany(u => u.VendorCategoriesCreated)
                    .HasForeignKey(e => e.CreatedBy)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.ModifiedByUser)
                    .WithMany(u => u.VendorCategoriesModified)
                    .HasForeignKey(e => e.ModifiedBy)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Vendor Configuration
            modelBuilder.Entity<Vendor>(entity =>
            {
                entity.HasKey(e => e.VendorId);
                entity.Property(e => e.VendorCode).IsRequired().HasMaxLength(30);
                entity.HasIndex(e => e.VendorCode).IsUnique();

                entity.HasOne(e => e.VendorCategory)
                    .WithMany(vc => vc.Vendors)
                    .HasForeignKey(e => e.VendorCategoryId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Company)
                    .WithMany(c => c.Vendors)
                    .HasForeignKey(e => e.CompanyId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.CreatedByUser)
                    .WithMany(u => u.VendorsCreated)
                    .HasForeignKey(e => e.CreatedBy)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.ModifiedByUser)
                    .WithMany(u => u.VendorsModified)
                    .HasForeignKey(e => e.ModifiedBy)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // DocumentType Configuration
            modelBuilder.Entity<DocumentType>(entity =>
            {
                entity.HasKey(e => e.DocumentTypeId);

                entity.HasOne(e => e.CreatedByUser)
                    .WithMany(u => u.DocumentTypesCreated)
                    .HasForeignKey(e => e.CreatedBy)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.ModifiedByUser)
                    .WithMany(u => u.DocumentTypesModified)
                    .HasForeignKey(e => e.ModifiedBy)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Document Configuration
            modelBuilder.Entity<Document>(entity =>
            {
                entity.HasKey(e => e.DocumentsId);

                entity.HasOne(e => e.DocumentType)
                    .WithMany(dt => dt.Documents)
                    .HasForeignKey(e => e.DocumentTypeId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.User)
                    .WithMany(u => u.Documents)
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.MasterType)
                    .WithMany(m => m.Documents)
                    .HasForeignKey(e => e.MasterTypeId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // BudgetCategory Configuration
            modelBuilder.Entity<BudgetCategory>(entity =>
            {
                entity.HasKey(e => e.BudgetCategoryId);

                entity.HasOne(e => e.Department)
                    .WithMany(d => d.BudgetCategories)
                    .HasForeignKey(e => e.DepartmentId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.CreatedByUser)
                    .WithMany(u => u.BudgetCategoriesCreated)
                    .HasForeignKey(e => e.CreatedBy)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.ModifiedByUser)
                    .WithMany(u => u.BudgetCategoriesModified)
                    .HasForeignKey(e => e.ModifiedBy)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Budget Configuration
            modelBuilder.Entity<Budget>(entity =>
            {
                entity.HasKey(e => e.BudgetId);
                entity.Property(e => e.BudgetCode).IsRequired().HasMaxLength(20);
                entity.HasIndex(e => e.BudgetCode).IsUnique();

                entity.HasOne(e => e.CreatedByUser)
                    .WithMany(u => u.BudgetsCreated)
                    .HasForeignKey(e => e.CreatedBy)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.ModifiedByUser)
                    .WithMany(u => u.BudgetsModified)
                    .HasForeignKey(e => e.ModifiedBy)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // BudgetLine Configuration
            modelBuilder.Entity<BudgetLine>(entity =>
            {
                entity.HasKey(e => e.BudgetLineId);

                entity.HasOne(e => e.Budget)
                    .WithMany(b => b.BudgetLines)
                    .HasForeignKey(e => e.BudgetId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.BudgetCategory)
                    .WithMany(bc => bc.BudgetLines)
                    .HasForeignKey(e => e.BudgetCategoryId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.CreatedByUser)
                    .WithMany(u => u.BudgetLinesCreated)
                    .HasForeignKey(e => e.CreatedBy)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.ModifiedByUser)
                    .WithMany(u => u.BudgetLinesModified)
                    .HasForeignKey(e => e.ModifiedBy)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // ApprovalFlow Configuration
            modelBuilder.Entity<ApprovalFlow>(entity =>
            {
                entity.HasKey(e => e.ApprovalFlowId);

                entity.HasOne(e => e.RequiredRole)
                    .WithMany(r => r.ApprovalFlows)
                    .HasForeignKey(e => e.RequiredRoleId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.CreatedByUser)
                    .WithMany(u => u.ApprovalFlowsCreated)
                    .HasForeignKey(e => e.CreatedBy)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.ModifiedByUser)
                    .WithMany(u => u.ApprovalFlowsModified)
                    .HasForeignKey(e => e.ModifiedBy)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // CapexRequest Configuration
            modelBuilder.Entity<CapexRequest>(entity =>
            {
                entity.HasKey(e => e.CapexRequestId);

                entity.HasOne(e => e.Department)
                    .WithMany(d => d.CapexRequests)
                    .HasForeignKey(e => e.DepartmentId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.BudgetLine)
                    .WithMany(bl => bl.CapexRequests)
                    .HasForeignKey(e => e.BudgetLineId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.RequestedByUser)
                    .WithMany(u => u.CapexRequestsRequested)
                    .HasForeignKey(e => e.RequestedBy)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.ApprovedByUser)
                    .WithMany(u => u.CapexRequestsApproved)
                    .HasForeignKey(e => e.ApprovedBy)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // PurchaseRequisition Configuration
            modelBuilder.Entity<PurchaseRequisition>(entity =>
            {
                entity.HasKey(e => e.PurchaseRequisitionId);
                entity.Property(e => e.PRNumber).IsRequired().HasMaxLength(50);
                entity.HasIndex(e => e.PRNumber).IsUnique();

                entity.HasOne(e => e.CapexRequest)
                    .WithMany()
                    .HasForeignKey(e => e.CapexRequestId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.RequestedByUser)
                    .WithMany(u => u.PurchaseRequisitionsRequested)
                    .HasForeignKey(e => e.RequestedBy)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.ApprovedByUser)
                    .WithMany(u => u.PurchaseRequisitionsApproved)
                    .HasForeignKey(e => e.ApprovedBy)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.CreatedByUser)
                    .WithMany(u => u.PurchaseRequisitionsCreated)
                    .HasForeignKey(e => e.CreatedBy)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.ModifiedByUser)
                    .WithMany(u => u.PurchaseRequisitionsModified)
                    .HasForeignKey(e => e.ModifiedBy)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // PurchaseRequisitionItem Configuration
            modelBuilder.Entity<PurchaseRequisitionItem>(entity =>
            {
                entity.HasKey(e => e.PRItemId);

                entity.HasOne(e => e.PurchaseRequisition)
                    .WithMany(pr => pr.PurchaseRequisitionItems)
                    .HasForeignKey(e => e.PurchaseRequisitionId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.VendorCategory)
                    .WithMany(vc => vc.PurchaseRequisitionItems)
                    .HasForeignKey(e => e.CategoryId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // RFQ Configuration
            modelBuilder.Entity<RFQ>(entity =>
            {
                entity.HasKey(e => e.RFQId);
                entity.Property(e => e.RFQNumber).IsRequired().HasMaxLength(30);
                entity.HasIndex(e => e.RFQNumber).IsUnique();

                entity.HasOne(e => e.PurchaseRequisition)
                    .WithMany(pr => pr.RFQs)
                    .HasForeignKey(e => e.PurchaseRequisitionId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.CreatedByEmployee)
                    .WithMany(em => em.RFQsCreated)
                    .HasForeignKey(e => e.CreatedBy)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // RFQVendor Configuration
            modelBuilder.Entity<RFQVendor>(entity =>
            {
                entity.HasKey(e => e.RFQVendorId);

                entity.HasOne(e => e.RFQ)
                    .WithMany(r => r.RFQVendors)
                    .HasForeignKey(e => e.RFQId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Vendor)
                    .WithMany(v => v.RFQVendors)
                    .HasForeignKey(e => e.VendorId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Quotation Configuration
            modelBuilder.Entity<Quotation>(entity =>
            {
                entity.HasKey(e => e.QuotationId);

                entity.HasOne(e => e.RFQ)
                    .WithMany(r => r.Quotations)
                    .HasForeignKey(e => e.RFQId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Vendor)
                    .WithMany(v => v.Quotations)
                    .HasForeignKey(e => e.VendorId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // QuotationItem Configuration
            modelBuilder.Entity<QuotationItem>(entity =>
            {
                entity.HasKey(e => e.QuotationItemId);

                entity.HasOne(e => e.Quotation)
                    .WithMany(q => q.QuotationItems)
                    .HasForeignKey(e => e.QuotationId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.PurchaseRequisitionItem)
                    .WithMany(pri => pri.QuotationItems)
                    .HasForeignKey(e => e.PRItemId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // VendorSelection Configuration
            modelBuilder.Entity<VendorSelection>(entity =>
            {
                entity.HasKey(e => e.VendorSelectionId);

                entity.HasOne(e => e.RFQ)
                    .WithMany(r => r.VendorSelections)
                    .HasForeignKey(e => e.RFQId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Quotation)
                    .WithMany(q => q.VendorSelections)
                    .HasForeignKey(e => e.QuotationId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.SelectedVendor)
                    .WithMany(v => v.VendorSelections)
                    .HasForeignKey(e => e.SelectedVendorId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.SelectedByUser)
                    .WithMany(u => u.VendorSelectionsSelected)
                    .HasForeignKey(e => e.SelectedBy)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // PurchaseOrder Configuration
            modelBuilder.Entity<PurchaseOrder>(entity =>
            {
                entity.HasKey(e => e.POId);
                entity.Property(e => e.POCode).IsRequired().HasMaxLength(30);
                entity.HasIndex(e => e.POCode).IsUnique();

                entity.HasOne(e => e.PurchaseRequisition)
                    .WithMany()
                    .HasForeignKey(e => e.PurchaseRequisitionId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Quotation)
                    .WithMany(q => q.PurchaseOrders)
                    .HasForeignKey(e => e.QuotationId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Vendor)
                    .WithMany(v => v.PurchaseOrders)
                    .HasForeignKey(e => e.VendorId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.RequestedByUser)
                    .WithMany(u => u.PurchaseOrdersRequested)
                    .HasForeignKey(e => e.RequestedBy)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.ApprovedByUser)
                    .WithMany(u => u.PurchaseOrdersApproved)
                    .HasForeignKey(e => e.ApprovedBy)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.CreatedByUser)
                    .WithMany(u => u.PurchaseOrdersCreated)
                    .HasForeignKey(e => e.CreatedBy)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.ModifiedByUser)
                    .WithMany(u => u.PurchaseOrdersModified)
                    .HasForeignKey(e => e.ModifiedBy)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // PurchaseOrderItem Configuration
            modelBuilder.Entity<PurchaseOrderItem>(entity =>
            {
                entity.HasKey(e => e.POItemId);

                entity.HasOne(e => e.PurchaseOrder)
                    .WithMany(po => po.PurchaseOrderItems)
                    .HasForeignKey(e => e.POId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.PurchaseRequisitionItem)
                    .WithMany(pri => pri.PurchaseOrderItems)
                    .HasForeignKey(e => e.PRItemId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // GRN Configuration
            modelBuilder.Entity<GRN>(entity =>
            {
                entity.HasKey(e => e.GRNId);
                entity.Property(e => e.GRNCode).IsRequired().HasMaxLength(30);
                entity.HasIndex(e => e.GRNCode).IsUnique();

                entity.HasOne(e => e.PurchaseOrder)
                    .WithMany(po => po.GRNs)
                    .HasForeignKey(e => e.POId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Vendor)
                    .WithMany(v => v.GRNs)
                    .HasForeignKey(e => e.VendorId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.ReceivedByUser)
                    .WithMany(u => u.GRNsReceived)
                    .HasForeignKey(e => e.ReceivedBy)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.QualityCheckedByUser)
                    .WithMany(u => u.GRNsQualityChecked)
                    .HasForeignKey(e => e.QualityCheckedBy)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.CreatedByEmployee)
                    .WithMany(em => em.GRNsCreated)
                    .HasForeignKey(e => e.CreatedBy)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Asset Configuration
            modelBuilder.Entity<Asset>(entity =>
            {
                entity.HasKey(e => e.AssetId);
                entity.Property(e => e.AssetCode).IsRequired().HasMaxLength(50);
                entity.HasIndex(e => e.AssetCode).IsUnique();

                entity.HasOne(e => e.CapexRequest)
                    .WithMany(cr => cr.Assets)
                    .HasForeignKey(e => e.CapexRequestId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.PurchaseOrder)
                    .WithMany(po => po.Assets)
                    .HasForeignKey(e => e.PurchaseOrderId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.GRN)
                    .WithMany(g => g.Assets)
                    .HasForeignKey(e => e.GRNId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Vendor)
                    .WithMany(v => v.Assets)
                    .HasForeignKey(e => e.VendorId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Department)
                    .WithMany(d => d.Assets)
                    .HasForeignKey(e => e.DepartmentId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // OpexRequest Configuration
            modelBuilder.Entity<OpexRequest>(entity =>
            {
                entity.HasKey(e => e.OpexRequestId);

                entity.HasOne(e => e.BudgetLine)
                    .WithMany(bl => bl.OpexRequests)
                    .HasForeignKey(e => e.BudgetLineId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.RequestedByUser)
                    .WithMany(u => u.OpexRequestsRequested)
                    .HasForeignKey(e => e.RequestedBy)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.ApprovedByUser)
                    .WithMany(u => u.OpexRequestsApproved)
                    .HasForeignKey(e => e.ApprovedBy)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // ExpenseClaim Configuration
            modelBuilder.Entity<ExpenseClaim>(entity =>
            {
                entity.HasKey(e => e.ExpenseClaimId);
                entity.Property(e => e.ClaimNumber).IsRequired().HasMaxLength(50);
                entity.HasIndex(e => e.ClaimNumber).IsUnique();

                entity.HasOne(e => e.OpexRequest)
                    .WithMany(or => or.ExpenseClaims)
                    .HasForeignKey(e => e.OpexRequestId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.ClaimByUser)
                    .WithMany(u => u.ExpenseClaimsClaimed)
                    .HasForeignKey(e => e.ClaimBy)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.ApprovedByUser)
                    .WithMany(u => u.ExpenseClaimsApproved)
                    .HasForeignKey(e => e.ApprovedBy)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // WorkOrder Configuration
            modelBuilder.Entity<WorkOrder>(entity =>
            {
                entity.HasKey(e => e.WorkOrderId);
                entity.Property(e => e.WONumber).IsRequired().HasMaxLength(30);
                entity.HasIndex(e => e.WONumber).IsUnique();

                entity.HasOne(e => e.Vendor)
                    .WithMany(v => v.WorkOrders)
                    .HasForeignKey(e => e.VendorId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.OpexRequest)
                    .WithMany(or => or.WorkOrders)
                    .HasForeignKey(e => e.OpexRequestId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.CreatedByUser)
                    .WithMany(u => u.WorkOrdersCreated)
                    .HasForeignKey(e => e.CreatedBy)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // AccountMaster Configuration
            modelBuilder.Entity<AccountMaster>(entity =>
            {
                entity.HasKey(e => e.AccountId);
                entity.Property(e => e.AccountCode).IsRequired().HasMaxLength(30);
                entity.HasIndex(e => e.AccountCode).IsUnique();

                entity.HasOne(e => e.CreatedByUser)
                    .WithMany(u => u.AccountMastersCreated)
                    .HasForeignKey(e => e.CreatedBy)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.ModifiedByUser)
                    .WithMany(u => u.AccountMastersModified)
                    .HasForeignKey(e => e.ModifiedBy)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // RevenueEntry Configuration
            modelBuilder.Entity<RevenueEntry>(entity =>
            {
                entity.HasKey(e => e.RevenueEntryId);
                entity.HasIndex(e => e.InvoiceNumber).IsUnique();

                entity.HasOne(e => e.Customer)
                    .WithMany(c => c.RevenueEntries)
                    .HasForeignKey(e => e.CustomerId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Department)
                    .WithMany(d => d.RevenueEntries)
                    .HasForeignKey(e => e.DepartmentId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.AccountMaster)
                    .WithMany(am => am.RevenueEntries)
                    .HasForeignKey(e => e.AccountId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.CreatedByUser)
                    .WithMany(u => u.RevenueEntriesCreated)
                    .HasForeignKey(e => e.CreatedBy)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.ModifiedByUser)
                    .WithMany(u => u.RevenueEntriesModified)
                    .HasForeignKey(e => e.ModifiedBy)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // APInvoice Configuration
            modelBuilder.Entity<APInvoice>(entity =>
            {
                entity.HasKey(e => e.APInvoiceId);
                entity.Property(e => e.InvoiceNumber).IsRequired().HasMaxLength(50);
                entity.HasIndex(e => e.InvoiceNumber).IsUnique();

                entity.HasOne(e => e.Vendor)
                    .WithMany(v => v.APInvoices)
                    .HasForeignKey(e => e.VendorId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.PurchaseOrder)
                    .WithMany(po => po.APInvoices)
                    .HasForeignKey(e => e.PurchaseOrderId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.GRN)
                    .WithMany(g => g.APInvoices)
                    .HasForeignKey(e => e.GRNId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.WorkOrder)
                    .WithMany(wo => wo.APInvoices)
                    .HasForeignKey(e => e.WorkOrderId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.ApprovedByUser)
                    .WithMany(u => u.APInvoicesApproved)
                    .HasForeignKey(e => e.ApprovedBy)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // ARInvoice Configuration
            modelBuilder.Entity<ARInvoice>(entity =>
            {
                entity.HasKey(e => e.ARInvoiceId);
                entity.Property(e => e.InvoiceNumber).IsRequired().HasMaxLength(50);

                entity.HasOne(e => e.Customer)
                    .WithMany(c => c.ARInvoices)
                    .HasForeignKey(e => e.CustomerId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.RevenueEntry)
                    .WithMany(re => re.ARInvoices)
                    .HasForeignKey(e => e.RevenueEntryId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Payment Configuration
            modelBuilder.Entity<Payment>(entity =>
            {
                entity.HasKey(e => e.PaymentId);
                entity.Property(e => e.PaymentNumber).IsRequired().HasMaxLength(50);
                entity.HasIndex(e => e.PaymentNumber).IsUnique();

                entity.HasOne(e => e.APInvoice)
                    .WithMany(api => api.Payments)
                    .HasForeignKey(e => e.APInvoiceId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.ARInvoice)
                    .WithMany(ari => ari.Payments)
                    .HasForeignKey(e => e.ARInvoiceId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Vendor)
                    .WithMany(v => v.Payments)
                    .HasForeignKey(e => e.VendorId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Customer)
                    .WithMany(c => c.Payments)
                    .HasForeignKey(e => e.CustomerId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.ApprovedByUser)
                    .WithMany(u => u.PaymentsApproved)
                    .HasForeignKey(e => e.ApprovedBy)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // JournalEntry Configuration
            modelBuilder.Entity<JournalEntry>(entity =>
            {
                entity.HasKey(e => e.JournalEntryId);
                entity.Property(e => e.JournalNumber).IsRequired().HasMaxLength(50);
                entity.HasIndex(e => e.JournalNumber).IsUnique();

                entity.HasOne(e => e.AccountMaster)
                    .WithMany(am => am.JournalEntries)
                    .HasForeignKey(e => e.AccountId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.CreatedByUser)
                    .WithMany(u => u.JournalEntriesCreated)
                    .HasForeignKey(e => e.CreatedBy)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // AuditLog Configuration
            modelBuilder.Entity<AuditLog>(entity =>
            {
                entity.HasKey(e => e.AuditLogId);

                entity.HasOne(e => e.AuditByUser)
                    .WithMany(u => u.AuditLogs)
                    .HasForeignKey(e => e.AuditBy)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // UserActivityLog Configuration
            modelBuilder.Entity<UserActivityLog>(entity =>
            {
                entity.HasKey(e => e.UserActivityLogId);

                entity.HasOne(e => e.User)
                    .WithMany(u => u.UserActivityLogs)
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // NotificationLog Configuration
            modelBuilder.Entity<NotificationLog>(entity =>
            {
                entity.HasKey(e => e.NotificationLogId);

                entity.HasOne(e => e.User)
                    .WithMany(u => u.NotificationLogs)
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // ApprovalLog Configuration
            modelBuilder.Entity<ApprovalLog>(entity =>
            {
                entity.HasKey(e => e.ApprovalLogId);

                entity.HasOne(e => e.ApproverUser)
                    .WithMany(u => u.ApprovalLogs)
                    .HasForeignKey(e => e.ApproverId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}