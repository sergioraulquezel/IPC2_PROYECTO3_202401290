using Microsoft.AspNetCore.Mvc;
using ITGSA.API.Helpers;
using ITGSA.API.Services;

namespace ITGSA.API.Controllers;

[ApiController]
[Route("[controller]")]
public class ConfiguracionController : ControllerBase
{
    private readonly ConfiguracionService _service;

    public ConfiguracionController(ConfiguracionService service)
    {
        _service = service;
    }

    [HttpPost("/grabarConfiguracion")]
    public IActionResult GrabarConfiguracion(IFormFile archivo)
    {
        if (archivo == null || archivo.Length == 0)
            return BadRequest("<error>Archivo no proporcionado</error>");

        try
        {
            using var stream = archivo.OpenReadStream();
            var (clientes, bancos) = XmlHelper.ParsearConfig(stream);
            var respuesta = _service.ProcesarConfiguracion(clientes, bancos);
            var xml = XmlHelper.GenerarRespuestaConfig(respuesta);
            return Content(xml, "application/xml");
        }
        catch (Exception ex)
        {
            return BadRequest($"<error>{ex.Message}</error>");
        }
    }
}
