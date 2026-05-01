using System.Xml.Linq;
using ITGSA.API.Models;

namespace ITGSA.API.Data;

public class DataStore
{
    private readonly string _dataPath;
    private readonly string _clientesFile;
    private readonly string _bancosFile;
    private readonly string _facturasFile;
    private readonly string _pagosFile;

    public DataStore(IWebHostEnvironment env)
    {
        _dataPath = Path.Combine(env.ContentRootPath, "Data", "Storage");
        Directory.CreateDirectory(_dataPath);
        _clientesFile = Path.Combine(_dataPath, "clientes.xml");
        _bancosFile = Path.Combine(_dataPath, "bancos.xml");
        _facturasFile = Path.Combine(_dataPath, "facturas.xml");
        _pagosFile = Path.Combine(_dataPath, "pagos.xml");
        InicializarArchivos();
    }

    private void InicializarArchivos()
    {
        if (!File.Exists(_clientesFile))
            new XDocument(new XDeclaration("1.0", "utf-8", null), new XElement("clientes")).Save(_clientesFile);
        if (!File.Exists(_bancosFile))
            new XDocument(new XDeclaration("1.0", "utf-8", null), new XElement("bancos")).Save(_bancosFile);
        if (!File.Exists(_facturasFile))
            new XDocument(new XDeclaration("1.0", "utf-8", null), new XElement("facturas")).Save(_facturasFile);
        if (!File.Exists(_pagosFile))
            new XDocument(new XDeclaration("1.0", "utf-8", null), new XElement("pagos")).Save(_pagosFile);
    }

    public void LimpiarDatos()
    {
        new XDocument(new XDeclaration("1.0", "utf-8", null), new XElement("clientes")).Save(_clientesFile);
        new XDocument(new XDeclaration("1.0", "utf-8", null), new XElement("bancos")).Save(_bancosFile);
        new XDocument(new XDeclaration("1.0", "utf-8", null), new XElement("facturas")).Save(_facturasFile);
        new XDocument(new XDeclaration("1.0", "utf-8", null), new XElement("pagos")).Save(_pagosFile);
    }

    public List<Cliente> ObtenerClientes()
    {
        var doc = XDocument.Load(_clientesFile);
        return doc.Descendants("cliente").Select(e => new Cliente
        {
            NIT = e.Element("NIT")?.Value ?? string.Empty,
            Nombre = e.Element("nombre")?.Value ?? string.Empty,
            SaldoFavor = decimal.Parse(e.Element("saldoFavor")?.Value ?? "0",
                System.Globalization.CultureInfo.InvariantCulture)
        }).ToList();
    }

    public void GuardarClientes(List<Cliente> clientes)
    {
        var doc = new XDocument(new XDeclaration("1.0", "utf-8", null),
            new XElement("clientes",
                clientes.Select(c => new XElement("cliente",
                    new XElement("NIT", c.NIT),
                    new XElement("nombre", c.Nombre),
                    new XElement("saldoFavor", c.SaldoFavor.ToString("F2",
                        System.Globalization.CultureInfo.InvariantCulture))
                ))
            )
        );
        doc.Save(_clientesFile);
    }

    public List<Banco> ObtenerBancos()
    {
        var doc = XDocument.Load(_bancosFile);
        return doc.Descendants("banco").Select(e => new Banco
        {
            Codigo = int.Parse(e.Element("codigo")?.Value ?? "0"),
            Nombre = e.Element("nombre")?.Value ?? string.Empty
        }).ToList();
    }

    public void GuardarBancos(List<Banco> bancos)
    {
        var doc = new XDocument(new XDeclaration("1.0", "utf-8", null),
            new XElement("bancos",
                bancos.Select(b => new XElement("banco",
                    new XElement("codigo", b.Codigo),
                    new XElement("nombre", b.Nombre)
                ))
            )
        );
        doc.Save(_bancosFile);
    }

    public List<Factura> ObtenerFacturas()
    {
        var doc = XDocument.Load(_facturasFile);
        return doc.Descendants("factura").Select(e => new Factura
        {
            NumeroFactura = e.Element("numeroFactura")?.Value ?? string.Empty,
            NITCliente = e.Element("NITCliente")?.Value ?? string.Empty,
            Fecha = DateTime.ParseExact(e.Element("fecha")?.Value ?? "01/01/2000", "dd/MM/yyyy",
                System.Globalization.CultureInfo.InvariantCulture),
            Valor = decimal.Parse(e.Element("valor")?.Value ?? "0",
                System.Globalization.CultureInfo.InvariantCulture),
            SaldoPendiente = decimal.Parse(e.Element("saldoPendiente")?.Value ?? "0",
                System.Globalization.CultureInfo.InvariantCulture),
            Pagada = bool.Parse(e.Element("pagada")?.Value ?? "false")
        }).ToList();
    }

    public void GuardarFacturas(List<Factura> facturas)
    {
        var doc = new XDocument(new XDeclaration("1.0", "utf-8", null),
            new XElement("facturas",
                facturas.Select(f => new XElement("factura",
                    new XElement("numeroFactura", f.NumeroFactura),
                    new XElement("NITCliente", f.NITCliente),
                    new XElement("fecha", f.Fecha.ToString("dd/MM/yyyy")),
                    new XElement("valor", f.Valor.ToString("F2",
                        System.Globalization.CultureInfo.InvariantCulture)),
                    new XElement("saldoPendiente", f.SaldoPendiente.ToString("F2",
                        System.Globalization.CultureInfo.InvariantCulture)),
                    new XElement("pagada", f.Pagada.ToString().ToLower())
                ))
            )
        );
        doc.Save(_facturasFile);
    }

    public List<Pago> ObtenerPagos()
    {
        var doc = XDocument.Load(_pagosFile);
        return doc.Descendants("pago").Select(e => new Pago
        {
            CodigoBanco = int.Parse(e.Element("codigoBanco")?.Value ?? "0"),
            NombreBanco = e.Element("nombreBanco")?.Value ?? string.Empty,
            Fecha = DateTime.ParseExact(e.Element("fecha")?.Value ?? "01/01/2000", "dd/MM/yyyy",
                System.Globalization.CultureInfo.InvariantCulture),
            NITCliente = e.Element("NITCliente")?.Value ?? string.Empty,
            Valor = decimal.Parse(e.Element("valor")?.Value ?? "0",
                System.Globalization.CultureInfo.InvariantCulture)
        }).ToList();
    }

    public void GuardarPagos(List<Pago> pagos)
    {
        var doc = new XDocument(new XDeclaration("1.0", "utf-8", null),
            new XElement("pagos",
                pagos.Select(p => new XElement("pago",
                    new XElement("codigoBanco", p.CodigoBanco),
                    new XElement("nombreBanco", p.NombreBanco),
                    new XElement("fecha", p.Fecha.ToString("dd/MM/yyyy")),
                    new XElement("NITCliente", p.NITCliente),
                    new XElement("valor", p.Valor.ToString("F2",
                        System.Globalization.CultureInfo.InvariantCulture))
                ))
            )
        );
        doc.Save(_pagosFile);
    }
}
