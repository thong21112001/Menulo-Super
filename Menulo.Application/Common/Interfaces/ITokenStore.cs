namespace Menulo.Application.Common.Interfaces
{
    public record StoredGoogleToken(string UserId, string RefreshToken);

    public interface ITokenStore
    {
        Task SaveAsync(StoredGoogleToken t);
        Task<StoredGoogleToken?> GetAsync(string userId);
    }
}
