using Fincore.Application;
using Fincore.Application.AutoMapper;
using Fincore.Application.AutoMapper.Capex;
using Fincore.Application.AutoMapper.Capex;
using Fincore.Application.AutoMapper.MasterTable;
using Fincore.Application.AutoMapper.MasterTable;
using Fincore.Application.AutoMapper.MasterTable;
using Fincore.Application.AutoMapper.Payment;
using Fincore.Application.Interfaces;
using Fincore.Application.Interfaces.Budget;
using Fincore.Application.Interfaces.BudgetCategory;
using Fincore.Application.Interfaces.BudgetLine;
using Fincore.Application.Interfaces.Dashboard;
using Fincore.Application.Interfaces.ExpenseClaim;
using Fincore.Application.Interfaces.ICapex;
using Fincore.Application.Interfaces.ICapex;
using Fincore.Application.Interfaces.IMasterTable;
using Fincore.Application.Interfaces.IMasterTable;
using Fincore.Application.Interfaces.IMasterTable;
using Fincore.Application.Interfaces.IPayment;
using Fincore.Application.Interfaces.IPayment;
using Fincore.Application.Interfaces.Opex;
using Fincore.Application.Interfaces.Payment;
using Fincore.Application.Interfaces.Reports;
using Fincore.Application.Interfaces.WorkOrder;
using Fincore.Application.Mapper;
using Fincore.Application.Mapping;
using Fincore.Domain.Models;
using Fincore.Infrastructure;
using Fincore.Infrastructure.Data;
using Fincore.Infrastructure.Mapping.Payment;
using Fincore.Infrastructure.Seed;
using Fincore.Infrastructure.Services;
using Fincore.Infrastructure.Services.Budget;
using Fincore.Infrastructure.Services.BudgetCategory;
using Fincore.Infrastructure.Services.BudgetLine;
using Fincore.Infrastructure.Services.Capex;
using Fincore.Infrastructure.Services.Capex;
using Fincore.Infrastructure.Services.Dashboard;
using Fincore.Infrastructure.Services.ExpenseClaim;
using Fincore.Infrastructure.Services.MasterTable;
using Fincore.Infrastructure.Services.MasterTable;
using Fincore.Infrastructure.Services.Opex;
using Fincore.Infrastructure.Services.Payment;
using Fincore.Infrastructure.Services.PaymentModule;
using Fincore.Infrastructure.Services.Reports;
using Fincore.Infrastructure.Services.WorkOrder;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using QuestPDF.Infrastructure;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;



var builder = WebApplication.CreateBuilder(args);
QuestPDF.Settings.License = LicenseType.Community;
// Add services to the container.
builder.Services.AddControllers();

builder.Services.AddControllers()
    .ConfigureApiBehaviorOptions(options =>
    {
        options.SuppressModelStateInvalidFilter = true;
    })
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    });
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo { Title = "Fincore.API", Version = "v1" });

    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Paste Access Token Here"
    });

    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});


builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("dbconn"),
        b => b.MigrationsAssembly("Fincore.Infrastructure")));



//builder.Services.AddScoped<IARInvoiceService, ARInvoiceService>();
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("Jwt"));
var jwtSettings = builder.Configuration.GetSection("Jwt").Get<JwtSettings>();



// ---------------------- AutoMapper ----------------------
builder.Services.AddAutoMapper(typeof(AccountsMasterMapper));
builder.Services.AddAutoMapper(typeof(PaymentMapper));
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
builder.Services.AddAutoMapper(typeof(MapperConfigPayment));
builder.Services.AddAutoMapper(typeof(APInvoiceProfile));
builder.Services.AddAutoMapper(typeof(ARInvoiceProfile));
// ---------------------- Services ----------------------
builder.Services.AddScoped<IAccountMasterService, AccountMasterService>();

builder.Services.AddScoped<IRevenueService, RevenueService>();
builder.Services.AddScoped<IAPInvoiceService, APInvoiceService>();
builder.Services.AddScoped<IARInvoiceService, ARInvoiceService>();

builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddScoped<IJournalEntryService, JournalEntryService>();

builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IJwtTokenHelper, JwtTokenHelper>();
builder.Services.AddScoped<ITwoFactorHelper, TwoFactorHelper>();



builder.Services.AddScoped<ICityService, CityService>();
builder.Services.AddScoped<ICountryService, CountryService>();
builder.Services.AddScoped<IStateService, StateService>();
builder.Services.AddScoped<ICurrencyService, CurrencyService>();
builder.Services.AddScoped<IAuditLogService, AuditLogService>();
builder.Services.AddScoped<IUserActivityLogService, UserActivityLogService>();
builder.Services.AddScoped<INotificationLogService, NotificationLogService>();
builder.Services.AddScoped<IApprovalLogService, ApprovalLogService>();
builder.Services.AddAutoMapper(typeof(LogsMappingProfile));







builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings.Issuer,
        ValidAudience = jwtSettings.Audience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Key)),
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddScoped<IRevenueReportService, RevenueReportService>();

