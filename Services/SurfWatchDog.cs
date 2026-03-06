using Microsoft.Extensions.Hosting;

namespace Zero.Services
{
    public class SurfWatchdog : BackgroundService
    {
        private readonly IServiceProvider _services;
        private readonly TimeSpan _periodoSubida = TimeSpan.FromHours(3); // Checa a cada 3 horas

        public SurfWatchdog(IServiceProvider services)
        {
            _services = services;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using (var scope = _services.CreateScope())
                {
                    var surfService = scope.ServiceProvider.GetRequiredService<SurfService>();
                    var geminiService = scope.ServiceProvider.GetRequiredService<GeminiService>();

                    // 1. Pega os dados
                    var dados = await surfService.ObterDadosMaritimos(-26.89, -48.65);

                    // 2. Pergunta ao Gemini se a condição é "Alerta Máximo"
                    var prompt = $"Analise os dados: {dados}. Responda APENAS 'SIM' se as ondas estiverem acima de 0.8m e vento favorável, ou 'NAO' se estiver ruim.";
                    var veredito = await geminiService.Perguntar(prompt);

                    if (veredito.Contains("SIM"))
                    {
                        // 3. Aqui vamos disparar a mensagem (Telegram em breve!)
                        Console.WriteLine("ALERTA: O MAR TÁ CLÁSSICO! Enviando notificação...");
                    }
                }

                await Task.Delay(_periodoSubida, stoppingToken);
            }
        }
    }
}