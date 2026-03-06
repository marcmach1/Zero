using Microsoft.AspNetCore.Mvc;
using Zero.Services;

namespace Zero.Controllers;

[ApiController]
[Route("zero")]
public class ZeroController : ControllerBase
{
    private readonly GeminiService _gemini;

    public ZeroController(GeminiService gemini) => _gemini = gemini;

    [HttpGet("perguntar")]
    public async Task<IActionResult> Get(string prompt)
    {
        var resposta = await _gemini.Perguntar(prompt);
        return Ok(new { resposta });
    }


    [HttpGet("surf")]
    public async Task<IActionResult> GetSurfReport([FromServices] SurfService surfService, [FromServices] GeminiService geminiService)
    {
        // 1. Busca os dados brutos da API de Maré
        var dadosBrutos = await surfService.ObterDadosMaritimos();

        // 2. Cria o prompt especial enviando os dados para o seu "Surfista IA"
        var promptComDados = $"Analise estes dados reais de agora em Navegantes e me dê o boletim: {dadosBrutos}";

        // 3. O Gemini processa com a personalidade que você já configurou
        var resposta = await geminiService.Perguntar(promptComDados);

        return Ok(new { boletim = resposta });
    }
}