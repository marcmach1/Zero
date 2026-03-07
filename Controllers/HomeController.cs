using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Zero.Models;
using Zero.Services; // Importante para o Controller enxergar o serviço

namespace Zero.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly SurfService _surfService;

    // O .NET injeta o Logger e o SurfService automaticamente aqui
    public HomeController(ILogger<HomeController> logger, SurfService surfService)
    {
        _logger = logger;
        _surfService = surfService;
    }

    public async Task<IActionResult> Index()
    {
        // 1. Busca os dados de Navegantes
        var dadosDoMar = await _surfService.ObterDadosMaritimos(-26.89, -48.65);

        // 2. Passamos os dados para a "sacola" (ViewBag) para exibir no HTML
        ViewBag.DadosMar = dadosDoMar;

        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}