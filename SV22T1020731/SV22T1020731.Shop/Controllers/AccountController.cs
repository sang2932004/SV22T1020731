using Microsoft.AspNetCore.Mvc;
using SV22T1020731.Models.Partner;

namespace SV22T1020731.Shop.Controllers
{
    public class AccountController : Controller
    {
        // ===== THÔNG TIN CÁ NHÂN =====
        public IActionResult Profile()
        {
            // 🔥 giả lập user (sau này lấy từ session)
            var user = new Customer()
            {
                CustomerName = "Nguyễn Văn A",
                Email = "a@gmail.com",
                Phone = "0123456789",
                Address = "Huế"
            };

            return View(user);
        }

        [HttpPost]
        public IActionResult Profile(Customer model)
        {
            if (ModelState.IsValid)
            {
                // TODO: lưu DB
                ViewBag.Success = "Cập nhật thành công!";
            }

            return View(model);
        }

        // ===== ĐỔI MẬT KHẨU =====
        public IActionResult ChangePassword()
        {
            return View();
        }

        [HttpPost]
        public IActionResult ChangePassword(string oldPassword, string newPassword, string confirmPassword)
        {
            if (newPassword != confirmPassword)
            {
                ViewBag.Error = "Mật khẩu xác nhận không đúng!";
                return View();
            }

            // TODO: kiểm tra mật khẩu cũ + lưu DB
            ViewBag.Success = "Đổi mật khẩu thành công!";
            return View();
        }
    }
}