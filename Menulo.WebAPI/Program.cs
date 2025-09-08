using Menulo.Application;
using Menulo.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

//1. Controllers + Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//2. DI cho hạ tầng & ứng dụng
builder.Services.AddInfrastructureServices(builder.Configuration);
builder.Services.AddApplicationServices();

//3. 
builder.Services.AddCors
(options =>
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

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseRouting();
app.UseCors("AllowSpecificOrigin");

app.UseAuthorization();

app.MapControllers();

app.Run();
