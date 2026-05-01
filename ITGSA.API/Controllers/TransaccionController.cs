using Microsoft.AspNetCore.Mvc;
using ITGSA.API.Helpers;
using ITGSA.API.Services;

namespace ITGSA.API.Controllers;

[ApiController]
[Route("[controller]")]
public class TransaccionController : ControllerBase
{
    private readonly TransaccionService _service;

    public TransaccionController(TransaccionService service)
    {
        _service = service;
    }

    [HttpPost("/grabarTransaccion")]
    public IActionResult GrabarTransaccion(IFormFile archivo)
    {
        if (archivo == null || archivo.Length == 0)
            return BadRequest("<e>Archivo no proporcionado</e>");

        try
        {
            using var stream = archivo.OpenReadStream();
            var (facturas, pagos) = XmlHelper.ParsearTransacciones(stream);
            var respuesta = _service.ProcesarTransacciones(facturas, pagos);
            var xml = XmlHelper.GenerarRespuestaTransacciones(respuesta);
            return Content(xml, "application/xml");
        }
        catch (Exception ex)
        {
            return BadRequest($"<e>{ex.Message}</e>");
        }
    }
}
