using System.Net.Http.Json;

namespace Zero.Services
{
    public class SurfService
    {
        private readonly HttpClient _httpClient;
        // Coordenadas de Navegantes/SC
        private const double Lat = -26.89;
        private const double Lon = -48.65;

        public SurfService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        // Agora o método recebe latitude e longitude de quem o chama
        public async Task<string> ObterDadosMaritimos(double lat, double lon)
        {
            try
            {
                var url = $"https://marine-api.open-meteo.com/v1/marine?latitude={lat}&longitude={lon}&current=wave_height,wave_period,wave_direction,wind_wave_height&timezone=America%2FSao_Paulo";

                var response = await _httpClient.GetFromJsonAsync<dynamic>(url);
                return response?.ToString() ?? "Dados indisponíveis.";
            }
            catch (Exception ex)
            {
                return $"Erro na API de ondas: {ex.Message}";
            }
        }
    }
}