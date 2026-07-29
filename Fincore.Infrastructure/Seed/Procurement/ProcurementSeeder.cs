using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bogus;
using Fincore.Domain.Models;
using Fincore.Infrastructure.Data;
using Fincore.Infrastructure.Seed.Authentication;
using Fincore.Infrastructure.Seed.Helpers;
using Microsoft.EntityFrameworkCore;

namespace Fincore.Infrastructure.Seed.Procurement
{
    /// <summary>Phase 6 – ApprovalFlow ➜ Capex ➜ PR ➜ RFQ ➜ Quotation ➜ VendorSelection ➜ PO ➜ GRN</summary>
    public static class ProcurementSeeder
    {
        public static async Task SeedAsync(AppDbContext db)
        {
            await SeedApprovalFlowsAsync(db);
            await SeedCapexRequestsAsync(db);
            await SeedPurchaseRequisitionsAsync(db);
            await SeedPRItemsAsync(db);
            await SeedRFQsAsync(db);
            await SeedRFQVendorsAsync(db);
            await SeedQuotationsAsync(db);
            await SeedQuotationItemsAsync(db);
            await SeedVendorSelectionsAsync(db);
            await SeedPurchaseOrdersAsync(db);
            await SeedPOItemsAsync(db);
            await SeedGRNsAsync(db);
        }

        private static async Task SeedApprovalFlowsAsync(AppDbContext db)
        {
            if (await db.ApprovalFlows.AnyAsync()) return;
            var admin = RoleSeeder.BootstrapUserId;
            var now = DateTime.UtcNow;
            var roles = await db.Roles.ToListAsync();
            int R(string n) => roles.First(r => r.RoleName == n).RoleId;

            var flows = new (decimal min, decimal max, int level, int role)[]
            {
                (0,          50000,     1, R("Department Head")),
                (50001,      200000,    2, R("Finance Manager")),
                (200001,     1000000,   3, R("Admin")),
                (1000001,    5000000,   4, R("SuperAdmin")),
                (0,          25000,     1, R("Procurement Head")),
                (25001,      100000,    2, R("Finance Manager")),
                (0,          10000,     1, R("HR Manager")),
                (10001,      75000,     2, R("Finance Manager")),
                (0,          100000,    1, R("Accountant")),
                (100001,     500000,    2, R("Finance Manager"))
            };

            db.ApprovalFlows.AddRange(flows.Select(f => new ApprovalFlow
            {
                MinAmount      = f.min,
                MaxAmount      = f.max,
                ApprovalLevel  = f.level,
                RequiredRoleId = f.role,
                IsActive       = 1,
                CreatedAt      = now,
                ModifiedAt     = now,
                CreatedBy      = admin,
                ModifiedBy     = admin
            }));
            await db.SaveChangesAsync();
        }

        private static async Task SeedCapexRequestsAsync(AppDbContext db)
        {
            if (await db.CapexRequests.AnyAsync()) return;
            var depts = await db.Departments.OrderBy(d => d.DepartmentId).ToListAsync();
            var lines = await db.BudgetLines.OrderBy(b => b.BudgetLineId).ToListAsync();
            var users = await db.Users.Where(u => u.UserCategory == "Employee").ToListAsync();
            var admin = RoleSeeder.BootstrapUserId;
            var now = DateTime.UtcNow;
            var rng = new Randomizer(FakerHelper.GlobalSeed + 8);
            string[] statuses = { "Approved", "Approved", "Pending", "Rejected" };

            var list = new List<CapexRequest>();
            for (int i = 0; i < 20; i++)
            {
                var s = statuses[i % statuses.Length];
                list.Add(new CapexRequest
                {
                    Title            = $"Capex Item {i + 1:D2}",
                    Description      = $"Capital expense for infrastructure/asset acquisition #{i + 1}",
                    Amount           = System.Math.Round((decimal)rng.Double(50000, 4500000), 2),
                    DepartmentId     = depts[i % depts.Count].DepartmentId,
                    BudgetLineId     = lines[i % lines.Count].BudgetLineId,
                    RequestedBy      = users[i % users.Count].UserId,
                    ApprovalStatus   = s,
                    ApprovedBy       = s == "Approved" ? admin : (int?)null,
                    ApprovedAt       = s == "Approved" ? now.AddDays(-rng.Int(1, 60)) : (DateTime?)null,
                    CreatedAt        = now.AddDays(-rng.Int(30, 200)),
                    ModifiedAt       = now
                });
            }
            db.CapexRequests.AddRange(list);
            await db.SaveChangesAsync();
        }

