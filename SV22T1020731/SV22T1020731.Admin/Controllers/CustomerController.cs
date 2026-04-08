using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SV22T1020731.Models.Common;
using SV22T1020731.Models.Partner;

namespace SV22T1020731.Admin.Controllers
{
    /// <summary>
    /// Cung cấp các chức năng quản lý dữ liệu liên quan đến khách hàng
    /// </summary>
    [Authorize]
    public class CustomerController : Controller
    {
        private const string CUSTOMER_SEARCH = "CustomerSearchInput";
        /// <summary>
        /// Nhập đầu vào tìm kiếm -> Hiển thị kết quả (Của Search)
        /// </summary>
        /// <returns></returns>
        public IActionResult Index()
        {
            try
            {
                var input = ApplicationContext.GetSessionData<PaginationSearchInput>(CUSTOMER_SEARCH);
                if (input == null)
                    input = new PaginationSearchInput()
                    {
                        Page = 1,
                        PageSize = ApplicationContext.PageSize,
                        SearchValue = ""
                    };

                return View(input);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "Hệ thống đang xảy ra lỗi, vui lòng thử lại sau");
                return RedirectToAction("Index", "Home");
            }
        }

        /// <summary>
        /// Tìm kiếm và trả về kết quả
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> Search(PaginationSearchInput input)
        {
            try
            {
                var result = await PartnerDataService.ListCustomersAsync(0, input);

                ApplicationContext.SetSessionData(CUSTOMER_SEARCH, input);

                return View(result);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "Hệ thống đang xảy ra lỗi, vui lòng thử lại sau");
                return RedirectToAction("Index");
            }
        }

        /// <summary>
        /// Bổ sung khách hàng
        /// </summary>
        /// <returns></returns>
        public IActionResult Create()
        {
            try
            {
                ViewBag.Title = "Bổ sung khách hàng";
                var model = new Customer()
                {
                    CustomerID = 0
                };
                return View("Edit", model);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "Hệ thống đang xảy ra lỗi, vui lòng thử lại sau");
                return RedirectToAction("Index");
            }
        }

        /// <summary>
        /// Cập nhật thông tin khách hàng
        /// </summary>
        /// <param name="id">Mã khách hàng cần cập nhật thông tin</param>
        /// <returns></returns>
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                ViewBag.Title = "Cập nhật thông tin khách hàng";
                var model = await PartnerDataService.GetCustomerAsync(id);
                if (model == null)
                    return RedirectToAction("Index");
                return View(model);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "Hệ thống đang xảy ra lỗi, vui lòng thử lại sau");
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        public async Task<IActionResult> SaveData(Customer data) //Binding dữ liệu
        {
            try
            {
                ViewBag.Title = data.CustomerID == 0 ? "Bổ sung khsach hàng" : "Cập nhật thông tin khách hàng";

                //Kiểm tra tính đúng của dữ liệu

                //Sử dụng ModelState để lưu thông tin lỗi và chuyển thông báo lỗi ra View
                //Giả thiết: Yêu cầu phải nhập tên, email và tỉnh thành

                if (string.IsNullOrWhiteSpace(data.CustomerName))
                    ModelState.AddModelError(nameof(data.CustomerName), "Vui lòng nhập tên khách hàng");

                if (string.IsNullOrWhiteSpace(data.Email))
                    ModelState.AddModelError(nameof(data.Email), "Email không được để trống");
                else if (!await PartnerDataService.ValidatelCustomerEmailAsync(data.Email, data.CustomerID))
                    ModelState.AddModelError(nameof(data.Email), "Email đã được khách hàng khác dùng");

                if (string.IsNullOrWhiteSpace(data.Province))
                    ModelState.AddModelError(nameof(data.Province), "Vui lòng chọn tỉnh/thành phố");

                //Nếu dữ liệu không hợp lệ thì trả lại cho view để nhập lại
                if (!ModelState.IsValid)
                    return View("Edit", data);

                //(TUỲ CHỌN) Hiệu chỉnh dữ liệu theo quy định của hệ thống
                if (string.IsNullOrWhiteSpace(data.ContactName)) data.ContactName = data.CustomerName;
                if (string.IsNullOrEmpty(data.Phone)) data.Phone = "";
                if (string.IsNullOrEmpty(data.Address)) data.Address = "";

                //Lưu dữ liệu vào CSDL
                if (data.CustomerID == 0)
                    await PartnerDataService.AddCustomerAsync(data);
                else
                    await PartnerDataService.UpdateCustomerAsync(data);

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("error", "Hệ thống tạm thời đang bận, vui lòng thử lại sau vài ngày");
                return View("Edit", data);
            }

        }

        /// <summary>
        /// Xóa khách hàng
        /// </summary>
        /// <param name="id">Mã khách hàng cần xóa</param>
        /// <returns></returns>
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                if (Request.Method == "POST")
                {
                    await PartnerDataService.DeleteCustomerAsync(id);
                    return RedirectToAction("Index");
                }


                //GET
                var model = await PartnerDataService.GetCustomerAsync(id);
                if (model == null)
                    return RedirectToAction("Index");

                ViewBag.CanDelete = !await PartnerDataService.IsUsedCustomerAsync(id);

                return View(model);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "Hệ thống đang xảy ra lỗi, vui lòng thử lại sau");
                return RedirectToAction("Index");
            }
        }

        /// <summary>
        /// Thay đổi mật khẩu cho account khách hàng
        /// </summary>
        /// <param name="id">Mã khách hàng có account cần thay đổi mật khẩu</param>
        /// <returns></returns>
        public IActionResult ChangePassword(int id)
        {
            try
            {
                ViewBag.Title = "Thay đổi mật khẩu";
                return View();
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "Hệ thống đang xảy ra lỗi, vui lòng thử lại sau");
                return RedirectToAction("Index");
            }
        }

    }
}