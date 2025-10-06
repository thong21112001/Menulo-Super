using Asp.Versioning.ApiExplorer;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Menulo.Api
{
    /// <summary>
    /// Tự động tạo 1 SwaggerDoc cho mỗi API version (v1, v2, ...)
    /// </summary>
    public sealed class ConfigureSwaggerOptions : IConfigureOptions<SwaggerGenOptions>
    {
        private readonly IApiVersionDescriptionProvider _provider;
        public ConfigureSwaggerOptions(IApiVersionDescriptionProvider provider) => _provider = provider;

        public void Configure(SwaggerGenOptions options)
        {
            foreach (var desc in _provider.ApiVersionDescriptions)
            {
                options.SwaggerDoc(desc.GroupName, new OpenApiInfo
                {
                    Title = "Menulo API",
                    Version = desc.ApiVersion.ToString(),
                    Description = desc.IsDeprecated ? "This API version has been deprecated." : null
                });
            }
        }
    }

    public static class SwaggerExtensions
    {
        public static IServiceCollection AddSwaggerVersionedDocs(this IServiceCollection services)
        {
            services.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>();
            return services;
        }
    }
}