        private static async Task SeedPurchaseRequisitionsAsync(AppDbContext db)
        {
            if (await db.PurchaseRequisitions.AnyAsync()) return;
            var capex   = await db.CapexRequests.OrderBy(c => c.CapexRequestId).ToListAsync();
            var users   = await db.Users.Where(u => u.UserCategory == "Employee").ToListAsync();
            var admin = RoleSeeder.BootstrapUserId;
            var now = DateTime.UtcNow;
            var rng = new Randomizer(FakerHelper.GlobalSeed + 9);
            string[] statuses = { "Approved", "Approved", "Pending", "Rejected", "InReview" };

            var list = new List<PurchaseRequisition>();
            for (int i = 0; i < 25; i++)
            {
                var s = statuses[i % statuses.Length];
                var c = capex[i % capex.Count];
                list.Add(new PurchaseRequisition
                {
                    PRNumber          = $"PR-{DateTime.UtcNow.Year}-{i + 1:D4}",
                    CapexRequestId    = c.CapexRequestId,
                    PRTitle           = $"Requisition for {c.Title}",
                    RequestedBy       = users[i % users.Count].UserId,
                    RequiredTillDate  = now.AddDays(rng.Int(15, 90)),
                    ApprovalStatus    = s,
                    ApprovedBy        = s == "Approved" ? admin : (int?)null,
                    IsActive          = 1,
                    ApprovedAt        = s == "Approved" ? now.AddDays(-rng.Int(1, 30)) : (DateTime?)null,
                    CreatedAt         = now.AddDays(-rng.Int(10, 150)),
                    ModifiedAt        = now,
                    CreatedBy         = admin,
                    ModifiedBy        = admin
                });
            }
            db.PurchaseRequisitions.AddRange(list);
            await db.SaveChangesAsync();
        }

        private static async Task SeedPRItemsAsync(AppDbContext db)
        {
            if (await db.PurchaseRequisitionItems.AnyAsync()) return;
            var prs = await db.PurchaseRequisitions.OrderBy(p => p.PurchaseRequisitionId).ToListAsync();
            var cats = await db.VendorCategories.OrderBy(v => v.VendorCategoryId).ToListAsync();
            var rng = new Randomizer(FakerHelper.GlobalSeed + 10);
            string[] units = { "PCS", "BOX", "KG", "LTR", "HOURS", "SET" };
            string[] statuses = { "Open", "Ordered", "Cancelled" };
            string[] items = { "Laptop","Chair","Monitor","Cable","Router","Server Rack","Printer","Software Licence","Cartridge","Bench Testing","Freight","Cleaning Kit","LED Panel","Air Conditioner","Consulting Hours" };

            var list = new List<PurchaseRequisitionItem>();
            foreach (var pr in prs)
            {
                for (int j = 0; j < 2; j++)
                {
                    var qty  = rng.Int(1, 25);
                    var unit = System.Math.Round((decimal)rng.Double(100, 15000), 2);
                    var tax  = 18m;
                    var taxAmt = System.Math.Round(qty * unit * tax / 100, 2);
                    var line = System.Math.Round((unit * qty) + taxAmt, 2);
                    list.Add(new PurchaseRequisitionItem
                    {
                        PurchaseRequisitionId = pr.PurchaseRequisitionId,
                        ItemName              = items[rng.Int(0, items.Length - 1)],
                        ItemDescription       = "Auto-generated item description for procurement seed data.",
                        CategoryId            = cats[rng.Int(0, cats.Count - 1)].VendorCategoryId,
                        Quantity              = qty,
                        UnitOfMaterial        = units[rng.Int(0, units.Length - 1)],
                        EstimatedUnitPrice    = unit,
                        TaxPercentage         = tax,
                        TaxAmount             = taxAmt,
                        LineTotal             = line,
                        ItemStatus            = statuses[rng.Int(0, statuses.Length - 1)]
                    });
                }
            }
            db.PurchaseRequisitionItems.AddRange(list);
            await db.SaveChangesAsync();
        }

