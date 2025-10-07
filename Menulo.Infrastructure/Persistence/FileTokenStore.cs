using Menulo.Application.Common.Interfaces;

namespace Menulo.Infrastructure.Persistence
{
    public class FileTokenStore : ITokenStore
    {
        private readonly string _path = Path.Combine(AppContext.BaseDirectory, "google_token.secret.json");

        public async Task SaveAsync(StoredGoogleToken t) =>
            await System.IO.File.WriteAllTextAsync(_path, System.Text.Json.JsonSerializer.Serialize(t));

        public async Task<StoredGoogleToken?> GetAsync(string userId)
        {
            if (!System.IO.File.Exists(_path)) return null;
            var json = await System.IO.File.ReadAllTextAsync(_path);
            var obj = System.Text.Json.JsonSerializer.Deserialize<StoredGoogleToken>(json);
            return obj?.UserId == userId ? obj : null;
        }
    }
}
