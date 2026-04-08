using Microsoft.AspNetCore.Mvc;
using SV22T1020731.Models.Catalog;

namespace SV22T1020731.Shop.Controllers
{
    public class ProductController : Controller
    {
        // ===== DATA =====
        private List<Product> GetProducts()
        {
            return new List<Product>()
            {
                new Product { ProductID = 1, ProductName = "Áo thun", CategoryID = 1, Price = 100000, Photo = "a.jpg" },
                new Product { ProductID = 2, ProductName = "Quần jean", CategoryID = 2, Price = 200000, Photo = "b.jpg" },
                new Product { ProductID = 3, ProductName = "Giày thể thao", CategoryID = 3, Price = 300000, Photo = "c.jpg" },
                new Product { ProductID = 4, ProductName = "Áo sơ mi", CategoryID = 1, Price = 150000, Photo = "d.jpg" },
                new Product { ProductID = 5, ProductName = "Áo hoodie", CategoryID = 1, Price = 320000, Photo = "e.jpg" },
                new Product { ProductID = 6, ProductName = "Quần short", CategoryID = 2, Price = 150000, Photo = "f.jpg" },
                new Product { ProductID = 7, ProductName = "Giày Adidas", CategoryID = 3, Price = 600000, Photo = "g.jpg" },
                new Product { ProductID = 8, ProductName = "Áo polo", CategoryID = 1, Price = 220000, Photo = "h.jpg" }
            };
        }

        // ===== DANH SÁCH + SEARCH =====
        public IActionResult Index(string searchValue = "", int categoryId = 0, int minPrice = 0, int maxPrice = 0)
        {
            var products = GetProducts();

            // 🔍 2.2.3 Tìm theo tên
            if (!string.IsNullOrWhiteSpace(searchValue))
            {
                products = products
                    .Where(p => p.ProductName.ToLower().Contains(searchValue.ToLower()))
                    .ToList();
            }

            // 🔍 2.2.2 theo loại
            if (categoryId > 0)
            {
                products = products
                    .Where(p => p.CategoryID == categoryId)
                    .ToList();
            }

            // 🔍 2.2.4 theo giá
            if (minPrice > 0)
            {
                products = products
                    .Where(p => p.Price >= minPrice)
                    .ToList();
            }

            if (maxPrice > 0)
            {
                products = products
                    .Where(p => p.Price <= maxPrice)
                    .ToList();
            }

            return View(products);
        }

        // ===== 2.2.5 CHI TIẾT =====
        public IActionResult Details(int id)
        {
            var product = GetProducts().FirstOrDefault(p => p.ProductID == id);

            if (product == null)
                return RedirectToAction("Index");

            return View(product);
        }
    }
}