        private static async Task SeedRFQsAsync(AppDbContext db)
        {
            if (await db.RFQs.AnyAsync()) return;
            var prs = await db.PurchaseRequisitions.OrderBy(p => p.PurchaseRequisitionId).ToListAsync();
            var employees = await db.Employees.OrderBy(e => e.EmployeeId).ToListAsync();
            var now = DateTime.UtcNow;
            var rng = new Randomizer(FakerHelper.GlobalSeed + 11);

            var list = new List<RFQ>();
            for (int i = 0; i < 20; i++)
            {
                var pr = prs[i % prs.Count];
                var issue = now.AddDays(-rng.Int(5, 60));
                list.Add(new RFQ
                {
                    RFQNumber              = $"RFQ-{DateTime.UtcNow.Year}-{i + 1:D4}",
                    PurchaseRequisitionId  = pr.PurchaseRequisitionId,
                    Title                  = $"RFQ #{i + 1} – {pr.PRTitle.Substring(0, System.Math.Min(15, pr.PRTitle.Length))}",
                    Description            = "Request for quotation issued to shortlisted vendors.",
                    IssueDate              = issue,
                    LastDate               = issue.AddDays(15),
                    IsActive               = 1,
                    CreatedBy              = employees[i % employees.Count].EmployeeId,
                    CreatedAt              = issue,
                    ModifiedAt             = now
                });
            }
            db.RFQs.AddRange(list);
            await db.SaveChangesAsync();
        }

        private static async Task SeedRFQVendorsAsync(AppDbContext db)
        {
            if (await db.RFQVendors.AnyAsync()) return;
            var rfqs = await db.RFQs.OrderBy(r => r.RFQId).ToListAsync();
            var vendors = await db.Vendors.OrderBy(v => v.VendorId).ToListAsync();
            var rng = new Randomizer(FakerHelper.GlobalSeed + 12);
            string[] statuses = { "Invited", "Responded", "Declined" };

            var list = new List<RFQVendor>();
            foreach (var rfq in rfqs)
            {
                for (int j = 0; j < 3; j++)
                {
                    var v = vendors[(rfq.RFQId + j) % vendors.Count];
                    list.Add(new RFQVendor
                    {
                        RFQId          = rfq.RFQId,
                        VendorId       = v.VendorId,
                        InvitedAt      = rfq.IssueDate,
                        ResponseStatus = statuses[rng.Int(0, statuses.Length - 1)]
                    });
                }
            }
            db.RFQVendors.AddRange(list);
            await db.SaveChangesAsync();
        }

        private static async Task SeedQuotationsAsync(AppDbContext db)
        {
            if (await db.Quotations.AnyAsync()) return;
            var rfqs = await db.RFQs.OrderBy(r => r.RFQId).ToListAsync();
            var vendors = await db.Vendors.OrderBy(v => v.VendorId).ToListAsync();
            var rng = new Randomizer(FakerHelper.GlobalSeed + 13);
            var now = DateTime.UtcNow;

            var list = new List<Quotation>();
            int q = 0;
            foreach (var rfq in rfqs)
            {
                // Two competing quotations per RFQ
                for (int k = 0; k < 2; k++)
                {
                    q++;
                    list.Add(new Quotation
                    {
                        RFQId           = rfq.RFQId,
                        VendorId        = vendors[(rfq.RFQId + k) % vendors.Count].VendorId,
                        QuotationNumber = $"QTN-{DateTime.UtcNow.Year}-{q:D5}",
                        QuotedAmount    = System.Math.Round((decimal)rng.Double(30000, 900000), 2),
                        Remarks         = k == 0 ? "Standard terms – 30 day payment" : "Premium offer – includes 12mo AMC",
                        IsSelected      = (byte)(k == 0 ? 1 : 0),
                        CreatedAt       = rfq.IssueDate?.AddDays(3),
                        ModifiedAt      = now
                    });
                }
            }
            db.Quotations.AddRange(list);
            await db.SaveChangesAsync();
        }

