using Microsoft.AspNetCore.Mvc;
using Zero.Services;

namespace Zero.Controllers;

[ApiController]
[Route("zero")]
public class ZeroController : Controller
{
    private readonly GeminiService _geminiService;
    private readonly SurfService _surfService;
    private readonly LocationService _locationService;

    // Injetamos o LocationService aqui também para o Controller reconhecer o serviço
    public ZeroController(GeminiService geminiService, SurfService surfService, LocationService locationService)
    {
        _geminiService = geminiService;
        _surfService = surfService;
        _locationService = locationService;
    }

    [HttpGet("perguntar")]
    public async Task<IActionResult> Get(string prompt)
    {
        var resposta = await _geminiService.Perguntar(prompt);
        return Ok(new { resposta });
    }

    // Esta é a rota que o seu botão na Home chama agora
    [HttpGet("boletim-local")]
public IActionResult BoletimLocal(double lat, double lon)
    {
        // Apenas abre a página. O JS da página vai ler lat e lon da URL.
        return View("Surf");
    }

    [HttpGet("surf")]
    public async Task<IActionResult> GetSurfReport(double lat = -26.89, double lon = -48.65)
    {
        // 1. Descobre a cidade (Opcional, mas legal para o prompt)
        string cidade = await _locationService.ObterCidadePorCoordenadas(lat, lon);

        // 2. Busca dados marítimos para o ponto exato
        var dadosBrutos = await _surfService.ObterDadosMaritimos(lat, lon);

        string promptComDados = $@"Você é o Zero, surfista local experiente de {cidade}. 
        Analise estes dados técnicos: {dadosBrutos}.

        INSTRUÇÃO CRÍTICA: 
        - Procure por valores de 'windspeed', 'winddirection', 'wave_height' e 'weathercode' nos dados acima.
        - Converta a direção do vento de graus para pontos cardeais (ex: 270° é Oeste/West).
        - Se o 'weathercode' for 0 ou 1, o tempo está Limpo/Ensolarado.
        - NÃO responda 'Não fornecido'. Se não achar o valor exato, dê uma estimativa baseada no contexto dos dados.

        Responda EXATAMENTE neste formato:

        Fala brother, [frase curta de local]! A previsão do dia é essa aqui:

        TAMANHO: [X.Xm]
        PERÍODO: [Xs]
        DIREÇÃO DO VENTO: [Ponto Cardeal]
        VELOCIDADE: [X km/h]
        PREVISÃO DO TEMPO: [Clima]

        E o que isso quer dizer? [Veredito final sobre o surf]";

        var respostaIA = await _geminiService.Perguntar(promptComDados);

        var resposta = await _geminiService.Perguntar(promptComDados);
        return Ok(new { boletim = resposta, cidade = cidade });
    }

    [HttpGet("surf/view")]
    public IActionResult SurfView()
    {
        return View("Surf");
    }
}