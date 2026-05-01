namespace ITGSA.API.Models;

public class Cliente
{
    public string NIT { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public decimal SaldoFavor { get; set; } = 0;
    public List<Factura> Facturas { get; set; } = new();
    public List<Pago> Pagos { get; set; } = new();
}
