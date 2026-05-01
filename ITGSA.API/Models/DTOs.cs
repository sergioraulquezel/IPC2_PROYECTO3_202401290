namespace ITGSA.API.Models;

public class RespuestaConfig
{
    public int ClientesCreados { get; set; }
    public int ClientesActualizados { get; set; }
    public int BancosCreados { get; set; }
    public int BancosActualizados { get; set; }
}

public class RespuestaTransacciones
{
    public int NuevasFacturas { get; set; }
    public int FacturasDuplicadas { get; set; }
    public int FacturasConError { get; set; }
    public int NuevosPagos { get; set; }
    public int PagosDuplicados { get; set; }
    public int PagosConError { get; set; }
}

public class TransaccionEstadoCuenta
{
    public DateTime Fecha { get; set; }
    public decimal? Cargo { get; set; }
    public decimal? Abono { get; set; }
    public string Descripcion { get; set; } = string.Empty;
}

public class EstadoCuentaCliente
{
    public string NIT { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public decimal SaldoActual { get; set; }
    public List<TransaccionEstadoCuenta> Transacciones { get; set; } = new();
}

public class ResumenPagoMes
{
    public string Banco { get; set; } = string.Empty;
    public int CodigoBanco { get; set; }
    public decimal TotalPagado { get; set; }
    public string Mes { get; set; } = string.Empty;
    public int Anio { get; set; }
}
