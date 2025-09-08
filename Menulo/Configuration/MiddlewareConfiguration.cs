namespace Menulo.Configuration
{
    public static class MiddlewareConfiguration
    {
        public static IApplicationBuilder ConfigureMiddleware(this IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Menulo API v1");
                    c.RoutePrefix = "swagger";
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

            app.UseCors("AllowSpecificOrigin");

            // Middleware để sử dụng Session
            app.UseSession();

            // Middleware xác thực và phân quyền
            app.UseAuthentication(); // Xác thực
            app.UseAuthorization();  // Phân quyền

            return app;
        }
    }
}
