using Microsoft.AspNetCore.Mvc;
using Zero.Services;
using System.Text.Json; // Importa o Helper para usar a tradução do clima

namespace Zero.Controllers;

[ApiController]
[Route("zero")]
public class ZeroController : Controller
{
    private readonly GeminiService _geminiService;
    private readonly SurfService _surfService;
    private readonly LocationService _locationService;

    // Injetamos o LocationService aqui também para o Controller reconhecer o serviço
    public ZeroController(GeminiService geminiService, SurfService surfService, LocationService locationService)
    {
        _geminiService = geminiService;
        _surfService = surfService;
        _locationService = locationService;
    }

    [HttpGet("perguntar")]
    public async Task<IActionResult> Get(string prompt)
    {
        var resposta = await _geminiService.Perguntar(prompt);
        return Ok(new { resposta });
    }

    // Esta é a rota que o seu botão na Home chama agora
    [HttpGet("boletim-local")]
public IActionResult BoletimLocal(double lat, double lon)
    {
        // Apenas abre a página. O JS da página vai ler lat e lon da URL.
        return View("Surf");
    }

  
   
    [HttpGet("surf")]
    public async Task<IActionResult> GetSurfReport(double lat = -26.89, double lon = -48.65)
    {
        // Garantia de coordenadas caso o navegador falhe
        if (lat == 0) lat = -26.89;
        if (lon == 0) lon = -48.65;

        var resultadoBruto = await _surfService.ObterDadosMaritimos(lat, lon);
        Console.WriteLine($"Dados brutos recebidos: {resultadoBruto}");

        try
        {
            // 1. Separação dos JSONs
            var partes = resultadoBruto.Split('|');
            var jsonOndas = partes[0].Replace("[DADOS_ONDAS]: ", "").Trim();
            var jsonVento = partes[1].Replace("[DADOS_VENTO]: ", "").Trim();

            using var docOndas = JsonDocument.Parse(jsonOndas);
            using var docVento = JsonDocument.Parse(jsonVento);

            var hourlyOndas = docOndas.RootElement.GetProperty("hourly");
            var hourlyVento = docVento.RootElement.GetProperty("hourly");

            // 2. Lógica de Índice de Horário com Fallback
            int indiceHora = DateTime.Now.Hour;
            var waveArray = hourlyOndas.GetProperty("wave_height");

            // Se o dado da hora atual for nulo, tenta a hora anterior (resiliência)
            if (waveArray[indiceHora].ValueKind == JsonValueKind.Null && indiceHora > 0)
            {
                indiceHora--;
            }

            // 3. Funções de extração segura (Null-Safe)
            double SafeGetDouble(JsonElement element) => 
                element.ValueKind == JsonValueKind.Number ? element.GetDouble() : 0.0;

            int SafeGetInt(JsonElement element) => 
                element.ValueKind == JsonValueKind.Number ? element.GetInt32() : 0;

            // 4. Captura dos dados
            double altura = SafeGetDouble(waveArray[indiceHora]);
            double periodo = SafeGetDouble(hourlyOndas.GetProperty("wave_period")[indiceHora]);
            double ventoVel = SafeGetDouble(hourlyVento.GetProperty("wind_speed_10m")[indiceHora]);
            double ventoDir = SafeGetDouble(hourlyVento.GetProperty("wind_direction_10m")[indiceHora]);
            int climaCode = SafeGetInt(hourlyVento.GetProperty("weathercode")[indiceHora]);

            // 5. Traduções
            string climaTraduzido = WeatherHelper.TraduzirWeatherCode(climaCode);
            string ventoCardeal = WeatherHelper.ConverterDirecaoVento(ventoDir);

            // 6. Lógica de strings para o Prompt (Avisa o Zero se a onda sumiu)
            string infoOndas = (altura <= 0) ? "Sensores de boia temporariamente offline" : $"{altura}m";
            string infoPeriodo = (periodo <= 0) ? "Não disponível" : $"{periodo}s";

            // 7. Prompt Estruturado
            string promptFinal = $@"
                Você é o Zero, surfista local de Navegantes/SC. 
                Analise os dados reais abaixo e gere um boletim ORGANIZADO por tópicos.
                IMPORTANTE: Se as ondas estiverem como 'offline', avise a galera mas foque no vento e visual.

                📊 DADOS REAIS DE AGORA:
                - ONDAS: {infoOndas}
                - PERÍODO: {infoPeriodo}
                - VENTO: {ventoVel} km/h de {ventoCardeal}
                - TEMPO: {climaTraduzido}

                ---
                ESTRUTURA OBRIGATÓRIA DA RESPOSTA:
                🤙 **SALVE, MESTRE!** (Saudação curta)

                🌊 **CONDIÇÕES DO MAR**
                (Analise {infoOndas} e {infoPeriodo})

                💨 **VENTO E FORMAÇÃO**
                {ventoVel} km/h de {ventoCardeal} (Explique se é Terral, Maral ou de lado em Navega)

                🌤️ **VISUAL**
                {climaTraduzido}

                📌 **VEREDITO DO ZERO**
                (Vale a caída ou melhor esperar?)

                📍 **DICA PRA ACERTAR O PICO**
                (Sugira um ponto em Navegantes ou região)
                ---";

            var respostaIA = await _geminiService.Perguntar(promptFinal);

            return Ok(new { 
                boletim = respostaIA, 
                dados_tecnicos = new { altura, periodo, ventoVel, ventoCardeal, climaTraduzido, horaSincronizada = indiceHora },
                cidade = "Navegantes" 
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro crítico: {ex.Message}");
            var fallback = await _geminiService.Perguntar("Zero, os sensores pifaram de vez. Dá um salve na galera de Navega, avisa do erro técnico e diz pra olharem pela câmera do Porto!");
            return Ok(new { boletim = fallback, erro = ex.Message });
        }
    }
}