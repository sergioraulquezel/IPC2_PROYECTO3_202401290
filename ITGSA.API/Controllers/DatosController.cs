using Microsoft.AspNetCore.Mvc;
using ITGSA.API.Data;
using ITGSA.API.Helpers;
using ITGSA.API.Services;

namespace ITGSA.API.Controllers;

[ApiController]
[Route("[controller]")]
public class DatosController : ControllerBase
{
    private readonly DataStore _store;
    private readonly ConsultaService _consultaService;

    public DatosController(DataStore store, ConsultaService consultaService)
    {
        _store = store;
        _consultaService = consultaService;
    }

    [HttpPost("/limpiarDatos")]
    public IActionResult LimpiarDatos()
    {
        _store.LimpiarDatos();
        return Content("<respuesta><mensaje>Datos eliminados correctamente</mensaje></respuesta>", "application/xml");
    }

    [HttpGet("/devolverEstadoCuenta")]
    public IActionResult DevolverEstadoCuenta([FromQuery] string? nit)
    {
        var estados = _consultaService.ObtenerEstadoCuenta(nit);
        var xml = XmlHelper.GenerarEstadoCuenta(estados);
        return Content(xml, "application/xml");
    }

    [HttpGet("/devolverResumenPagos")]
    public IActionResult DevolverResumenPagos([FromQuery] int mes, [FromQuery] int anio)
    {
        if (mes < 1 || mes > 12 || anio < 2000)
            return BadRequest("<e>Mes o año inválido</e>");

        var resumen = _consultaService.ObtenerResumenPagos(mes, anio);
        var xml = XmlHelper.GenerarResumenPagos(resumen, mes, anio);
        return Content(xml, "application/xml");
    }
}
