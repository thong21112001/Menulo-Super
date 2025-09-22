using Menulo.Application;
using Menulo.Configuration;
using Menulo.Infrastructure;
using Menulo.Infrastructure.Data;
using Microsoft.OpenApi.Models;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

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

//2. DI cho hạ tầng & ứng dụng
builder.Services.AddInfrastructureServices(builder.Configuration); // gọi từ Menulo.Infrastructure
builder.Services.AddApplicationServices(); // gọi từ Menulo.Application

//3. Đăng ký Controllers với tùy chọn JSON
builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
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
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Menulo API", Version = "v1" });
});

//6. Cấu hình cookie, seesion, cache
builder.Services.AddAuthenticationServices();

// 7.3) (tuỳ chọn) Chính sách role/claim
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("SuperAdminOnly", p => p.RequireRole("superadmin"));
    options.AddPolicy("HasRestaurant", p => p.RequireClaim("RestaurantId"));
});

var app = builder.Build();

//7. Middleware pipeline (điều chỉnh thứ tự Session)
app.ConfigureMiddleware(app.Environment);

app.MapRazorPages();
app.MapControllers();

//8. Khởi tạo dữ liệu ban đầu
DbInitializer.Initialize(app.Services);

app.Run();
