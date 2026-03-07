using System.Text.Json;

namespace Zero.Services;

public class LocationService
{
    private readonly HttpClient _httpClient;

    public LocationService()
    {
        _httpClient = new HttpClient();
        // O Nominatim (OpenStreetMap) exige um User-Agent para identificar quem está chamando
        _httpClient.DefaultRequestHeaders.Add("User-Agent", "ZeroSurfStationApp");
    }

    public async Task<string> ObterCidadePorCoordenadas(double lat, double lon)
    {
        try
        {
            // API gratuita do OpenStreetMap para Geocodificação Reversa
            var url = $"https://nominatim.openstreetmap.org/reverse?format=json&lat={lat.ToString().Replace(",", ".")}&lon={lon.ToString().Replace(",", ".")}";
            
            var response = await _httpClient.GetStringAsync(url);
            using var data = JsonDocument.Parse(response);
            
            if (data.RootElement.TryGetProperty("address", out var address))
            {
                // Tenta pegar a cidade, se não tiver, tenta a vila, cidade pequena ou o bairro
                if (address.TryGetProperty("city", out var city)) return city.GetString() ?? "Navegantes";
                if (address.TryGetProperty("town", out var town)) return town.GetString() ?? "Navegantes";
                if (address.TryGetProperty("suburb", out var suburb)) return suburb.GetString() ?? "Navegantes";
            }
            
            return "Navegantes"; // Fallback caso não encontre nada
        }
        catch (Exception ex)
        {
            Console.WriteLine($"⚠️ Erro ao buscar localização: {ex.Message}");
            return "Navegantes"; // Se cair a internet da localização, o Zero assume que você está em casa
        }
    }
}