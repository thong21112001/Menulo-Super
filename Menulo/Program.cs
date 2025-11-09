using Menulo.Api;
using Menulo.Application;
using Menulo.Configuration;
using Menulo.Infrastructure;
using Menulo.Infrastructure.Data;
using Menulo.Infrastructure.RealTime;
using Menulo.Routing;
using Menulo.Services;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpClient();

// 0) Routing chung (URL đẹp)
builder.Services.AddRouting(o =>
{
    o.LowercaseUrls = true;
    o.LowercaseQueryStrings = true;
    o.AppendTrailingSlash = false;
});

//1. Đăng ký Razor Pages + các quy ước phân quyền
builder.Services.AddRazorPages(options =>
{
    //Cho phép truy cập trang đăng nhập, đăng ký, privacy
    options.Conventions.AllowAnonymousToPage("/Identity/Account/Login");
    options.Conventions.AllowAnonymousToPage("/Identity/Account/Register");
    options.Conventions.AllowAnonymousToPage("/Privacy");
    options.Conventions.AllowAnonymousToPage("/About/Index");
    // Áp dụng Authorize cho tất cả các trang còn lại
    options.Conventions.AuthorizeFolder("/");
});

builder.Services.AddMenuloUiRoutes();

//2. DI cho hạ tầng & ứng dụng
builder.Services.AddInfrastructureServices(builder.Configuration); // gọi từ Menulo.Infrastructure
builder.Services.AddApplicationServices(); // gọi từ Menulo.Application

//Tối ưu xử lý ảnh (đăng ký DI)
builder.Services.AddScoped<IImageProcessingService, ImageProcessingService>();

//3. Đăng ký Controllers với tùy chọn JSON
builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

//4. Đăng ký CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigin",
        builder =>
        {
            builder
                .AllowAnyOrigin()
                .AllowAnyHeader()
                .AllowAnyMethod();
        });
});

//5. Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//6. Cấu hình cookie, seesion, cache
builder.Services.AddAuthenticationServices();

//7. (tuỳ chọn) Chính sách role/claim
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("SuperAdminOnly", p => p.RequireRole("superadmin"));
    options.AddPolicy("HasRestaurant", p => p.RequireClaim("RestaurantId"));
});

//8. API Versioning + Explorer
builder.Services.AddMenuloApiVersioning();

//9. Bơm cấu hình Swagger theo version (tạo doc v1, v2, ...)
builder.Services.AddSwaggerVersionedDocs();

var app = builder.Build();

//10. Middleware pipeline (điều chỉnh thứ tự Session)
app.ConfigureMiddleware(app.Environment);

app.MapRazorPages();
app.MapMenuloApi();
app.MapControllers();

app.MapHub<TableHub>("/tableHub");

//11. Khởi tạo dữ liệu ban đầu
DbInitializer.Initialize(app.Services);

app.Run();
