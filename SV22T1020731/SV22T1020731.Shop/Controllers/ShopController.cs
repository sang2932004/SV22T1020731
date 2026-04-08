using Microsoft.AspNetCore.Mvc;
using SV22T1020731.BusinessLayers;
using SV22T1020731.Models.Catalog;

namespace SV22T1020731.Shop.Controllers
{
    public class ShopController : Controller
    {
        public async Task<IActionResult> Index(ProductSearchInput input)
        {
            var data = await CatalogDataService.ListProductsAsync(input);
            return View(data);
        }

        public async Task<IActionResult> Detail(int id)
        {
            var product = await CatalogDataService.GetProductAsync(id);
            return View(product);
        }
    }
}