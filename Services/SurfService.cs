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

        public async Task<string> ObterDadosMaritimos()
        {
            try
            {
                // API Open-Meteo (Marine) - Pega altura da onda, período e direção
                var url = $"https://marine-api.open-meteo.com/v1/marine?latitude={Lat}&longitude={Lon}&current=wave_height,wave_period,wave_direction,wind_wave_height&timezone=America%2FSao_Paulo";

                var response = await _httpClient.GetFromJsonAsync<dynamic>(url);
                
                // Retornamos como string para enviar direto ao Gemini processar
                return response?.ToString() ?? "Não foi possível obter dados do mar.";
            }
            catch (Exception ex)
            {
                return $"Erro ao conectar com a API de ondas: {ex.Message}";
            }
        }
    }
}