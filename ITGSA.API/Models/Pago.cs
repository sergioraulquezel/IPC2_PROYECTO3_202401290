namespace ITGSA.API.Models;

public class Pago
{
    public int CodigoBanco { get; set; }
    public string NombreBanco { get; set; } = string.Empty;
    public DateTime Fecha { get; set; }
    public string NITCliente { get; set; } = string.Empty;
    public decimal Valor { get; set; }
}
