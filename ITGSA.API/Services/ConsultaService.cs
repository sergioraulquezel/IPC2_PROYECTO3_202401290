using ITGSA.API.Data;
using ITGSA.API.Models;

namespace ITGSA.API.Services;

public class ConsultaService
{
    private readonly DataStore _store;

    public ConsultaService(DataStore store)
    {
        _store = store;
    }

    public List<EstadoCuentaCliente> ObtenerEstadoCuenta(string? nit)
    {
        var clientes = _store.ObtenerClientes();
        var facturas = _store.ObtenerFacturas();
        var pagos = _store.ObtenerPagos();
        var bancos = _store.ObtenerBancos();

        IEnumerable<Cliente> seleccion = string.IsNullOrWhiteSpace(nit)
            ? clientes.OrderBy(c => c.NIT)
            : clientes.Where(c => c.NIT.Equals(nit, StringComparison.OrdinalIgnoreCase));

        var resultado = new List<EstadoCuentaCliente>();

        foreach (var cliente in seleccion)
        {
            var transacciones = new List<TransaccionEstadoCuenta>();

            var facturasCliente = facturas
                .Where(f => f.NITCliente.Equals(cliente.NIT, StringComparison.OrdinalIgnoreCase))
                .ToList();

            foreach (var f in facturasCliente)
            {
                transacciones.Add(new TransaccionEstadoCuenta
                {
                    Fecha = f.Fecha,
                    Cargo = f.Valor,
                    Abono = null,
                    Descripcion = $"Fact. # {f.NumeroFactura}"
                });
            }

            var pagosCliente = pagos
                .Where(p => p.NITCliente.Equals(cliente.NIT, StringComparison.OrdinalIgnoreCase))
                .ToList();

            foreach (var p in pagosCliente)
            {
                var banco = bancos.FirstOrDefault(b => b.Codigo == p.CodigoBanco);
                string nombreBanco = banco?.Nombre ?? p.NombreBanco;

                transacciones.Add(new TransaccionEstadoCuenta
                {
                    Fecha = p.Fecha,
                    Cargo = null,
                    Abono = p.Valor,
                    Descripcion = nombreBanco
                });
            }

            transacciones = transacciones.OrderByDescending(t => t.Fecha).ToList();

            decimal saldo = facturasCliente.Sum(f => f.SaldoPendiente) - cliente.SaldoFavor;

            resultado.Add(new EstadoCuentaCliente
            {
                NIT = cliente.NIT,
                Nombre = cliente.Nombre,
                SaldoActual = saldo,
                Transacciones = transacciones
            });
        }

        return resultado;
    }

    public List<ResumenPagoMes> ObtenerResumenPagos(int mes, int anio)
    {
        var pagos = _store.ObtenerPagos();
        var bancos = _store.ObtenerBancos();

        var meses = new List<(int mes, int anio)>();
        for (int i = 0; i < 3; i++)
        {
            var fecha = new DateTime(anio, mes, 1).AddMonths(-i);
            meses.Add((fecha.Month, fecha.Year));
        }

        var resultado = new List<ResumenPagoMes>();

        foreach (var (m, a) in meses)
        {
            var pagosMes = pagos.Where(p => p.Fecha.Month == m && p.Fecha.Year == a).ToList();

            var agrupados = pagosMes
                .GroupBy(p => p.CodigoBanco)
                .Select(g =>
                {
                    var banco = bancos.FirstOrDefault(b => b.Codigo == g.Key);
                    return new ResumenPagoMes
                    {
                        CodigoBanco = g.Key,
                        Banco = banco?.Nombre ?? g.First().NombreBanco,
                        TotalPagado = g.Sum(p => p.Valor),
                        Mes = m.ToString(),
                        Anio = a
                    };
                });

            resultado.AddRange(agrupados);
        }

        return resultado;
    }
}
