using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Drive.v3.Data;
using Google.Apis.Services;
using Menulo.Application.Common.Interfaces;
using Microsoft.Extensions.Options;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace Menulo.Infrastructure.External.Storage
{
    public class GoogleDriveImageStorageService : IImageStorageService
    {
        private readonly DriveService _drive;
        private readonly GoogleDriveOptions _opt;

        public GoogleDriveImageStorageService(IOptions<GoogleDriveOptions> opt)
        {
            _opt = opt.Value;

            var credential = GoogleCredential.FromFile(_opt.ServiceAccountJsonPath)
                .CreateScoped(DriveService.ScopeConstants.Drive);

            _drive = new DriveService(new BaseClientService.Initializer
            {
                HttpClientInitializer = credential,
                ApplicationName = "Menulo Image Storage"
            });
        }

        public async Task<string> UploadAsync(Stream content, string fileName, string contentType, int restaurantId, string restaurantName, string logicalName)
        {
            var folderId = await EnsureRestaurantFolderAsync(restaurantId, restaurantName);

            var fileMeta = new Google.Apis.Drive.v3.Data.File
            {
                Name = BuildFileName(fileName, logicalName),
                Parents = new List<string> { folderId }
            };

            var req = _drive.Files.Create(fileMeta, content, contentType);
            req.Fields = "id";
            var res = await req.UploadAsync();
            if (res.Status != Google.Apis.Upload.UploadStatus.Completed)
                throw new Exception($"Upload failed: {res.Exception?.Message}");

            var created = req.ResponseBody!;
            // make public
            await _drive.Permissions.Create(new Permission { Type = "anyone", Role = "reader" }, created.Id).ExecuteAsync();
            return $"https://drive.google.com/uc?id={created.Id}";
        }

        public async Task<string> UploadAndReplaceAsync(Stream content, string fileName, string contentType, int restaurantId, string restaurantName, string logicalName, string? oldPublicUrl)
        {
            var url = await UploadAsync(content, fileName, contentType, restaurantId, restaurantName, logicalName);
            if (!string.IsNullOrWhiteSpace(oldPublicUrl))
            {
                try { await DeleteByPublicUrlAsync(oldPublicUrl); } catch { /* log if needed */ }
            }
            return url;
        }

        public async Task DeleteByPublicUrlAsync(string publicUrl)
        {
            var id = ExtractFileId(publicUrl);
            if (string.IsNullOrWhiteSpace(id)) return;
            await _drive.Files.Delete(id).ExecuteAsync();
        }

        // ===== helpers =====
        private async Task<string> EnsureRestaurantFolderAsync(int id, string name)
        {
            string folderName = $"{id}_{ToSlug(name)}";
            var list = _drive.Files.List();
            list.Q = $"mimeType='application/vnd.google-apps.folder' and name='{folderName}' and '{_opt.RootFolderId}' in parents and trashed=false";
            list.Spaces = "drive";
            list.Fields = "files(id,name)";
            var result = await list.ExecuteAsync();
            var existed = result.Files?.FirstOrDefault();
            if (existed != null) return existed.Id;

            var meta = new Google.Apis.Drive.v3.Data.File
            {
                Name = folderName,
                MimeType = "application/vnd.google-apps.folder",
                Parents = new List<string> { _opt.RootFolderId }
            };
            var create = _drive.Files.Create(meta);
            create.Fields = "id";
            var folder = await create.ExecuteAsync();
            return folder.Id;
        }

        private static string BuildFileName(string original, string logicalName)
        {
            var ext = Path.GetExtension(original);
            return $"{logicalName}{ext}";
        }

        private static string ToSlug(string input)
        {
            string normalized = input.Normalize(NormalizationForm.FormD);
            var sb = new System.Text.StringBuilder();
            foreach (var c in normalized)
            {
                if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                    sb.Append(char.IsLetterOrDigit(c) ? char.ToLowerInvariant(c) : '-');
            }
            return string.Join('-', sb.ToString()
                .Replace("--", "-")
                .Trim('-')
                .Split('-', StringSplitOptions.RemoveEmptyEntries));
        }

        private static string? ExtractFileId(string url)
        {
            var m1 = Regex.Match(url, @"[?&]id=([^&]+)");
            if (m1.Success) return m1.Groups[1].Value;
            var m2 = Regex.Match(url, @"/file/d/([^/]+)/");
            if (m2.Success) return m2.Groups[1].Value;
            return null;
        }
    }
}
