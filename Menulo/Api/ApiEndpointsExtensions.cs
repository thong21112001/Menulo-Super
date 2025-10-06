namespace Menulo.Api
{
    public static class ApiEndpointsExtensions
    {
        public static IEndpointRouteBuilder MapMenuloApi(this IEndpointRouteBuilder app)
        {
            // Nhóm URL gốc /api/v1 ...; Controllers sẽ tự match theo [Route] của chúng.
            var v1 = app.MapGroup("/api/v1");

            // Ví dụ nếu sau này bạn thêm Minimal API:
            // var restaurants = v1.MapGroup("/restaurants");
            // restaurants.MapGet("/", () => Results.Ok("list"));
            // restaurants.MapGet("/{id:int}", (int id) => Results.Ok(id));

            return app;
        }
    }
}
