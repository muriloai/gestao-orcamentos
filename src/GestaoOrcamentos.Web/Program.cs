using System.Globalization;

using GestaoOrcamentos.Web.Data;
using GestaoOrcamentos.Web.Services;

using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;

var aplicarMigracoes = args.Contains("--migrate", StringComparer.OrdinalIgnoreCase);
var argumentosConfiguracao = args.Where(argumento =>
    !string.Equals(argumento, "--migrate", StringComparison.OrdinalIgnoreCase)).ToArray();
var builder = WebApplication.CreateBuilder(new WebApplicationOptions
{
    Args = argumentosConfiguracao,
    ContentRootPath = AppContext.BaseDirectory
});

var connectionString = builder.Configuration.GetConnectionString("GestaoOrcamentos");
if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException(
        "Configure a conexão 'GestaoOrcamentos' pela variável ConnectionStrings__GestaoOrcamentos.");
}

var portaLocal = builder.Configuration.GetValue("LocalServer:Port", 5198);
if (portaLocal is < 1 or > 65535)
{
    throw new InvalidOperationException("A porta local deve estar entre 1 e 65535.");
}

builder.WebHost.ConfigureKestrel(options => options.ListenLocalhost(portaLocal));

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();
builder.Services.AddOpenApi();
builder.Services.AddDbContext<GestaoOrcamentosDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddScoped<ClienteService>();
builder.Services.AddScoped<OrcamentoService>();
builder.Services.AddScoped<RelatorioOrcamentosService>();
builder.Services.AddScoped<AlertasOrcamentoService>();
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddHostedService<AlertaOrcamentoWorker>();
builder.Services.AddHttpClient<ViaCepClient>(cliente =>
{
    cliente.BaseAddress = new Uri("https://viacep.com.br/ws/");
    cliente.Timeout = TimeSpan.FromSeconds(5);
});

await using var app = builder.Build();

if (aplicarMigracoes)
{
    try
    {
        await using var escopo = app.Services.CreateAsyncScope();
        var dbContext = escopo.ServiceProvider.GetRequiredService<GestaoOrcamentosDbContext>();
        await dbContext.Database.MigrateAsync();
        app.Logger.LogInformation("As migrations do banco foram aplicadas com sucesso.");
        return 0;
    }
    catch (Exception ex)
    {
        app.Logger.LogCritical(ex, "Não foi possível aplicar as migrations do banco.");
        return 1;
    }
}

var cultura = CultureInfo.GetCultureInfo("pt-BR");

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}
else
{
    app.MapOpenApi();
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

await app.RunAsync();
return 0;
