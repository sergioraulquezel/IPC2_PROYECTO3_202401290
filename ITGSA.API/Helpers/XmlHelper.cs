using System.Xml.Linq;
using ITGSA.API.Models;

namespace ITGSA.API.Helpers;

public static class XmlHelper
{
    public static (List<Cliente> clientes, List<Banco> bancos) ParsearConfig(Stream stream)
    {
        var doc = XDocument.Load(stream);
        var clientes = new List<Cliente>();
        var bancos = new List<Banco>();

        foreach (var el in doc.Descendants("cliente"))
        {
            clientes.Add(new Cliente
            {
                NIT = el.Element("NIT")?.Value.Trim() ?? string.Empty,
                Nombre = el.Element("nombre")?.Value.Trim() ?? string.Empty
            });
        }

        foreach (var el in doc.Descendants("banco"))
        {
            var codigoStr = el.Element("codigo")?.Value.Trim() ?? "0";
            if (int.TryParse(codigoStr, out int codigo))
            {
                bancos.Add(new Banco
                {
                    Codigo = codigo,
                    Nombre = el.Element("nombre")?.Value.Trim() ?? string.Empty
                });
            }
        }

        return (clientes, bancos);
    }

    public static (List<Factura> facturas, List<Pago> pagos) ParsearTransacciones(Stream stream)
    {
        var doc = XDocument.Load(stream);
        var facturas = new List<Factura>();
        var pagos = new List<Pago>();

        foreach (var el in doc.Descendants("factura"))
        {
            var fechaStr = el.Element("fecha")?.Value.Trim() ?? string.Empty;
            var valorStr = el.Element("valor")?.Value.Trim() ?? "0";

            if (!DateTime.TryParseExact(fechaStr, "dd/MM/yyyy",
                System.Globalization.CultureInfo.InvariantCulture,
                System.Globalization.DateTimeStyles.None, out DateTime fecha))
                fecha = DateTime.MinValue;

            decimal.TryParse(valorStr,
                System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture, out decimal valor);

            facturas.Add(new Factura
            {
                NumeroFactura = el.Element("numeroFactura")?.Value.Trim() ?? string.Empty,
                NITCliente = el.Element("NITcliente")?.Value.Trim() ?? string.Empty,
                Fecha = fecha,
                Valor = valor,
                SaldoPendiente = valor
            });
        }

        foreach (var el in doc.Descendants("pago"))
        {
            var codigoStr = el.Element("codigoBanco")?.Value.Trim() ?? "0";
            var fechaStr = el.Element("fecha")?.Value.Trim() ?? string.Empty;
            var valorStr = el.Element("valor")?.Value.Trim() ?? "0";

            if (!int.TryParse(codigoStr, out int codigo)) codigo = 0;

            if (!DateTime.TryParseExact(fechaStr, "dd/MM/yyyy",
                System.Globalization.CultureInfo.InvariantCulture,
                System.Globalization.DateTimeStyles.None, out DateTime fecha))
                fecha = DateTime.MinValue;

            decimal.TryParse(valorStr,
                System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture, out decimal valor);

            pagos.Add(new Pago
            {
                CodigoBanco = codigo,
                Fecha = fecha,
                NITCliente = el.Element("NITcliente")?.Value.Trim() ?? string.Empty,
                Valor = valor
            });
        }

        return (facturas, pagos);
    }

    public static string GenerarRespuestaConfig(RespuestaConfig r)
    {
        var doc = new XDocument(
            new XDeclaration("1.0", "utf-8", null),
            new XElement("respuesta",
                new XElement("clientes",
                    new XElement("creados", r.ClientesCreados),
                    new XElement("actualizados", r.ClientesActualizados)),
                new XElement("bancos",
                    new XElement("creados", r.BancosCreados),
                    new XElement("actualizados", r.BancosActualizados))
            )
        );
        return doc.ToString();
    }

    public static string GenerarRespuestaTransacciones(RespuestaTransacciones r)
    {
        var doc = new XDocument(
            new XDeclaration("1.0", "utf-8", null),
            new XElement("transacciones",
                new XElement("facturas",
                    new XElement("nuevasFacturas", r.NuevasFacturas),
                    new XElement("facturasDuplicadas", r.FacturasDuplicadas),
                    new XElement("facturasConError", r.FacturasConError)),
                new XElement("pagos",
                    new XElement("nuevosPagos", r.NuevosPagos),
                    new XElement("pagosDuplicados", r.PagosDuplicados),
                    new XElement("pagosConError", r.PagosConError))
            )
        );
        return doc.ToString();
    }

    public static string GenerarEstadoCuenta(List<EstadoCuentaCliente> estados)
    {
        var root = new XElement("estadosCuenta");

        foreach (var e in estados)
        {
            var clienteEl = new XElement("cliente",
                new XElement("NIT", e.NIT),
                new XElement("nombre", e.Nombre),
                new XElement("saldoActual", e.SaldoActual.ToString("F2"))
            );

            var transEl = new XElement("transacciones");
            foreach (var t in e.Transacciones)
            {
                var tEl = new XElement("transaccion",
                    new XElement("fecha", t.Fecha.ToString("dd/MM/yyyy")),
                    new XElement("descripcion", t.Descripcion)
                );

                if (t.Cargo.HasValue)
                    tEl.Add(new XElement("cargo", t.Cargo.Value.ToString("F2")));
                else
                    tEl.Add(new XElement("cargo"));

                if (t.Abono.HasValue)
                    tEl.Add(new XElement("abono", t.Abono.Value.ToString("F2")));
                else
                    tEl.Add(new XElement("abono"));

                transEl.Add(tEl);
            }

            clienteEl.Add(transEl);
            root.Add(clienteEl);
        }

        var doc = new XDocument(new XDeclaration("1.0", "utf-8", null), root);
        return doc.ToString();
    }

    public static string GenerarResumenPagos(List<ResumenPagoMes> resumen, int mesElegido, int anioElegido)
    {
        var meses = new List<(int mes, int anio)>();
        for (int i = 0; i < 3; i++)
        {
            var fecha = new DateTime(anioElegido, mesElegido, 1).AddMonths(-i);
            meses.Add((fecha.Month, fecha.Year));
        }

        var bancosDistintos = resumen.Select(r => new { r.CodigoBanco, r.Banco }).Distinct().ToList();

        var root = new XElement("resumenPagos",
            new XElement("mesElegido", new DateTime(anioElegido, mesElegido, 1).ToString("MMMM/yyyy",
                new System.Globalization.CultureInfo("es-GT")))
        );

        var bancosEl = new XElement("bancos");
        foreach (var b in bancosDistintos)
        {
            var bancoEl = new XElement("banco",
                new XElement("codigo", b.CodigoBanco),
                new XElement("nombre", b.Banco)
            );

            foreach (var (mes, anio) in meses)
            {
                var total = resumen
                    .Where(r => r.CodigoBanco == b.CodigoBanco && r.Mes == mes.ToString() && r.Anio == anio)
                    .Sum(r => r.TotalPagado);

                var fechaLabel = new DateTime(anio, mes, 1).ToString("MMM-yy",
                    new System.Globalization.CultureInfo("es-GT"));

                bancoEl.Add(new XElement("pago",
                    new XElement("periodo", fechaLabel),
                    new XElement("total", total.ToString("F2"))
                ));
            }

            bancosEl.Add(bancoEl);
        }

        root.Add(bancosEl);
        var doc = new XDocument(new XDeclaration("1.0", "utf-8", null), root);
        return doc.ToString();
    }
}
