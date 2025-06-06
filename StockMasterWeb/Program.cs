using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using StockMasterWeb.Models;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

// 👇 EPPlus — лицензия на Excel
ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

// 👇 Подключаем EF Core
builder.Services.AddDbContext<StockMasterContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// 👇 Контроллеры с представлениями
builder.Services.AddControllersWithViews();

// 👇 Аутентификация через cookies
builder.Services.AddAuthentication("Cookies")
    .AddCookie("Cookies", options =>
    {
        options.LoginPath = "/Auth/Login";
    });

// 👇 Авторизация
builder.Services.AddAuthorization();

var app = builder.Build();

// 👇 Обработка ошибок и HTTPS
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// 👇 Важно: Authentication перед Authorization!
app.UseAuthentication();
app.UseAuthorization();

// 👇 Стартовая точка — страница логина
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Auth}/{action=Login}/{id?}");

app.Run();
