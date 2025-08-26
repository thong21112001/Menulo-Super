using Menulo.Configuration;
using Menulo.Infrastructure;
using Menulo.Infrastructure.Data;

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

//2. Đăng ký chuỗi connect và Cấu hình Identity
builder.Services.AddInfrastructureServices(builder.Configuration); // gọi từ Menulo.Infrastructure

//3. Cấu hình cookie, seesion, cache
builder.Services.AddAuthenticationServices();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();

//4. Khởi tạo dữ liệu ban đầu
DbInitializer.Initialize(app.Services);

app.Run();
