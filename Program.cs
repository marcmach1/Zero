using Zero.Services;

var builder = WebApplication.CreateBuilder(args);

// --- Configuração dos Serviços ---
builder.Services.AddControllersWithViews();

// Registrando nossos serviços de forma limpa
builder.Services.AddScoped<GeminiService>();
builder.Services.AddHttpClient<SurfService>();
builder.Services.AddSingleton<NotificationService>();

// O Watchdog é quem fica rodando em segundo plano vigiando o mar
builder.Services.AddHostedService<SurfWatchdog>();

var app = builder.Build();

// --- Pipeline de Requisições (Middleware) ---
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles(); // Mudança leve aqui para o padrão mais comum
app.UseRouting();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// --- O Zero entra em ação ---
Console.WriteLine("🌊 Zero Surf Station inicializada com sucesso!");
Console.WriteLine("🤖 Vigia do mar ativa em Navegantes...");

app.Run();