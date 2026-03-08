using Zero.Services;

var builder = WebApplication.CreateBuilder(args);

// --- Configuração dos Serviços ---
builder.Services.AddControllersWithViews();

// --- Configuração dos Serviços ---
builder.Services.AddControllersWithViews();

// 1. Tenta buscar no formato Seção:Chave (O que o Balta recomenda)
var geminiKey = builder.Configuration["Gemini:ApiKey"];

// 2. Fallback: Se não achou na seção, tenta o nome antigo (Sua AppSettings anterior)
if (string.IsNullOrEmpty(geminiKey)) {
    geminiKey = builder.Configuration["GeminiApiKey"];
}

// Log para você ter CERTEZA no terminal se a chave carregou
if (string.IsNullOrEmpty(geminiKey)) {
    Console.WriteLine("⚠️ ERRO: Chave do Gemini não encontrada nas configurações!");
} else {
    Console.WriteLine("✅ Chave do Gemini carregada com sucesso.");
}

// Registra o serviço com a chave encontrada
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