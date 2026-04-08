using Microsoft.AspNetCore.Authentication.Cookies;
using System.Globalization;
using SV22T1020731.BusinessLayers;

var builder = WebApplication.CreateBuilder(args);

// ================== SERVICES ==================
builder.Services.AddHttpContextAccessor();

builder.Services.AddDistributedMemoryCache();

// 🔥 SESSION (dùng cho giỏ hàng + login shop)
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// MVC
builder.Services.AddControllersWithViews()
    .AddMvcOptions(option =>
    {
        option.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true;
    });

// 🔥 AUTHENTICATION (SHOP)
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(option =>
    {
        option.Cookie.Name = "SV22T1020731.Shop";

        // 🔥 QUAN TRỌNG (fix lỗi login shop)
        option.LoginPath = "/Shop/Account/Login";
        option.AccessDeniedPath = "/Shop/Account/Login";

        option.ExpireTimeSpan = TimeSpan.FromDays(7);
    });

builder.Services.AddAuthorization();

var app = builder.Build();

// ================== MIDDLEWARE ==================
app.UseDeveloperExceptionPage();
app.UseStaticFiles();

app.UseRouting();

app.UseSession();          // 🔥 phải đặt trước auth

app.UseAuthentication();
app.UseAuthorization();

// ================== ROUTE ==================
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// ================== CULTURE ==================
var cultureInfo = new CultureInfo("vi-VN");
CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;

// ================== DATABASE ==================
string connectionString = builder.Configuration.GetConnectionString("LiteCommerceDB")
    ?? throw new Exception("Không tìm thấy connection string");

Configuration.Initialize(connectionString);

app.Run();