builder.Services.AddScoped<IExpenseReportService, ExpenseReportService>();

builder.Services.AddScoped<IProfitLossReportService, ProfitLossReportService>();

builder.Services.AddScoped<IBalanceSheetReportService, BalanceSheetReportService>();

builder.Services.AddScoped<ICashFlowReportService, CashFlowReportService>();

builder.Services.AddScoped<IBudgetVarianceReportService, BudgetVarianceReportService>();

builder.Services.AddScoped<ICapexReportService, CapexReportService>();

builder.Services.AddScoped<IOpexReportService, OpexReportService>();

builder.Services.AddScoped<IVendorSpendReportService, VendorSpendReportService>();

//builder.Services.AddScoped<ITrialBalanceReportService, TrialBalanceReportService>();

//builder.Services.AddScoped<IBudgetReportService, BudgetReportService>();

//builder.Services.AddScoped<IGeneralLedgerReportService, GeneralLedgerReportService>();


builder.Services.AddScoped<IDocumentService, DocumentService>();
builder.Services.AddScoped<IDocumentTypeService,DocumentTypeService>();
builder.Services.AddScoped<IExecutiveService, ExecutiveService>();
builder.Services.AddScoped<IFinanceService, FinanceService>();
builder.Services.AddScoped<IProcurementService, ProcurementService>();

builder.Services.AddScoped<IMasterType, MasterTypeService>();
builder.Services.AddAutoMapper(typeof(DocumentMappingProfile));

builder.Services.AddMemoryCache();

// ---------------------- Rate Limiting ----------------------
builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("FixedPolicy", policy =>
    {
        policy.PermitLimit = 10;
        policy.Window = TimeSpan.FromMinutes(1);
        policy.QueueLimit = 0;
        policy.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
    });

    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
});

builder.Services.AddMemoryCache();
builder.Services.AddAutoMapper(
    typeof(MapperConfigPurchaseOrderItem),
    typeof(MapperConfigPurchaseOrderItem),
    typeof(MapperConfigAsset),
    typeof(MapperConfigGRN)
);

builder.Services.AddScoped<IPurchaseOrderService, PurchaseOrderService>();
builder.Services.AddScoped<IPurchaseOrderItemService, PurchaseOrderItemService>();
builder.Services.AddScoped<IAssetService, AssetService>();
builder.Services.AddScoped<IGRNService, GRNService>();

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

builder.Services.AddScoped<ICapexReq, CapexReq>();
builder.Services.AddScoped<IPRService, PRService>();
builder.Services.AddScoped<IPRItemService, PRItemService>();
builder.Services.AddScoped<IRFQService, RFQService>();
builder.Services.AddScoped<IRFQVendorService, RFQVendorService>();
builder.Services.AddScoped<IQuotationService, QuotationService>();
builder.Services.AddScoped<IQuotationItemService, QuotationItemService>();
builder.Services.AddScoped<IVendorSelectionService, VendorSelectionService>();
builder.Services.AddScoped<IApprovalFlowService, ApprovalFlowService>();
builder.Services.AddScoped<IOpexRequestService, OpexRequestService>();
builder.Services.AddScoped<IExpenseClaimService, ExpenseClaimService>();
builder.Services.AddScoped<IWorkOrderService, WorkOrderService>();
builder.Services.AddScoped<IBudgetCategoryService, BudgetCategoryService>();
builder.Services.AddScoped<IBudgetService, BudgetService>();
builder.Services.AddScoped<IDashboardBudgetService, DashboardBudgetService>();

builder.Services.AddScoped<IBudgetLineService, BudgetLineService>();

builder.Services.AddAutoMapper(typeof(MasterTableMappingProfile));
builder.Services.AddAutoMapper(typeof(MappingProfile));





builder.Services.AddScoped<ICompanyService, CompanyService>();
builder.Services.AddScoped<IDepartmentService, DepartmentService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
builder.Services.AddScoped<IPermissionService, PermissionService>();
builder.Services.AddScoped<IRoleService, RoleService>();
builder.Services.AddScoped<IEmployeeService, EmployeeService>();
builder.Services.AddScoped<ICustomerService, CustomerService>();
builder.Services.AddScoped<IVendorService, VendorService>();
builder.Services.AddScoped<IVendorCategoryService, VendorCategoryService>();
builder.Services.AddAutoMapper(typeof(AMCapexRequest));
builder.Services.AddScoped<IReportService, ReportService>();
builder.Services.AddScoped<IGeneralLedgerService, GeneralLedgerService>();
var app = builder.Build();


// -------------------------
// Configure HTTP Pipeline
// -------------------------

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

await DatabaseSeeder.SeedAsync(app.Services);
app.UseHttpsRedirection();

app.UseRateLimiter();

app.UseAuthentication();
app.UseAuthorization();

app.UseRateLimiter();

app.MapControllers()
    .RequireRateLimiting("FixedPolicy");

app.Run();