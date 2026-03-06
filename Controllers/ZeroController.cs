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
}