namespace Menulo.Application.Common.Interfaces
{
    // Một DTO để trả về kết quả từ Infrastructure - "IdentityResult"
    public class IdentityResultDto
    {
        public bool Succeeded { get; init; }
        public string UserId { get; init; } = string.Empty;
        public IEnumerable<string> Errors { get; init; } = Enumerable.Empty<string>();

        public static IdentityResultDto Success(string userId)
            => new() { Succeeded = true, UserId = userId };

        public static IdentityResultDto Failure(IEnumerable<string> errors)
            => new() { Succeeded = false, Errors = errors };
    }
}
