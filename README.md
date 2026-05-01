# ITGSA — Sistema de Facturación y Pagos

Sistema de gestión del canal de distribución On-Line de **Industria Típica Guatemalteca, S.A.** Controla facturas generadas por la Tienda Virtual y pagos realizados a través de bancas virtuales.

Proyecto 3 — Introducción a la Programación y Computación 2 | 1S 2026

---

## Tecnologías

- .NET 9
- ASP.NET Core Web API (Backend)
- ASP.NET Core Razor Pages (Frontend)
- Persistencia en archivos XML
- Chart.js 4.4.0

---

## Estructura del repositorio

```
ITGSA/
├── ITGSA.API/          # Backend — API REST
└── ITGSA.Frontend/     # Frontend — Razor Pages
```

---

## Requisitos

- .NET 9 SDK
- Puertos 5000 y 5001 disponibles
- Conexión a internet (Chart.js se carga desde CDN)

---

## Ejecutar el proyecto

**1. Backend**
```bash
cd ITGSA.API
dotnet run
# Disponible en http://localhost:5000
```

**2. Frontend**
```bash
cd ITGSA.Frontend
dotnet run
# Disponible en http://localhost:5001
```

---

## Endpoints del API

| Método | Endpoint | Descripción |
|--------|----------|-------------|
| POST | `/grabarConfiguracion` | Carga clientes y bancos desde `config.xml` |
| POST | `/grabarTransaccion` | Carga facturas y pagos desde `transac.xml` |
| POST | `/limpiarDatos` | Elimina todos los datos del sistema |
| GET | `/devolverEstadoCuenta?nit=` | Estado de cuenta de un cliente o todos |
| GET | `/devolverResumenPagos?mes=&anio=` | Resumen de pagos por banco (últimos 3 meses) |

---

## Archivos de entrada

**config.xml** — Registra clientes y bancos (incremental, actualiza si ya existe)
```xml
<config>
  <clientes>
    <cliente>
      <NIT>399399K</NIT>
      <nombre>Almacen Guatemalteco</nombre>
    </cliente>
  </clientes>
  <bancos>
    <banco>
      <codigo>1</codigo>
      <nombre>Banco Industrial</nombre>
    </banco>
  </bancos>
</config>
```

**transac.xml** — Registra facturas y pagos (incremental, no permite actualizar)
```xml
<transacciones>
  <facturas>
    <factura>
      <numeroFactura>F-001</numeroFactura>
      <NITcliente>399399K</NITcliente>
      <fecha>01/03/2026</fecha>
      <valor>1500.00</valor>
    </factura>
  </facturas>
  <pagos>
    <pago>
      <codigoBanco>1</codigoBanco>
      <fecha>05/03/2026</fecha>
      <NITcliente>399399K</NITcliente>
      <valor>1500.00</valor>
    </pago>
  </pagos>
</transacciones>
```

---

## Lógica de pagos

- Los pagos se aplican a la factura más antigua con saldo pendiente primero.
- Si el pago supera el total de facturas pendientes, el excedente queda como **saldo a favor**.
- El saldo a favor se aplica automáticamente en la siguiente factura del cliente.

---

## Persistencia

Los datos se almacenan en `ITGSA.API/Data/Storage/`:

```
clientes.xml
bancos.xml
facturas.xml
pagos.xml
```

---

## Autor

**Tu Nombre** — Carné: 000000000  
Facultad de Ingeniería — Universidad de San Carlos de Guatemala
