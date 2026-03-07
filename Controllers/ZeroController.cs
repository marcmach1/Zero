using Microsoft.AspNetCore.Mvc;
using Zero.Services;

namespace Zero.Controllers;

[ApiController]
[Route("zero")]
public class ZeroController : ControllerBase
{
    private readonly GeminiService _geminiService;
    private readonly SurfService _surfService;

    // Injetamos tudo aqui no início. É o jeito mais seguro e limpo!
    public ZeroController(GeminiService geminiService, SurfService surfService)
    {
        _geminiService = geminiService;
        _surfService = surfService;
    }

    [HttpGet("perguntar")]
    public async Task<IActionResult> Get(string prompt)
    {
        var resposta = await _geminiService.Perguntar(prompt);
        return Ok(new { resposta });
    }

    [HttpGet("surf")]
    public async Task<IActionResult> GetSurfReport()
    {
        // 1. Busca os dados brutos (O SurfService agora está blindado contra o 400/403)
        var dadosBrutos = await _surfService.ObterDadosMaritimos(-26.89, -48.65);

        // 2. Se o serviço retornou erro, a gente mostra o que aconteceu
        if (dadosBrutos.Contains("Erro"))
        {
            return StatusCode(500, new { erro = "A API de surf falhou", detalhes = dadosBrutos });
        }

        // 3. Cria o prompt para a IA
        var promptComDados = $"Analise estes dados reais de agora em Navegantes e me dê o boletim: {dadosBrutos}";

        // 4. O Gemini processa
        var resposta = await _geminiService.Perguntar(promptComDados);

        return Ok(new { boletim = resposta });
    }
}