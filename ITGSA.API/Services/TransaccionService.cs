using ITGSA.API.Data;
using ITGSA.API.Models;

namespace ITGSA.API.Services;

public class TransaccionService
{
    private readonly DataStore _store;

    public TransaccionService(DataStore store)
    {
        _store = store;
    }

    public RespuestaTransacciones ProcesarTransacciones(List<Factura> facturasNuevas, List<Pago> pagosNuevos)
    {
        var clientes = _store.ObtenerClientes();
        var bancos = _store.ObtenerBancos();
        var facturasExistentes = _store.ObtenerFacturas();
        var pagosExistentes = _store.ObtenerPagos();

        int nuevasFacturas = 0;
        int facturasDuplicadas = 0;
        int facturasConError = 0;
        int nuevosPagos = 0;
        int pagosDuplicados = 0;
        int pagosConError = 0;

        foreach (var factura in facturasNuevas)
        {
            bool duplicada = facturasExistentes.Any(f =>
                f.NumeroFactura.Equals(factura.NumeroFactura, StringComparison.OrdinalIgnoreCase));

            if (duplicada)
            {
                facturasDuplicadas++;
                continue;
            }

            bool clienteExiste = clientes.Any(c =>
                c.NIT.Equals(factura.NITCliente, StringComparison.OrdinalIgnoreCase));

            if (!clienteExiste || factura.Fecha == DateTime.MinValue || factura.Valor <= 0)
            {
                facturasConError++;
                continue;
            }

            var cliente = clientes.First(c =>
                c.NIT.Equals(factura.NITCliente, StringComparison.OrdinalIgnoreCase));

            if (cliente.SaldoFavor > 0)
            {
                decimal abono = Math.Min(cliente.SaldoFavor, factura.Valor);
                factura.SaldoPendiente = factura.Valor - abono;
                cliente.SaldoFavor -= abono;

                if (factura.SaldoPendiente <= 0)
                    factura.Pagada = true;
            }

            facturasExistentes.Add(factura);
            nuevasFacturas++;
        }

        foreach (var pago in pagosNuevos)
        {
            bool bancoExiste = bancos.Any(b => b.Codigo == pago.CodigoBanco);
            bool clienteExiste = clientes.Any(c =>
                c.NIT.Equals(pago.NITCliente, StringComparison.OrdinalIgnoreCase));

            if (!bancoExiste || !clienteExiste || pago.Fecha == DateTime.MinValue || pago.Valor <= 0)
            {
                pagosConError++;
                continue;
            }

            bool duplicado = pagosExistentes.Any(p =>
                p.CodigoBanco == pago.CodigoBanco &&
                p.NITCliente.Equals(pago.NITCliente, StringComparison.OrdinalIgnoreCase) &&
                p.Fecha == pago.Fecha &&
                p.Valor == pago.Valor);

            if (duplicado)
            {
                pagosDuplicados++;
                continue;
            }

            var banco = bancos.First(b => b.Codigo == pago.CodigoBanco);
            pago.NombreBanco = banco.Nombre;

            var cliente = clientes.First(c =>
                c.NIT.Equals(pago.NITCliente, StringComparison.OrdinalIgnoreCase));

            decimal montoPendiente = pago.Valor;

            var facturasCliente = facturasExistentes
                .Where(f => f.NITCliente.Equals(pago.NITCliente, StringComparison.OrdinalIgnoreCase) && !f.Pagada)
                .OrderBy(f => f.Fecha)
                .ToList();

            foreach (var factura in facturasCliente)
            {
                if (montoPendiente <= 0) break;

                if (montoPendiente >= factura.SaldoPendiente)
                {
                    montoPendiente -= factura.SaldoPendiente;
                    factura.SaldoPendiente = 0;
                    factura.Pagada = true;
                }
                else
                {
                    factura.SaldoPendiente -= montoPendiente;
                    montoPendiente = 0;
                }
            }

            if (montoPendiente > 0)
                cliente.SaldoFavor += montoPendiente;

            pagosExistentes.Add(pago);
            nuevosPagos++;
        }

        _store.GuardarClientes(clientes);
        _store.GuardarFacturas(facturasExistentes);
        _store.GuardarPagos(pagosExistentes);

        return new RespuestaTransacciones
        {
            NuevasFacturas = nuevasFacturas,
            FacturasDuplicadas = facturasDuplicadas,
            FacturasConError = facturasConError,
            NuevosPagos = nuevosPagos,
            PagosDuplicados = pagosDuplicados,
            PagosConError = pagosConError
        };
    }
}
