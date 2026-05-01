using Microsoft.AspNetCore.Mvc.RazorPages;
using ITGSA.Frontend.Models;
using ITGSA.Frontend.Services;

namespace ITGSA.Frontend.Pages.Peticiones;

public class EstadoCuentaModel : PageModel
{
    private readonly ApiClient _api;
    public EstadoCuentaModel(ApiClient api) { _api = api; }

    public List<EstadoCuentaCliente> Estados { get; set; } = new();
    public string? NitConsultado { get; set; }
    public string? Error { get; set; }
    public bool Buscado { get; set; }

    public async Task OnGetAsync(string? nit)
    {
        NitConsultado = nit;
        if (Request.Query.ContainsKey("nit"))
        {
            Buscado = true;
            try { Estados = await _api.ObtenerEstadoCuenta(nit); }
            catch (Exception ex) { Error = $"Error al conectar con la API: {ex.Message}"; }
        }
    }
}
