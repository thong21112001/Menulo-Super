using Asp.Versioning;

namespace Menulo.Api
{
    public static class ApiVersioningExtensions
    {
        public static IServiceCollection AddMenuloApiVersioning(this IServiceCollection services)
        {
            services
              .AddApiVersioning(o =>
              {
                  o.AssumeDefaultVersionWhenUnspecified = true;
                  o.DefaultApiVersion = new ApiVersion(1, 0);  // v1.0
                  o.ReportApiVersions = true;
                  o.ApiVersionReader = ApiVersionReader.Combine(
                      new UrlSegmentApiVersionReader(),                // /api/v1/...
                      new QueryStringApiVersionReader("api-version"),  // ?api-version=1.0
                      new HeaderApiVersionReader("x-api-version"));    // Header: x-api-version:1.0
              })
              .AddApiExplorer(o =>
              {
                  o.GroupNameFormat = "'v'VVV";          // v1, v1.1
                  o.SubstituteApiVersionInUrl = true;    // thay {version:apiVersion} trong route
              });

            return services;
        }
    }
}
