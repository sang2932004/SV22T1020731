using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SV22T1020731.BusinessLayers;

namespace SV22T1020731.Admin.Controllers
{
    /// <summary>
    /// Cung cấp các chức năng quản lý liên quan đến tài khoản người dùng
    /// </summary>
    [Authorize]
    public class AccountController : Controller
    {
        /// <summary>
        /// Đăng nhập tài khoản người dùng
        /// </summary>
        [AllowAnonymous]
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        /// <summary>
        /// Xử lý đăng nhập
        /// </summary>
        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> Login(string username, string password)
        {
            try
            {

                ViewBag.UserName = username;

                // Kiểm tra dữ liệu đầu vào
                if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
                {
                    ModelState.AddModelError("Error", "Vui lòng nhập đầy đủ tên đăng nhập và mật khẩu.");
                    return View();
                }

                // Mã hoá MD5 mật khẩu 
                string hashedPassword = CryptHelper.HashMD5(password);

                // Kiểm tra tài khoản từ database
                var userAccount = await SecurityDataService.AuthorizeAsync(username, hashedPassword);
                if (userAccount == null)
                {
                    ModelState.AddModelError("Error", "Tên đăng nhập hoặc mật khẩu không đúng.");
                    return View();
                }

                // Chặn tài khoản Customer vào Admin
                if (userAccount.RoleNames == "Customer")
                {
                    ModelState.AddModelError("Error", "Tài khoản không có quyền truy cập hệ thống quản trị.");
                    return View();
                }

                // Tạo thông tin user
                var userData = new WebUserData()
                {
                    UserId = userAccount.UserId,
                    UserName = userAccount.UserName,
                    DisplayName = userAccount.DisplayName,
                    Email = userAccount.Email,
                    Photo = userAccount.Photo,
                    Roles = userAccount.RoleNames.Split(',').ToList()
                };

                // Tạo principal (chứng thực)
                var principal = userData.CreatePrincipal();

                // Đăng nhập (ghi cookie)
                await HttpContext.SignInAsync(principal);

                // Chuyển về trang chủ
                return RedirectToAction("Index", "Home");
            }
            catch
            {
                ModelState.AddModelError(string.Empty, "Hệ thống đang xảy ra lỗi, vui lòng thử lại sau");
                return View();
            }
        }

        /// <summary>
        /// Đăng xuất tài khoản
        /// </summary>
        public async Task<IActionResult> Logout()
        {
            try
            {
                HttpContext.Session.Clear();
                await HttpContext.SignOutAsync();
                return RedirectToAction("Login");
            }
            catch
            {
                ModelState.AddModelError(string.Empty, "Hệ thống đang xảy ra lỗi, vui lòng thử lại sau");
                return RedirectToAction("Login");
            }
        }

        /// <summary>
        /// Đổi mật khẩu
        /// </summary>
        public IActionResult ChangePassword()
        {
            try
            {
                return View();
            }
            catch
            {
                ModelState.AddModelError(string.Empty, "Hệ thống đang xảy ra lỗi, vui lòng thử lại sau");
                return RedirectToAction("Login");
            }
        }

        /// <summary>
        /// Không có quyền truy cập
        /// </summary>
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}