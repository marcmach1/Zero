using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Zero.Services
{
    public class SurfWatchdog : BackgroundService
    {
        private readonly IServiceProvider _services;
        private readonly ILogger<SurfWatchdog> _logger;
        // Tempo entre as checagens (ex: 2 horas)
        private readonly TimeSpan _periodoEntreChecagens = TimeSpan.FromHours(2); 

        public SurfWatchdog(IServiceProvider services, ILogger<SurfWatchdog> logger)
        {
            _services = services;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("🐕 Watchdog de Surf iniciado e patrulhando...");

            while (!stoppingToken.IsCancellationRequested)
            {
                var agora = DateTime.Now;

                // --- REGRA DE HORÁRIO: Só vigia das 06:00 às 20:00 ---
                if (agora.Hour >= 6 && agora.Hour <= 20)
                {
                    _logger.LogInformation("🌊 Horário comercial de surf! Iniciando verificação das {time}...", agora.ToString("HH:mm"));

                    using (var scope = _services.CreateScope())
                    {
                        try 
                        {
                            // Pegando os serviços de dentro do escopo
                            var surfService = scope.ServiceProvider.GetRequiredService<SurfService>();
                            var geminiService = scope.ServiceProvider.GetRequiredService<GeminiService>();
                            var notifier = scope.ServiceProvider.GetRequiredService<NotificationService>();

                            // 1. Busca dados reais (Coordenadas de Navega)
                            var dados = await surfService.ObterDadosMaritimos(-26.89, -48.65);

                            // 2. Prompt para o Gemini
                            var prompt = $@"
                            Analise estes dados de hoje em Navegantes: {dados}. 
                            Critérios para MAR BOM: Altura > 0.6m, Período > 7s e vento fraco ou terral.
                            Se o mar estiver CLÁSSICO dentro desses critérios, responda:
                            'SIM' + um boletim curto, sarcástico e com gírias de surfista.
                            Caso contrário, responda apenas 'NAO'.
                            ";

                            var respostaGemini = await geminiService.Perguntar(prompt);

                            // 3. Só envia o Telegram se o Gemini deu o sinal verde (SIM)
                            if (respostaGemini.Trim().ToUpper().StartsWith("SIM"))
                            {
                                _logger.LogInformation("✅ Mar tá bom! Enviando notificação para o Marcos...");
                                await notifier.EnviarMensagem(respostaGemini);
                            }
                            else 
                            {
                                _logger.LogInformation("🫗 Mar flat ou vento ruim. O Zero vai continuar em silêncio.");
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError("❌ Erro na rodada do Watchdog: {message}", ex.Message);
                        }
                    }
                }
                else 
                {
                    _logger.LogInformation("🌙 {time}h - Hora de silêncio. O mar está escuro e o Zero está dormindo.", agora.Hour);
                }

                // Espera o tempo definido (2 horas) ou até o programa ser fechado
                await Task.Delay(_periodoEntreChecagens, stoppingToken);
            }
        }
    }
}