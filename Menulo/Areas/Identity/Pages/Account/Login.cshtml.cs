// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using Menulo.Infrastructure.Identity;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace Menulo.Areas.Identity.Pages.Account
{
    public class LoginModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;

        public LoginModel(SignInManager<ApplicationUser> signInManager, 
            UserManager<ApplicationUser> userManager)
        {
            _signInManager = signInManager;
            _userManager = userManager;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public string ReturnUrl { get; set; }

        [TempData]
        public string ErrorMessage { get; set; }

        public class InputModel
        {
            [Required]
            [Display(Name = "Username")]
            public string Username { get; set; }

            [Required]
            [DataType(DataType.Password)]
            public string Password { get; set; }

            [Display(Name = "Ghi nhớ đăng nhập?")]
            public bool RememberMe { get; set; }
        }

        public async Task OnGetAsync(string returnUrl = null)
        {
            if (!string.IsNullOrEmpty(ErrorMessage))
            {
                ModelState.AddModelError(string.Empty, ErrorMessage);
            }

            returnUrl ??= Url.Content("~/");

            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

            ReturnUrl = returnUrl;
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");

            if (ModelState.IsValid)
            {
                var result = await _signInManager.PasswordSignInAsync(Input.Username, Input.Password, Input.RememberMe, lockoutOnFailure: false);

                if (result.Succeeded)
                {
                    // ***** THÊM CLAIM *****

                    // Lấy thông tin user đầy đủ
                    var user = await _userManager.FindByNameAsync(Input.Username);
                    if (user == null)
                    {
                        // Trường hợp hiếm: đăng nhập thành công nhưng không tìm thấy user
                        // (có thể do lỗi db), đăng xuất cho an toàn
                        await _signInManager.SignOutAsync();
                        ModelState.AddModelError(string.Empty, "Lỗi không xác định.");
                        return Page();
                    }

                    // Xóa Claim "RestaurantId" CŨ (nếu có)
                    // (Phòng trường hợp superadmin đổi nhà hàng cho user này)
                    var oldClaims = await _userManager.GetClaimsAsync(user);
                    var oldRestaurantClaim = oldClaims.FirstOrDefault(c => c.Type == "RestaurantId");
                    if (oldRestaurantClaim != null)
                    {
                        await _userManager.RemoveClaimAsync(user, oldRestaurantClaim);
                    }

                    // Thêm Claim "RestaurantId" MỚI (nếu user này có)
                    if (user.RestaurantId.HasValue)
                    {
                        var newRestaurantClaim = new Claim("RestaurantId", user.RestaurantId.Value.ToString());
                        await _userManager.AddClaimAsync(user, newRestaurantClaim);
                    }

                    // Làm mới cookie đăng nhập. Để cookie MỚI chứa các claim vừa thêm
                    await _signInManager.RefreshSignInAsync(user);

                    return LocalRedirect(returnUrl);
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Tên đăng nhập không tồn tại hoặc mật khẩu không đúng.");
                    return Page();
                }
            }

            return Page();
        }
    }
}
