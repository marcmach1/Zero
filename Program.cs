using Zero.Services;

var builder = WebApplication.CreateBuilder(args);

// --- Configuração dos Serviços ---
builder.Services.AddControllersWithViews();

var geminiKey = builder.Configuration["GeminiApiKey"] ?? "";

if (string.IsNullOrEmpty(geminiKey)) {
    // Força a leitura manual do arquivo se o .NET se perder
    var config = new ConfigurationBuilder()
        .SetBasePath(Directory.GetCurrentDirectory())
        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
        .Build();
    geminiKey = config["GeminiApiKey"] ?? "";
}

builder.Services.AddSingleton(new GeminiService(geminiKey ?? ""));

builder.Services.AddSingleton<LocationService>();
builder.Services.AddScoped<SurfService>();
builder.Services.AddSingleton<NotificationService>();
builder.Services.AddHostedService<SurfWatchdog>();


var app = builder.Build();

// --- Pipeline de Requisições ---
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}

app.UseStaticFiles(); 
app.UseRouting();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapControllers(); 

Console.WriteLine("🌊 Zero Surf Station: Online e dropando!");

app.Run();