using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Menulo.Application.Common.Interfaces;
using Menulo.Application.Common.Options;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;

namespace Menulo.Controllers
{
    [Route("google/oauth2")]
    public class GoogleAuthController : Controller
    {
        private static readonly string[] Scopes = { "https://www.googleapis.com/auth/drive.file" };
        private readonly GoogleOAuthOptions _opt;
        private readonly ITokenStore _store;


        public GoogleAuthController(IOptions<GoogleOAuthOptions> opt, ITokenStore store)
        {
            _opt = opt.Value; _store = store;
        }


        [HttpGet("start")]
        public IActionResult Start()
        {
            var flow = new GoogleAuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer
            {
                ClientSecrets = new ClientSecrets
                {
                    ClientId = _opt.ClientId,
                    ClientSecret = _opt.ClientSecret
                },
                Scopes = Scopes
                // KHÔNG set AccessType/Prompt ở đây vì bản 1.7x không hỗ trợ
            });

            // 1) Build URL gốc từ thư viện
            var baseUrl = flow.CreateAuthorizationCodeRequest(_opt.RedirectUri)
                              .Build()
                              .ToString();

            // 2) Phân tích query hiện có, thêm tham số cần thiết CHỈ MỘT LẦN
            var uri = new Uri(baseUrl);

            // Nếu có Microsoft.AspNetCore.WebUtilities:
            var query = QueryHelpers.ParseQuery(uri.Query);
            var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var kv in query)
            {
                // giữ lại giá trị đầu tiên (đủ dùng)
                if (kv.Value.Count > 0) dict[kv.Key] = kv.Value[0];
            }

            // ép chỉ còn 1 giá trị cho mỗi param để tránh “only have a single value”
            dict["access_type"] = "offline";

            // Một số phiên bản không hiểu `prompt`, bạn có thể thay bằng approval_prompt=force
            // Ưu tiên prompt=consent, nếu bạn đã thử mà không được thì đổi sang approval_prompt
            if (!dict.ContainsKey("prompt") && !dict.ContainsKey("approval_prompt"))
                dict["prompt"] = "consent"; // hoặc: dict["approval_prompt"] = "force";

            // (tuỳ chọn) cấp lại scope đã cấp
            if (!dict.ContainsKey("include_granted_scopes"))
                dict["include_granted_scopes"] = "true";

            // 3) Dựng lại URL sạch sẽ (không nhân đôi tham số)
            var origin = $"{uri.Scheme}://{uri.Host}{(uri.IsDefaultPort ? "" : $":{uri.Port}")}";
            var finalUrl = QueryHelpers.AddQueryString(origin + uri.AbsolutePath, dict);

            return Redirect(finalUrl);
        }


        [HttpGet("callback")]
        public async Task<IActionResult> Callback([FromQuery] string code)
        {
            var flow = new GoogleAuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer
            {
                ClientSecrets = new ClientSecrets { ClientId = _opt.ClientId, ClientSecret = _opt.ClientSecret },
                Scopes = Scopes
            });
            var token = await flow.ExchangeCodeForTokenAsync("owner", code, _opt.RedirectUri, CancellationToken.None);
            if (string.IsNullOrEmpty(token.RefreshToken))
                return Content("Không lấy được refresh token. Vào https://myaccount.google.com/permissions xoá quyền app, rồi thử lại.");
            await _store.SaveAsync(new StoredGoogleToken("owner", token.RefreshToken));
            return Content("OK: Refresh token đã lưu. Đóng tab này lại.");
        }
    }
}
