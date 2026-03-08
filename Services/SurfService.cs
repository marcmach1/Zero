using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text.Json;

namespace Zero.Services
{
    public class SurfService
    {
        // Removi o HttpClient do construtor para teste de isolamento
        public SurfService() { }

public async Task<string> ObterDadosMaritimos(double lat, double lon)
{
    try 
    {
        using var handler = new HttpClientHandler();
        using var client = new HttpClient(handler);
        var culture = System.Globalization.CultureInfo.InvariantCulture;

        // 1. URL DE ONDAS (Marine API) 
        string urlMarine = string.Format(culture, 
            "https://marine-api.open-meteo.com/v1/marine?latitude={0:F2}&longitude={1:F2}&hourly=wave_height,wave_period&forecast_days=1", 
            lat, lon);
        
        // 2. URL DE VENTO/TEMPO (Forecast API) 
        string urlWeather = string.Format(culture, 
            "https://api.open-meteo.com/v1/forecast?latitude={0:F2}&longitude={1:F2}&hourly=wind_speed_10m,wind_direction_10m,weathercode&forecast_days=1", 
            lat, lon);

        // Configuração do Request (Marine)
        var reqMarine = new HttpRequestMessage(HttpMethod.Get, urlMarine);
        reqMarine.Headers.TryAddWithoutValidation("User-Agent", "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7)");
        var resMarine = await client.SendAsync(reqMarine);
        var jsonMarine = resMarine.IsSuccessStatusCode ? await resMarine.Content.ReadAsStringAsync() : "{}";

        // Configuração do Request (Weather)
        var reqWeather = new HttpRequestMessage(HttpMethod.Get, urlWeather);
        reqWeather.Headers.TryAddWithoutValidation("User-Agent", "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7)");
       
        var resWeather = await client.SendAsync(reqWeather);
        var jsonWeather = resWeather.IsSuccessStatusCode ? await resWeather.Content.ReadAsStringAsync() : "{}";

       
        // Limitamos o tamanho para não estourar o prompt (Substring)
        string resultadoUnificado = $"[DADOS_ONDAS]: {jsonMarine} | [DADOS_VENTO]: {jsonWeather}";

        return resultadoUnificado.Length > 3000 ? resultadoUnificado.Substring(0, 3000) : resultadoUnificado;
    }
    catch (Exception ex)
    {
        return $"{{\"boletim\":\"Erro fatal: {ex.Message}\"}}";
    }
}
      

        public async Task<string> GetTideDataAsync(double lat, double lon)
        {
           
            try 
            {
                // Aqui simulamos o retorno que o Gemini vai interpretar
                // Futuramente, você pode trocar isso por uma chamada à WorldTides ou outra API
                return "Alta: 10:45 | Baixa: 17:10 (Estimada)";
            }
            catch 
            {
                return "Maré: Consultar Tábua Local";
            }
        }
    }
}