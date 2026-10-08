using System.Globalization;

using GestaoOrcamentos.Web.Data;
using GestaoOrcamentos.Web.Services;

using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();
builder.Services.AddDbContext<GestaoOrcamentosDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("GestaoOrcamentos")));
builder.Services.AddScoped<ClienteService>();
builder.Services.AddScoped<OrcamentoService>();

var app = builder.Build();
var cultura = CultureInfo.GetCultureInfo("pt-BR");

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}

app.UseRequestLocalization(new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture(cultura),
    SupportedCultures = [cultura],
    SupportedUICultures = [cultura]
});

app.UseRouting();
app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();
app.MapRazorPages().WithStaticAssets();

app.Run();