        private static async Task SeedQuotationItemsAsync(AppDbContext db)
        {
            if (await db.QuotationItems.AnyAsync()) return;
            var quotations = await db.Quotations.Include(q => q.RFQ).OrderBy(q => q.QuotationId).ToListAsync();
            var rng = new Randomizer(FakerHelper.GlobalSeed + 14);

            var list = new List<QuotationItem>();
            foreach (var qtn in quotations)
            {
                var prItems = await db.PurchaseRequisitionItems
                    .Where(i => i.PurchaseRequisitionId == qtn.RFQ.PurchaseRequisitionId)
                    .ToListAsync();
                foreach (var pri in prItems)
                {
                    var unit  = System.Math.Round((pri.EstimatedUnitPrice ?? 100m) * (decimal)rng.Double(0.9, 1.1), 2);
                    var qty   = pri.Quantity;
                    var disc  = System.Math.Round(unit * qty * (decimal)rng.Double(0, 0.08), 2);
                    var line  = System.Math.Round(unit * qty - disc, 2);
                    list.Add(new QuotationItem
                    {
                        QuotationId    = qtn.QuotationId,
                        PRItemId       = pri.PRItemId,
                        Quantity       = qty,
                        UnitPrice      = unit,
                        TaxPercentage  = pri.TaxPercentage,
                        Discount       = disc,
                        LineTotal      = line
                    });
                }
            }
            db.QuotationItems.AddRange(list);
            await db.SaveChangesAsync();
        }

        private static async Task SeedVendorSelectionsAsync(AppDbContext db)
        {
            if (await db.VendorSelections.AnyAsync()) return;
            var selected = await db.Quotations.Where(q => q.IsSelected == 1).OrderBy(q => q.QuotationId).ToListAsync();
            var admin = RoleSeeder.BootstrapUserId;
            var now = DateTime.UtcNow;

            db.VendorSelections.AddRange(selected.Take(20).Select(q => new VendorSelection
            {
                RFQId            = q.RFQId,
                QuotationId      = q.QuotationId,
                SelectedVendorId = q.VendorId,
                SelectedDate     = now.AddDays(-10),
                SelectedBy       = admin,
                Remarks          = "L1 lowest bid"
            }));
            await db.SaveChangesAsync();
        }

        private static async Task SeedPurchaseOrdersAsync(AppDbContext db)
        {
            if (await db.PurchaseOrders.AnyAsync()) return;
            var selections = await db.VendorSelections
                .Include(v => v.Quotation)
                .Include(v => v.RFQ)
                .OrderBy(v => v.VendorSelectionId)
                .ToListAsync();
            var admin = RoleSeeder.BootstrapUserId;
            var rng = new Randomizer(FakerHelper.GlobalSeed + 15);
            var now = DateTime.UtcNow;
            string[] statuses = { "Approved", "Approved", "Pending", "Delivered" };

            var list = new List<PurchaseOrder>();
            int i = 0;
            foreach (var sel in selections)
            {
                i++;
                var s = statuses[i % statuses.Length];
                list.Add(new PurchaseOrder
                {
                    POCode                 = $"PO-{DateTime.UtcNow.Year}-{i:D4}",
                    PurchaseRequisitionId  = sel.RFQ != null ? sel.RFQ.PurchaseRequisitionId : (int?)null,
                    QuotationId            = sel.QuotationId,
                    VendorId = sel.SelectedVendorId,
                    RequestedBy            = admin,
                    RequiredTillDate       = now.AddDays(rng.Int(15, 90)),
                    OrderDate              = now.AddDays(-rng.Int(5, 45)),
                    ApprovalStatus         = s,
                    Amount                 = sel.Quotation.QuotedAmount,
                    ApprovedBy             = s == "Approved" || s == "Delivered" ? admin : (int?)null,
                    IsActive               = 1,
                    ApprovedAt             = s == "Approved" || s == "Delivered" ? now.AddDays(-rng.Int(1, 20)) : (DateTime?)null,
                    CreatedAt              = now.AddDays(-rng.Int(10, 60)),
                    ModifiedAt             = now,
                    CreatedBy              = admin,
                    ModifiedBy             = admin
                });
            }
            db.PurchaseOrders.AddRange(list);
            await db.SaveChangesAsync();
        }

