using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CampusWorkshops.Api.Services;

namespace CampusWorkshops.Api.Controllers;

[ApiController]
[AllowAnonymous] // Deixe aberto para facilitar o teste
[Route("api/[controller]")]
public class DiDemoController : ControllerBase
{
    // Visualiza os IDs por lifetime
    [HttpGet("lifetimes")]
    public IActionResult Lifetimes(
        [FromServices] IRequestIdTransient t1,
        [FromServices] IRequestIdTransient t2,
        [FromServices] IRequestIdScoped    s1,
        [FromServices] IRequestIdScoped    s2,
        [FromServices] IRequestIdSingleton g1,
        [FromServices] IRequestIdSingleton g2)
    {
        return Ok(new {
            transient1 = t1.Id, transient2 = t2.Id,    // normalmente diferentes NA MESMA chamada
            scoped1 = s1.Id, scoped2 = s2.Id,          // iguais NA MESMA chamada; mudam entre chamadas
            singleton1 = g1.Id, singleton2 = g2.Id     // sempre iguais
        });
    }

    // Demonstra o "cativeiro" do transient dentro de um singleton
    [HttpGet("captive-transient")]
    public IActionResult Captive(
        [FromServices] ReportingSingleton singleton,
        [FromServices] IPerRequestClock clockTransient)
    {
        // clockTransient é Transient: CreatedAt muda entre requisições
        var transientCreatedAtNow = clockTransient.CreatedAt;

        // GetClockCreatedAt (TODO) deveria retornar o CreatedAt visto pelo singleton
        DateTimeOffset? singletonClockCreatedAt = null;
        string? error = null;
        try
        {
            singletonClockCreatedAt = singleton.GetClockCreatedAt();
        }
        catch (Exception ex)
        {
            error = $"{ex.GetType().Name}: {ex.Message}";
        }

        return Ok(new {
            transientCreatedAtNow,
            singletonClockCreatedAt,
            esperado = "Valores DIFERENTES por requisição",
            observacao = "Mas como o transient foi resolvido DENTRO do singleton, ele congelou e não muda",
            todoImplementado = error is null,
            errorAoChamarSingleton = error
        });
    }
}
