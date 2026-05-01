using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ITGSA.Frontend.Models;
using ITGSA.Frontend.Services;

namespace ITGSA.Frontend.Pages.Transacciones;

public class IndexModel : PageModel
{
    private readonly ApiClient _api;
    public IndexModel(ApiClient api) { _api = api; }

    public RespuestaTransacciones? Respuesta { get; set; }
    public string? Error { get; set; }

    public void OnGet() { }

    public async Task<IActionResult> OnPostAsync(IFormFile archivo)
    {
        if (archivo == null || archivo.Length == 0 ||
            !archivo.FileName.EndsWith(".xml", StringComparison.OrdinalIgnoreCase))
        {
            Error = "Selecciona un archivo .xml válido.";
            return Page();
        }
        try
        {
            Respuesta = await _api.GrabarTransacciones(archivo);
            if (Respuesta == null) Error = "No se pudo procesar la respuesta del servidor.";
        }
        catch (Exception ex) { Error = $"Error al conectar con la API: {ex.Message}"; }
        return Page();
    }
}
