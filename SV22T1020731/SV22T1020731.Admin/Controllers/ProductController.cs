using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SV22T1020731.BusinessLayers;
using SV22T1020731.Models.Catalog;
using SV22T1020731.Models.Common;

namespace SV22T1020731.Admin.Controllers
{
    /// <summary>
    /// Cung cấp các chức năng quản lý dữ liệu liên quan đến mặt hàng
    /// </summary>
    [Authorize]
    public class ProductController : Controller
    {
        private const string PRODUCT_SEARCH = "ProductSearchInput";
        /// <summary>
        /// Tìm kiếm và hiển thị danh sách sản phẩm
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> Index()
        {
            try
            {
                var input = ApplicationContext.GetSessionData<ProductSearchInput>(PRODUCT_SEARCH);

                if (input == null)
                {
                    input = new ProductSearchInput()
                    {
                        Page = 1,
                        PageSize = ApplicationContext.PageSize,
                        SearchValue = "",
                        CategoryID = 0,
                        SupplierID = 0,
                        MinPrice = 0,
                        MaxPrice = 0
                    };
                }
                ViewBag.Suppliers = await PartnerDataService.ListSuppliersAsync(new PaginationSearchInput());
                ViewBag.Categories = await CatalogDataService.ListCategoriesAsync(new PaginationSearchInput());
                return View(input);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "Hệ thống đang xảy ra lỗi, vui lòng thử lại sau");
                return RedirectToAction("Index", "Home");
            }
        }

