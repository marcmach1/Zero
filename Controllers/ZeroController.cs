using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using Zero.Services;
using Zero.Models;
using Zero.Helpers;

[ApiController]
[Route("zero")]
public class ZeroController : Controller
{
    private readonly GeminiService _geminiService;
    private readonly SurfService _surfService;
    private readonly LocationService _locationService;

    public ZeroController(GeminiService geminiService, SurfService surfService, LocationService locationService)
    {
        _geminiService = geminiService;
        _surfService = surfService;
        _locationService = locationService;
    }

    [HttpGet("surf")]
    public async Task<IActionResult> GetBoletim(double lat, double lon)
    {
        try
        {
            // Fallback para Navegantes se as coordenadas forem zero
            if (lat == 0) lat = -26.89;
            if (lon == 0) lon = -48.65;

            var resultadoBruto = await _surfService.ObterDadosMaritimos(lat, lon);
            
            // --- PROTEÇÃO CONTRA O ERRO DE SPLIT ---
            if (string.IsNullOrEmpty(resultadoBruto) || !resultadoBruto.Contains("|"))
            {
                Console.WriteLine($"--- ERRO FORMATO: O serviço retornou: {resultadoBruto}");
                return Ok(new { boletim = "O Zero tentou olhar o mar, mas a neblina (erro de conexão) não deixou ver nada!", previsaoCards = new List<object>() });
            }

            var partes = resultadoBruto.Split('|');
            var jsonOndas = partes[0].Replace("[DADOS_ONDAS]: ", "").Trim();
            var jsonVento = partes[1].Replace("[DADOS_VENTO]: ", "").Trim();

            // --- VALIDAÇÃO DE JSON VAZIO ---
            if (jsonOndas == "{}" || jsonVento == "{}")
            {
                return Ok(new { boletim = "As boias de sinalização estão offline agora. Tenta daqui a pouco!", previsaoCards = new List<object>() });
            }

            using var docOndas = JsonDocument.Parse(jsonOndas);
            using var docVento = JsonDocument.Parse(jsonVento);

            var hourlyOndas = docOndas.RootElement.GetProperty("hourly");
            var hourlyVento = docVento.RootElement.GetProperty("hourly");

            // 1. Gera a lista para os cards
            var previsaoCards = ExtrairPrevisaoCards(hourlyOndas, hourlyVento);

            // 2. Dados para o boletim atual (UTC para bater com a API)
            int indiceHora = DateTime.UtcNow.Hour;
            
            // Garantindo que o índice não estoure o array da API
            int maxIndices = hourlyOndas.GetProperty("wave_height").GetArrayLength();
            if (indiceHora >= maxIndices) indiceHora = maxIndices - 1;

            double altura = SafeGetDouble(hourlyOndas.GetProperty("wave_height")[indiceHora]);
            double periodo = SafeGetDouble(hourlyOndas.GetProperty("wave_period")[indiceHora]);
            double ventoVel = SafeGetDouble(hourlyVento.GetProperty("wind_speed_10m")[indiceHora]);
            double ventoDir = SafeGetDouble(hourlyVento.GetProperty("wind_direction_10m")[indiceHora]);
            int climaCode = hourlyVento.GetProperty("weathercode")[indiceHora].GetInt32();

            string climaTraduzido = WeatherHelper.TraduzirWeatherCode(climaCode);
            string ventoCardeal = WeatherHelper.ConverterDirecaoVento(ventoDir);
            string infoOndas = (altura <= 0) ? "Flat ou sensores offline" : $"{altura:F1}m";

            // 3. IA Processa o Boletim
            string promptFinal = $@"
                Você é o Zero, surfista local de Navegantes/SC. 
                🚨 CONDIÇÕES REAIS AGORA:
                - ONDAS: {infoOndas}
                - PERÍODO: {periodo}s
                - VENTO: {ventoVel} km/h de {ventoCardeal}
                - CLIMA: {climaTraduzido}
                Gere um boletim curto com gírias de surfista (ex: 'tá rolando', 'outside', 'vaca', 'merreca').";

            var respostaIA = await _geminiService.Perguntar(promptFinal);
            Console.WriteLine($"--- RESPOSTA DO ZERO ---\n{promptFinal}");

            Console.WriteLine($"--- BOLETIM GERADO ---\n{respostaIA}");

            return Ok(new { 
                boletim = respostaIA, 
                previsaoCards = previsaoCards,
                dados_tecnicos = new { altura, periodo, ventoVel, ventoCardeal, climaTraduzido },
                cidade = "Navegantes" 
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"--- ERRO NO CONTROLLER: {ex.Message}");
            return StatusCode(500, new { 
                boletim = "Putz, o Zero tomou uma vaca feia no código!", 
                erro = ex.Message 
            });
        }
    }

    private double SafeGetDouble(JsonElement element) => 
        element.ValueKind == JsonValueKind.Number ? element.GetDouble() : 0.0;

    private List<PrevisaoPeriodoDTO> ExtrairPrevisaoCards(JsonElement hourlyOndas, JsonElement hourlyVento)
    {
        var cards = new List<PrevisaoPeriodoDTO>();
        int[] horasDesejadas = { 9, 15 }; 
        string[] nomes = { "Manhã", "Tarde" };

        try {
            for (int dia = 0; dia < 2; dia++)
            {
                for (int i = 0; i < horasDesejadas.Length; i++)
                {
                    int hora = horasDesejadas[i];
                    int indice = hora + (dia * 24);
                    
                    var heightProp = hourlyOndas.GetProperty("wave_height");
                    if (indice >= heightProp.GetArrayLength()) continue;

                    double altura = SafeGetDouble(heightProp[indice]);
                    double ventoDir = SafeGetDouble(hourlyVento.GetProperty("wind_direction_10m")[indice]);
                    double ventoV = SafeGetDouble(hourlyVento.GetProperty("wind_speed_10m")[indice]);
                    double p = SafeGetDouble(hourlyOndas.GetProperty("wave_period")[indice]);

                    cards.Add(new PrevisaoPeriodoDTO {
                        Titulo = $"{DateTime.Now.AddDays(dia):ddd / dd} - {nomes[i]}".ToUpper(),
                        Altura = altura,
                        AlturaMin = altura * 0.7, // Simulação de variação
                        Periodo = (int)p,
                        VentoVel = (int)ventoV,
                        VentoDirecao = WeatherHelper.ConverterDirecaoVento(ventoDir),
                        IconeVento = (int)ventoDir // Passando os graus para a seta girar
                    });
                }
            }
        } catch (Exception ex) {
            Console.WriteLine("Erro ao gerar cards: " + ex.Message);
        }
        return cards;
    }
}