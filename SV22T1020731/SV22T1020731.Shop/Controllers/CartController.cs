using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using SV22T1020731.Models.Sales;

namespace SV22T1020731.Shop.Controllers
{
    public class CartController : Controller
    {
        private const string CART = "CART";

        // 🛒 Lấy giỏ
        private List<OrderDetail> GetCart()
        {
            var data = HttpContext.Session.GetString(CART);
            if (string.IsNullOrEmpty(data))
                return new List<OrderDetail>();

            return JsonSerializer.Deserialize<List<OrderDetail>>(data);
        }

        // 💾 Lưu giỏ
        private void SaveCart(List<OrderDetail> cart)
        {
            HttpContext.Session.SetString(CART, JsonSerializer.Serialize(cart));
        }

        // 2.3.2 XEM GIỎ
        public IActionResult Index()
        {
            return View(GetCart());
        }

        // 2.3.1 THÊM
        public IActionResult Add(int id)
        {
            var cart = GetCart();

            var item = cart.FirstOrDefault(x => x.ProductID == id);

            if (item == null)
            {
                cart.Add(new OrderDetail()
                {
                    ProductID = id,
                    Quantity = 1,
                    SalePrice = 100000 // demo
                });
            }
            else
            {
                item.Quantity++;
            }

            SaveCart(cart);
            return RedirectToAction("Index");
        }

        // 2.3.3 UPDATE
        public IActionResult Update(int id, int quantity)
        {
            var cart = GetCart();
            var item = cart.FirstOrDefault(x => x.ProductID == id);

            if (item != null)
                item.Quantity = quantity;

            SaveCart(cart);
            return RedirectToAction("Index");
        }

        // 2.3.4 XÓA
        public IActionResult Remove(int id)
        {
            var cart = GetCart();
            cart.RemoveAll(x => x.ProductID == id);

            SaveCart(cart);
            return RedirectToAction("Index");
        }

        // 2.3.5 XÓA HẾT
        public IActionResult Clear()
        {
            HttpContext.Session.Remove(CART);
            return RedirectToAction("Index");
        }
    }
}