        public async Task<IActionResult> Search(ProductSearchInput input)
        {
            try
            {
                var result = await CatalogDataService.ListProductsAsync(input);

                // 🔥 FIX LỖI: ÉP KIỂU
                var data = result.Data as List<Product>;

                if (data != null)
                {
                    if (input.MinPrice > 0)
                        data = data.Where(p => p.Price >= input.MinPrice).ToList();

                    if (input.MaxPrice > 0)
                        data = data.Where(p => p.Price <= input.MaxPrice).ToList();

                    result.Data = data;
                }

                ApplicationContext.SetSessionData(PRODUCT_SEARCH, input);
                return View(result);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "Hệ thống đang xảy ra lỗi, vui lòng thử lại sau");
                return RedirectToAction("Index");
            }
        }

        /// <summary>
        /// Tạo mới sản phẩm
        /// </summary>
        /// <returns></returns>
        public IActionResult Create()
        {
            try
            {
                ViewBag.Title = "Thêm sản phẩm";
                var model = new Product()
                {
                    ProductID = 0
                };
                return View("Edit", model);
            }
            catch
            {
                ModelState.AddModelError(string.Empty, "Hệ thống đang xảy ra lỗi, vui lòng thử lại sau");
                return RedirectToAction("Index");
            }
        }

        /// <summary>
        /// Chỉnh sửa thông tin sản phẩm
        /// </summary>
        /// <param name="id">Mã sản phẩm cần thay dổi thông tin</param>
        /// <returns></returns>
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                //var product = await CatalogDataService.GetProductAsync(id);
                ViewBag.Photos = await CatalogDataService.ListPhotosAsync(id);
                ViewBag.Attributes = await CatalogDataService.ListAttributesAsync(id);
                var model = await CatalogDataService.GetProductAsync(id);
                if (model == null)
                    return RedirectToAction("Index");
                return View(model);
            }
            catch
            {
                ModelState.AddModelError(string.Empty, "Hệ thống đang xảy ra lỗi, vui lòng thử lại sau");
                return RedirectToAction("Index");
            }
        }

        /// <summary>
        /// Xóa một sản phẩm
        /// </summary>
        /// <param name="id">Mã sản phẩm cần xóa</param>
        /// <returns></returns>
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                if (Request.Method == "POST")
                {
                    await CatalogDataService.DeleteProductAsync(id);
                    return RedirectToAction("Index");
                }


                //GET
                var model = await CatalogDataService.GetProductAsync(id);
                if (model == null)
                    return RedirectToAction("Index");

                ViewBag.CanDelete = !await CatalogDataService.IsUsedProductAsync(id);

                return View(model);
            }
            catch
            {
                ModelState.AddModelError(string.Empty, "Hệ thống đang xảy ra lỗi, vui lòng thử lại sau");
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        public async Task<IActionResult> SaveData(Product data, IFormFile? uploadPhoto) //Binding dữ liệu
        {
            try
            {
                ViewBag.Title = data.ProductID == 0 ? "Bổ sung mặt hàng" : "Cập nhật thông tin mặt hàng";

                //Kiểm tra tính đúng của dữ liệu

                //Sử dụng ModelState để lưu thông tin lỗi và chuyển thông báo lỗi ra View
                //Giả thiết: Yêu cầu phải nhập tên, email và tỉnh thành

                if (string.IsNullOrWhiteSpace(data.ProductName))
                    ModelState.AddModelError(nameof(data.ProductName), "Vui lòng nhập tên mặt hàng");

                if (string.IsNullOrWhiteSpace(data.Unit))
                    ModelState.AddModelError(nameof(data.Unit), "Vui lòng nhập đơn vị");

                if (data.Price < 0)
                    ModelState.AddModelError(nameof(data.Price), "Vui lòng nhập giá tiền");

                //Nếu dữ liệu không hợp lệ thì trả lại cho view để nhập lại
                if (!ModelState.IsValid)
                    return View("Edit", data);

                //Xử lý upload ảnh
                if (uploadPhoto != null)
                {
                    var fileName = $"{Guid.NewGuid()}{Path.GetExtension(uploadPhoto.FileName)}";
                    var filePath = Path.Combine(ApplicationContext.WWWRootPath, "images/products", fileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await uploadPhoto.CopyToAsync(stream);
                    }
                    data.Photo = fileName;
                };

                //Lưu dữ liệu vào CSDL
                if (data.ProductID == 0)
                    await CatalogDataService.AddProductAsync(data);
                else
                    await CatalogDataService.UpdateProductAsync(data);

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                //Ghi log lỗi với các thông tin nằm trong exception
                //ex.Message
                //ex.StackTrace
                ModelState.AddModelError("error", "Hệ thống tạm thời đang bận, vui lòng thử lại sau vài ngày");
                return View("Edit", data);
            }

        }

        // ================== ATTRIBUTES ==================

        /// <summary>
        /// Hiển thị danh sách thuộc tính của mặt hàng
        /// </summary>
        /// <param name="id">Mã mặt hàng cần hiển thị thuộc tính</param>
        /// <returns></returns>
        public async Task<IActionResult> ListAttributes(int id, PaginationSearchInput input)
        {
            try
            {
                var result = await CatalogDataService.ListAttributesAsync(id);

                return View(result);
            }
            catch
            {
                ModelState.AddModelError(string.Empty, "Hệ thống đang xảy ra lỗi, vui lòng thử lại sau");
                return RedirectToAction("Index");
            }
        }

        /// <summary>
        /// Bổ sung thuộc tính mới cho sản phẩm
        /// </summary>
        /// <param name="id">Mã mặt hàng cần bổ sung thuộc tính</param>
        /// <returns></returns>
        public IActionResult CreateAttribute(int id)
        {
            try
            {
                ViewBag.Title = "Thêm thuộc tính mặt hàng";
                var model = new ProductAttribute() 
                { 
                    ProductID = id, 
                    DisplayOrder = 1 
                };
                return View("EditAttribute", model);
            }
            catch
            {
                ModelState.AddModelError(string.Empty, "Hệ thống đang xảy ra lỗi, vui lòng thử lại sau");
                return RedirectToAction("Edit", new { id });
            }
        }

        /// <summary>
        /// Cập nhật thuộc tính của sản phẩm
        /// </summary>
        /// <param name="id">mã sản phẩm có thuộc tính cần thay đổi</param>
        /// <param name="attributeId">Mã thuộc tính cần cập nhật</param>
        /// <returns></returns>
        public async Task<IActionResult> EditAttribute(int id, int attributeId)
        {
            try
            {
                var model = await CatalogDataService.GetAttributeAsync(attributeId);
                if (model == null)
                    return RedirectToAction("Edit", new { id });
                return View("EditAttribute", model);
            }
            catch
            {
                ModelState.AddModelError(string.Empty, "Hệ thống đang xảy ra lỗi, vui lòng thử lại sau");
                return RedirectToAction("Edit", new { id });
            }
        }

        /// <summary>
        /// Xóa thuộc tính sản phẩm
        /// </summary>
        /// <param name="id">Mã sản phẩm có thuộc tính cần xóa</param>
        /// <param name="attributeId">Mã sản phẩm cần xóa</param>
        /// <returns></returns>
        public async Task<IActionResult> DeleteAttribute(int id, int attributeId)
        {
            try
            {
                if (Request.Method == "POST")
                {
                    await CatalogDataService.DeleteAttributeAsync(attributeId);
                    return RedirectToAction("Edit", new { id });
                }

                var model = await CatalogDataService.GetAttributeAsync(attributeId);
                if (model == null)
                    return RedirectToAction("Edit", new { id });

                ViewBag.Title = "Xóa thuộc tính mặt hàng";
                return View("DeleteAttribute", model);
            }
            catch
            {
                ModelState.AddModelError(string.Empty, "Hệ thống đang xảy ra lỗi, vui lòng thử lại sau");
                return RedirectToAction("Edit", new { id });
            }
        }

        [HttpPost]
        public async Task<IActionResult> SaveAttribute(ProductAttribute data)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(data.AttributeName))
                    ModelState.AddModelError(nameof(data.AttributeName), "Vui lòng nhập tên thuộc tính");
                if (string.IsNullOrWhiteSpace(data.AttributeValue))
                    ModelState.AddModelError(nameof(data.AttributeValue), "Vui lòng nhập giá trị thuộc tính");

                if (!ModelState.IsValid)
                {
                    ViewBag.Title = data.AttributeID == 0 ? "Thêm thuộc tính mặt hàng" : "Cập nhật thuộc tính mặt hàng";
                    return View("EditAttribute", data);
                }

                if (data.AttributeID == 0)
                    await CatalogDataService.AddAttributeAsync(data);
                else
                    await CatalogDataService.UpdateAttributeAsync(data);

                return RedirectToAction("Edit", new { id = data.ProductID, anchor = "attributes" });
            }
            catch
            {
                ModelState.AddModelError(string.Empty, "Hệ thống đang xảy ra lỗi, vui lòng thử lại sau");
                return View("EditAttribute", data);
            }
        }

        // ================== PHOTOS ==================

        /// <summary>
        /// Hiên thị danh sách ảnh của từng sản phẩm
        /// </summary>
        /// <param name="id">Mã sản phẩm cần hiển thị ảnh</param>
        /// <returns></returns>
        public async Task<IActionResult> ListPhotos(int id)
        {
            try
            {
                var result = await CatalogDataService.ListPhotosAsync(id);
                return View(result);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "Hệ thống đang xảy ra lỗi, vui lòng thử lại sau");
                return RedirectToAction("Index");
            }
        }

        /// <summary>
        /// Bổ sung ảnh mới cho sản phẩm
        /// </summary>
        /// <param name="id">Mã sản phẩm cần bổ sung ảnh</param>
        /// <returns></returns>
        public IActionResult CreatePhoto(int id)
        {
            try
            {
                ViewBag.Title = "Thêm hình ảnh mặt hàng";
                var model = new ProductPhoto() 
                { 
                    ProductID = id, 
                    DisplayOrder = 1 
                };
                return View("EditPhoto", model);
            }
            catch
            {
                ModelState.AddModelError(string.Empty, "Hệ thống đang xảy ra lỗi, vui lòng thử lại sau");
                return RedirectToAction("Edit", new { id });
            }
        }

        /// <summary>
        /// Cập nhật ảnh của sản phẩm
        /// </summary>
        /// <param name="id">Mã sản phẩm cần cập nhật ảnh</param>
        /// <param name="photoId">Mã ảnh cần cập nhật</param>
        /// <returns></returns>
        public async Task<IActionResult> EditPhoto(int id, int photoId)
        {
            try
            {
                ViewBag.Title = "Cập nhật hình ảnh mặt hàng";
                var model = await CatalogDataService.GetPhotoAsync(photoId);
                if (model == null)
                    return RedirectToAction("Edit", new { id });
                return View("EditPhoto", model);
            }
            catch
            {
                ModelState.AddModelError(string.Empty, "Hệ thống đang xảy ra lỗi, vui lòng thử lại sau");
                return RedirectToAction("Edit", new { id });
            }
        }

        /// <summary>
        /// Xóa ảnh của sản phẩm
        /// </summary>
        /// <param name="id">Mã sản phẩm có ảnh cần xóa</param>
        /// <param name="photoId">Mã ảnh cần xóa</param>
        /// <returns></returns>
        public async Task<IActionResult> DeletePhoto(int id, int photoId)
        {
            try
            {
                if (Request.Method == "POST")
                {
                    await CatalogDataService.DeletePhotoAsync(photoId);
                    return RedirectToAction("Edit", new { id });
                }

                var model = await CatalogDataService.GetPhotoAsync(photoId);
                if (model == null)
                    return RedirectToAction("Edit", new { id });

                ViewBag.Title = "Xóa hình ảnh mặt hàng";
                return View("DeletePhoto", model);
            }
            catch
            {
                ModelState.AddModelError(string.Empty, "Hệ thống đang xảy ra lỗi, vui lòng thử lại sau");
                return RedirectToAction("Edit", new { id });
            }
        }

        [HttpPost]
        public async Task<IActionResult> SavePhoto(ProductPhoto data, IFormFile? uploadPhoto)
        {
            try
            {
                if (uploadPhoto == null && data.PhotoID == 0)
                    ModelState.AddModelError("uploadPhoto", "Vui lòng chọn ảnh");

                if (!ModelState.IsValid)
                {
                    ViewBag.Title = data.PhotoID == 0 ? "Thêm hình ảnh mặt hàng" : "Cập nhật hình ảnh mặt hàng";
                    return View("EditPhoto", data);
                }

                if (uploadPhoto != null)
                {
                    var fileName = $"{Guid.NewGuid()}{Path.GetExtension(uploadPhoto.FileName)}";
                    var filePath = Path.Combine(ApplicationContext.WWWRootPath, "images/products", fileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                        await uploadPhoto.CopyToAsync(stream);
                    data.Photo = fileName;
                }

                if (data.PhotoID == 0)
                    await CatalogDataService.AddPhotoAsync(data);
                else
                    await CatalogDataService.UpdatePhotoAsync(data);

                return RedirectToAction("Edit", new { id = data.ProductID, anchor = "photos" });
            }
            catch
            {
                ModelState.AddModelError(string.Empty, "Hệ thống đang xảy ra lỗi, vui lòng thử lại sau");
                return View("EditPhoto", data);
            }
        }
    }
}