        private static async Task SeedPOItemsAsync(AppDbContext db)
        {
            if (await db.PurchaseOrderItems.AnyAsync()) return;
            var pos = await db.PurchaseOrders.OrderBy(p => p.POId).ToListAsync();
            var rng = new Randomizer(FakerHelper.GlobalSeed + 16);

            var list = new List<PurchaseOrderItem>();
            foreach (var po in pos)
            {
                // Grab PR items linked via PurchaseRequisitionId
                var prItems = po.PurchaseRequisitionId.HasValue
                    ? await db.PurchaseRequisitionItems.Where(i => i.PurchaseRequisitionId == po.PurchaseRequisitionId.Value).ToListAsync()
                    : new List<PurchaseRequisitionItem>();

                foreach (var pri in prItems)
                {
                    var unit    = System.Math.Round((pri.EstimatedUnitPrice ?? 100m), 2);
                    var qty     = pri.Quantity;
                    var taxPct  = pri.TaxPercentage;
                    var taxAmt  = System.Math.Round(unit * qty * taxPct / 100, 2);
                    list.Add(new PurchaseOrderItem
                    {
                        POId              = po.POId,
                        PRItemId          = pri.PRItemId,
                        ItemName          = pri.ItemName,
                        ItemDescription   = pri.ItemDescription,
                        Quantity          = qty,
                        UnitOfMaterial    = pri.UnitOfMaterial,
                        UnitPrice         = unit,
                        TaxPercentage     = taxPct,
                        TaxAmount         = taxAmt,
                        LineTotal         = System.Math.Round(unit * qty + taxAmt, 2),
                        ItemStatus        = "Ordered"
                    });
                }
            }
            db.PurchaseOrderItems.AddRange(list);
            await db.SaveChangesAsync();
        }

        private static async Task SeedGRNsAsync(AppDbContext db)
        {
            if (await db.GRNs.AnyAsync()) return;
            var pos = await db.PurchaseOrders.Include(p => p.Quotation).OrderBy(p => p.POId).ToListAsync();
            var employees = await db.Employees.OrderBy(e => e.EmployeeId).ToListAsync();
            var admin = RoleSeeder.BootstrapUserId;
            var rng = new Randomizer(FakerHelper.GlobalSeed + 17);
            var now = DateTime.UtcNow;
            string[] qc = { "Passed", "Passed", "Failed" };
            string[] gs = { "Received", "Partial", "Rejected" };

            var list = new List<GRN>();
            int i = 0;
            foreach (var po in pos)
            {
                i++;
                list.Add(new GRN
                {
                    GRNCode              = $"GRN-{DateTime.UtcNow.Year}-{i:D4}",
                    POId                 = po.POId,
                    VendorId             = po.Quotation.VendorId,
                    IsActive             = 1,
                    ReceivedDate         = now.AddDays(-rng.Int(1, 30)),
                    ReceivedBy           = admin,
                    QualityCheckStatus   = qc[i % qc.Length],
                    QualityCheckedBy     = admin,
                    GRNStatus            = gs[i % gs.Length],
                    Remarks              = "Goods received and inspected as per PO.",
                    CreatedAt            = now.AddDays(-rng.Int(1, 30)),
                    ModifiedAt           = now,
                    CreatedBy            = employees[i % employees.Count].EmployeeId
                });
            }
            db.GRNs.AddRange(list);
            await db.SaveChangesAsync();
        }
    }
}
