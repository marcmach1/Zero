using System.Text;
using System.Text.Json;

namespace Zero.Services;

public class GeminiService
{
    private readonly string _apiKey;
    private readonly HttpClient _httpClient;

    public GeminiService(IConfiguration configuration)
    {
        _apiKey = configuration["GeminiApiKey"] ?? throw new Exception("Chave API não encontrada!");
        _httpClient = new HttpClient();
    }

    public async Task<string> Perguntar(string mensagem)
    {

        var url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:generateContent?key={_apiKey}";
        var payload = new {
        contents = new[] { 
            new { 
                role = "user", // O "chefe" dando as ordens
                parts = new[] { new { text = $"Você é o Zero, um assistente que se comunica como um surfista. Responda a esta pergunta de forma curta: {mensagem}" } } 
            }   
        }
        };

        var json = JsonSerializer.Serialize(payload);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync(url, content);
        var responseBody = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
            return $"Erro na API: {response.StatusCode}";

        // Extrai apenas o texto da resposta gigante do Google
        using var doc = JsonDocument.Parse(responseBody);
        return doc.RootElement
            .GetProperty("candidates")[0]
            .GetProperty("content")
            .GetProperty("parts")[0]
            .GetProperty("text").GetString() ?? "Sem resposta.";
    }
}