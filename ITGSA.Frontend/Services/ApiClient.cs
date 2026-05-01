using System.Net.Http.Headers;
using System.Xml.Linq;
using ITGSA.Frontend.Models;

namespace ITGSA.Frontend.Services;

public class ApiClient
{
    private readonly HttpClient _http;

    public ApiClient(IHttpClientFactory factory)
    {
        _http = factory.CreateClient("API");
    }

    public async Task<RespuestaConfig?> GrabarConfiguracion(IFormFile archivo)
    {
        using var content = new MultipartFormDataContent();
        using var stream = archivo.OpenReadStream();
        var fileContent = new StreamContent(stream);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/xml");
        content.Add(fileContent, "archivo", archivo.FileName);
        var response = await _http.PostAsync("/grabarConfiguracion", content);
        var xml = await response.Content.ReadAsStringAsync();
        return ParseConfig(xml);
    }

    public async Task<RespuestaTransacciones?> GrabarTransacciones(IFormFile archivo)
    {
        using var content = new MultipartFormDataContent();
        using var stream = archivo.OpenReadStream();
        var fileContent = new StreamContent(stream);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/xml");
        content.Add(fileContent, "archivo", archivo.FileName);
        var response = await _http.PostAsync("/grabarTransaccion", content);
        var xml = await response.Content.ReadAsStringAsync();
        return ParseTransacciones(xml);
    }

    public async Task<bool> LimpiarDatos()
    {
        var response = await _http.PostAsync("/limpiarDatos", null);
        return response.IsSuccessStatusCode;
    }

    public async Task<List<EstadoCuentaCliente>> ObtenerEstadoCuenta(string? nit)
    {
        var url = string.IsNullOrWhiteSpace(nit)
            ? "/devolverEstadoCuenta"
            : $"/devolverEstadoCuenta?nit={Uri.EscapeDataString(nit)}";
        var response = await _http.GetAsync(url);
        var xml = await response.Content.ReadAsStringAsync();
        return ParseEstadoCuenta(xml);
    }

    public async Task<ResumenPagosViewModel?> ObtenerResumenPagos(int mes, int anio)
    {
        var response = await _http.GetAsync($"/devolverResumenPagos?mes={mes}&anio={anio}");
        var xml = await response.Content.ReadAsStringAsync();
        return ParseResumenPagos(xml);
    }

    private static RespuestaConfig? ParseConfig(string xml)
    {
        try
        {
            var doc = XDocument.Parse(xml);
            return new RespuestaConfig
            {
                ClientesCreados = int.Parse(doc.Descendants("creados").First().Value),
                ClientesActualizados = int.Parse(doc.Descendants("actualizados").First().Value),
                BancosCreados = int.Parse(doc.Descendants("creados").Last().Value),
                BancosActualizados = int.Parse(doc.Descendants("actualizados").Last().Value)
            };
        }
        catch { return null; }
    }

    private static RespuestaTransacciones? ParseTransacciones(string xml)
    {
        try
        {
            var doc = XDocument.Parse(xml);
            return new RespuestaTransacciones
            {
                NuevasFacturas = int.Parse(doc.Descendants("nuevasFacturas").First().Value),
                FacturasDuplicadas = int.Parse(doc.Descendants("facturasDuplicadas").First().Value),
                FacturasConError = int.Parse(doc.Descendants("facturasConError").First().Value),
                NuevosPagos = int.Parse(doc.Descendants("nuevosPagos").First().Value),
                PagosDuplicados = int.Parse(doc.Descendants("pagosDuplicados").First().Value),
                PagosConError = int.Parse(doc.Descendants("pagosConError").First().Value)
            };
        }
        catch { return null; }
    }

    private static List<EstadoCuentaCliente> ParseEstadoCuenta(string xml)
    {
        var lista = new List<EstadoCuentaCliente>();
        try
        {
            var doc = XDocument.Parse(xml);
            foreach (var el in doc.Descendants("cliente"))
            {
                var cliente = new EstadoCuentaCliente
                {
                    NIT = el.Element("NIT")?.Value ?? string.Empty,
                    Nombre = el.Element("nombre")?.Value ?? string.Empty,
                    SaldoActual = decimal.Parse(el.Element("saldoActual")?.Value ?? "0",
                        System.Globalization.CultureInfo.InvariantCulture)
                };
                foreach (var t in el.Descendants("transaccion"))
                {
                    var cargoStr = t.Element("cargo")?.Value;
                    var abonoStr = t.Element("abono")?.Value;
                    cliente.Transacciones.Add(new TransaccionEstadoCuenta
                    {
                        Fecha = t.Element("fecha")?.Value ?? string.Empty,
                        Descripcion = t.Element("descripcion")?.Value ?? string.Empty,
                        Cargo = string.IsNullOrWhiteSpace(cargoStr) ? null :
                            decimal.Parse(cargoStr, System.Globalization.CultureInfo.InvariantCulture),
                        Abono = string.IsNullOrWhiteSpace(abonoStr) ? null :
                            decimal.Parse(abonoStr, System.Globalization.CultureInfo.InvariantCulture)
                    });
                }
                lista.Add(cliente);
            }
        }
        catch { }
        return lista;
    }

    private static ResumenPagosViewModel? ParseResumenPagos(string xml)
    {
        try
        {
            var doc = XDocument.Parse(xml);
            var vm = new ResumenPagosViewModel
            {
                MesElegido = doc.Descendants("mesElegido").FirstOrDefault()?.Value ?? string.Empty
            };
            foreach (var b in doc.Descendants("banco"))
            {
                var banco = new ResumenBanco
                {
                    CodigoBanco = int.Parse(b.Element("codigo")?.Value ?? "0"),
                    Nombre = b.Element("nombre")?.Value ?? string.Empty
                };
                foreach (var p in b.Elements("pago"))
                {
                    banco.Periodos.Add(new PeriodoPago
                    {
                        Periodo = p.Element("periodo")?.Value ?? string.Empty,
                        Total = decimal.Parse(p.Element("total")?.Value ?? "0",
                            System.Globalization.CultureInfo.InvariantCulture)
                    });
                }
                vm.Bancos.Add(banco);
            }
            return vm;
        }
        catch { return null; }
    }
}
