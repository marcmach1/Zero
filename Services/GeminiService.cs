using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Zero.Services;

public class GeminiService
{
    private readonly string _apiKey;
    private readonly HttpClient _httpClient;

    public GeminiService(string apiKey)
    {
        _apiKey = apiKey; // Sem segredos, direto ao ponto
        _httpClient = new HttpClient();
    }

    public async Task<string> Perguntar(string mensagem)
    {
        try 
        {
            // O MODELO DO SUCESSO: gemini-3.1-flash-lite-preview
            var url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-3.1-flash-lite-preview:generateContent?key={_apiKey}";
            
            var payload = new {
                contents = new[] { 
                    new { 
                        parts = new[] { 
                            new { text = $"Você é o Zero, um surfista local de Navegantes/SC. Analise os dados e dê um boletim curto, sincero e com gírias de surf: {mensagem}" } 
                        } 
                    }   
                }
            };

            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(url, content);
            var responseBody = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"--- ERRO NA CHAMADA ---");
                Console.WriteLine(responseBody);
                return $"Erro: {response.StatusCode}";
            }

            using var doc = JsonDocument.Parse(responseBody);
            
            // Extração da resposta
            return doc.RootElement
                .GetProperty("candidates")[0]
                .GetProperty("content")
                .GetProperty("parts")[0]
                .GetProperty("text").GetString() ?? "O mar tá em silêncio...";
        }
        catch (Exception ex)
        {
            return $"Erro fatal no Zero: {ex.Message}";
        }
    }
}