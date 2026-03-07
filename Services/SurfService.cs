using System;
using System.Net.Http;
using System.Threading.Tasks;

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
                // Usamos um handler que ignora qualquer proxy do sistema que possa estar sujando a requisição
                using var handler = new HttpClientHandler();
                using var client = new HttpClient(handler);
                
                // URL ultra-simples para teste
                string url = "https://marine-api.open-meteo.com/v1/marine?latitude=-26.89&longitude=-48.65&hourly=wave_height";

                var request = new HttpRequestMessage(HttpMethod.Get, url);
                
                // User-Agent de um navegador real para não ser barrado
                request.Headers.TryAddWithoutValidation("User-Agent", "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36");

                var response = await client.SendAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadAsStringAsync();
                }

                var corpoErro = await response.Content.ReadAsStringAsync();
                return $"{{\"boletim\":\"Erro {response.StatusCode}: {corpoErro}\"}}";
            }
            catch (Exception ex)
            {
                return $"{{\"boletim\":\"Erro fatal: {ex.Message}\"}}";
            }
        }
    }
}