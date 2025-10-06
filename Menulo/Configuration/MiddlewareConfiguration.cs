using Asp.Versioning.ApiExplorer;

namespace Menulo.Configuration
{
    public static class MiddlewareConfiguration
    {
        public static IApplicationBuilder ConfigureMiddleware(this IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();

                // Swagger + UI theo từng API version
                app.UseSwagger();

                // Lấy provider từ DI đúng cách
                var provider = app.ApplicationServices.GetRequiredService<IApiVersionDescriptionProvider>();

                app.UseSwaggerUI(o =>
                {
                    foreach (var d in provider.ApiVersionDescriptions)
                    {
                        o.SwaggerEndpoint($"/swagger/{d.GroupName}/swagger.json",
                                          $"Menulo API {d.GroupName.ToUpper()}");
                    }
                });
            }
            else
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            // CORS nên đặt sau UseRouting, trước AuthZ
            app.UseCors("AllowSpecificOrigin");

            // (nếu có dùng Session) đảm bảo đã AddSession() ở Program/Startup
            app.UseSession();

            // AuthN/AuthZ
            app.UseAuthentication();
            app.UseAuthorization();

            return app;
        }
    }
}