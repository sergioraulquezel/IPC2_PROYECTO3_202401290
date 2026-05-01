using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;
using ITGSA.Frontend.Models;
using ITGSA.Frontend.Services;

namespace ITGSA.Frontend.Pages.Peticiones;

public class IngresosModel : PageModel
{
    private readonly ApiClient _api;
    public IngresosModel(ApiClient api) { _api = api; }

    public ResumenPagosViewModel? Resumen { get; set; }
    public int MesElegido { get; set; } = DateTime.Now.Month;
    public int AnioElegido { get; set; } = DateTime.Now.Year;
    public string? Error { get; set; }
    public string ChartDataJson { get; set; } = "{}";

    public async Task OnGetAsync(int mes = 0, int anio = 0)
    {
        MesElegido = mes > 0 ? mes : DateTime.Now.Month;
        AnioElegido = anio > 0 ? anio : DateTime.Now.Year;

        if (mes > 0)
        {
            try
            {
                Resumen = await _api.ObtenerResumenPagos(MesElegido, AnioElegido);
                if (Resumen != null) ChartDataJson = BuildChartJson(Resumen);
            }
            catch (Exception ex) { Error = $"Error al conectar con la API: {ex.Message}"; }
        }
    }

    private static string BuildChartJson(ResumenPagosViewModel resumen)
    {
        var periodos = resumen.Bancos
            .SelectMany(b => b.Periodos.Select(p => p.Periodo))
            .Distinct().OrderByDescending(x => x).ToList();

        var bancos = resumen.Bancos.Select(b => new
        {
            nombre = b.Nombre,
            totales = periodos.Select(p =>
                b.Periodos.FirstOrDefault(x => x.Periodo == p)?.Total ?? 0).ToList()
        }).ToList();

        return JsonSerializer.Serialize(new { periodos, bancos });
    }
}
