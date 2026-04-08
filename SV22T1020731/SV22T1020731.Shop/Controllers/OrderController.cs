using Microsoft.AspNetCore.Mvc;

namespace SV22T1020731.Shop.Controllers
{
    public class OrderController : Controller
    {
        // 2.3.6 ĐẶT HÀNG
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(string customerName, string address)
        {
            // demo lưu đơn
            TempData["msg"] = "Đặt hàng thành công!";
            return RedirectToAction("History");
        }

        // 2.3.7 + 2.3.8
        public IActionResult History()
        {
            return View();
        }
    }
}