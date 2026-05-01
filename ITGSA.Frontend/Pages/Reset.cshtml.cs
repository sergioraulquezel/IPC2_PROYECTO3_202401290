using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ITGSA.Frontend.Services;

namespace ITGSA.Frontend.Pages;

public class ResetModel : PageModel
{
    private readonly ApiClient _api;
    public ResetModel(ApiClient api) { _api = api; }

    public async Task<IActionResult> OnPostAsync()
    {
        await _api.LimpiarDatos();
        return RedirectToPage("/Configuracion/Index");
    }
}
