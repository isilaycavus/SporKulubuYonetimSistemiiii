using System.Globalization;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using SporSalonuMVC.Models; // Projendeki namespace

var builder = WebApplication.CreateBuilder(args);

// Veritaban� ba�lant�s� (PostgreSQL)
builder.Services.AddDbContext<SporSalonuDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Oturum (Session) ve MVC servisleri
builder.Services.AddSession();
builder.Services.AddControllersWithViews();

// ? K�lt�r ayarlar� (TR - T�rk�e)
var cultureInfo = new CultureInfo("tr-TR");
cultureInfo.NumberFormat.CurrencyDecimalSeparator = ",";
cultureInfo.NumberFormat.NumberDecimalSeparator = ",";

var localizationOptions = new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture(cultureInfo),
    SupportedCultures = new List<CultureInfo> { cultureInfo },
    SupportedUICultures = new List<CultureInfo> { cultureInfo }
};

var app = builder.Build();

// ?? Uygulama yap�land�rmas�
app.UseRequestLocalization(localizationOptions); // ? T�rk�e format deste�i
app.UseStaticFiles();
app.UseRouting();
app.UseSession();

app.MapDefaultControllerRoute();
app.Run();
