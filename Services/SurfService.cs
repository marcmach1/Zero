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
                using var client = new HttpClient();
                var culture = System.Globalization.CultureInfo.InvariantCulture;

                // Open-Meteo é sensível ao formato. Usamos "F4" para precisão e cultura Invariant (ponto em vez de vírgula)
                string urlMarine = $"https://marine-api.open-meteo.com/v1/marine?latitude={lat.ToString(culture)}&longitude={lon.ToString(culture)}&hourly=wave_height,wave_period&forecast_days=1";
                string urlWeather = $"https://api.open-meteo.com/v1/forecast?latitude={lat.ToString(culture)}&longitude={lon.ToString(culture)}&hourly=wind_speed_10m,wind_direction_10m,weathercode&forecast_days=1";

                client.DefaultRequestHeaders.Add("User-Agent", "ZeroSurfApp/1.0");

                // Buscando dados de Ondas
                var resMarine = await client.GetAsync(urlMarine);
                if (!resMarine.IsSuccessStatusCode) throw new Exception($"Erro API Marine: {resMarine.StatusCode}");
                var jsonMarine = await resMarine.Content.ReadAsStringAsync();

                // Buscando dados de Vento
                var resWeather = await client.GetAsync(urlWeather);
                if (!resWeather.IsSuccessStatusCode) throw new Exception($"Erro API Weather: {resWeather.StatusCode}");
                var jsonWeather = await resWeather.Content.ReadAsStringAsync();

                // IMPORTANTE: Montamos a string garantindo que o delimitador | SEMPRE exista
                // Removi o Substring fixo para evitar cortar o delimitador no meio
                return $"[DADOS_ONDAS]: {jsonMarine} | [DADOS_VENTO]: {jsonWeather}";
            }
            catch (Exception ex)
            {
                // Se der erro aqui, retornamos uma string que o Controller consiga dar Split sem crashar
                Console.WriteLine($"--- ERRO NO SERVICE: {ex.Message}");
                return $"[DADOS_ONDAS]: {{}} | [DADOS_VENTO]: {{}} | ERRO: {ex.Message}";
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