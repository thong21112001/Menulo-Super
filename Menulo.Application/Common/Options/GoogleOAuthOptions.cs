namespace Menulo.Application.Common.Options
{
    public class GoogleOAuthOptions
    {
        public string ClientId { get; set; } = "";
        public string ClientSecret { get; set; } = "";
        public string RedirectUri { get; set; } = "";
        public string? RootFolderId { get; set; }
        public string ApplicationName { get; set; } = "Menulo Image Uploader";
    }
}
