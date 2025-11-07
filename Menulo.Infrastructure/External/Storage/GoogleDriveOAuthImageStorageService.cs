using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2.Responses;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Menulo.Application.Common.Interfaces;
using Menulo.Application.Common.Options;
using Microsoft.Extensions.Options;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace Menulo.Infrastructure.External.Storage
{
    public class GoogleDriveOAuthImageStorageService : IImageStorageService
    {
        private readonly GoogleOAuthOptions _opt;
        private readonly ITokenStore _store;
        private DriveService _drive = default!;

        public GoogleDriveOAuthImageStorageService(IOptions<GoogleOAuthOptions> opt, 
            ITokenStore store)
        {
            _opt = opt.Value; _store = store;
        }

        private async Task EnsureDriveAsync()
        {
            if (_drive != null) return;
            var saved = await _store.GetAsync("owner")
                        ?? throw new InvalidOperationException("Chưa có refresh token. Mở /google/oauth2/start để cấp quyền.");
            var flow = new GoogleAuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer
            {
                ClientSecrets = new ClientSecrets { ClientId = _opt.ClientId, ClientSecret = _opt.ClientSecret },
                Scopes = new[] { DriveService.Scope.DriveFile }
            });

            var cred = new UserCredential(flow, "owner", new TokenResponse { RefreshToken = saved.RefreshToken });

            var appName = string.IsNullOrWhiteSpace(_opt.ApplicationName)
                            ? "Menulo Image Uploader"  // fallback để chắc ăn
                            : _opt.ApplicationName;

            _drive = new DriveService(new BaseClientService.Initializer
            {
                HttpClientInitializer = cred,
                ApplicationName = appName   // <-- BẮT BUỘC có giá trị
            });
        }

        public async Task<string> UploadAsync(Stream content, string fileName, string contentType,
            int restaurantId, string restaurantName, string logicalName)
        {
            await EnsureDriveAsync();
            if (content.CanSeek) content.Position = 0;

            var folderId = await EnsureRestaurantFolderAsync(restaurantId, restaurantName);

            var meta = new Google.Apis.Drive.v3.Data.File
            {
                Name = $"{logicalName}{Path.GetExtension(fileName)}",
                Parents = new List<string> { folderId },
                MimeType = contentType // <-- đảm bảo Drive biết đây là image/jpeg, image/png, ...
            };

            var req = _drive.Files.Create(meta, content, contentType);
            req.Fields = "id";
            var res = await req.UploadAsync();
            if (res.Status != Google.Apis.Upload.UploadStatus.Completed)
                throw new Exception($"Upload failed: {res.Exception?.Message}");

            var created = req.ResponseBody!;
            await _drive.Permissions.Create(new Google.Apis.Drive.v3.Data.Permission { Type = "anyone", Role = "reader" }, created.Id).ExecuteAsync();
            // Trả URL dạng export=download để lấy raw bytes dễ dàng
            return $"https://drive.google.com/uc?export=download&id={created.Id}";
        }

        public async Task<string> UploadAndReplaceAsync(Stream content, string fileName, string contentType,
            int restaurantId, string restaurantName, string logicalName, string? oldPublicUrl)
        {
            var url = await UploadAsync(content, fileName, contentType, restaurantId, restaurantName, logicalName);
            if (!string.IsNullOrWhiteSpace(oldPublicUrl))
                try { await DeleteByPublicUrlAsync(oldPublicUrl); } catch { /* log */ }
            return url;
        }

        public async Task DeleteByPublicUrlAsync(string publicUrl)
        {
            await EnsureDriveAsync();
            var id = ExtractFileId(publicUrl);
            if (string.IsNullOrWhiteSpace(id)) return;
            await _drive.Files.Delete(id).ExecuteAsync();
        }

        private async Task<string> EnsureRestaurantFolderAsync(int id, string name)
        {
            await EnsureDriveAsync();
            var folderName = $"{id}_{ToSlug(name)}";

            var list = _drive.Files.List();
            list.Q = string.IsNullOrWhiteSpace(_opt.RootFolderId)
                ? $"mimeType='application/vnd.google-apps.folder' and name='{folderName}' and trashed=false"
                : $"mimeType='application/vnd.google-apps.folder' and name='{folderName}' and '{_opt.RootFolderId}' in parents and trashed=false";
            list.Fields = "files(id,name)";
            var existed = (await list.ExecuteAsync()).Files?.FirstOrDefault();
            if (existed != null) return existed.Id;

            var meta = new Google.Apis.Drive.v3.Data.File
            {
                Name = folderName,
                MimeType = "application/vnd.google-apps.folder",
                Parents = string.IsNullOrWhiteSpace(_opt.RootFolderId) ? null : new List<string> { _opt.RootFolderId! }
            };
            var create = _drive.Files.Create(meta);
            create.Fields = "id";
            var folder = await create.ExecuteAsync();
            return folder.Id;
        }

        private static string? ExtractFileId(string url)
        {
            var m1 = Regex.Match(url, @"[?&]id=([^&]+)"); if (m1.Success) return m1.Groups[1].Value;
            var m2 = Regex.Match(url, @"/file/d/([^/]+)/"); if (m2.Success) return m2.Groups[1].Value;
            return null;
        }

        private static string ToSlug(string input)
        {
            string normalized = input.Normalize(NormalizationForm.FormD);
            var sb = new System.Text.StringBuilder();
            foreach (var c in normalized)
                if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                    sb.Append(char.IsLetterOrDigit(c) ? char.ToLowerInvariant(c) : '-');
            return string.Join('-', sb.ToString().Replace("--", "-").Trim('-').Split('-', StringSplitOptions.RemoveEmptyEntries));
        }
    }
}
