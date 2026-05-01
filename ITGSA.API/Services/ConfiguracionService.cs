using ITGSA.API.Data;
using ITGSA.API.Models;

namespace ITGSA.API.Services;

public class ConfiguracionService
{
    private readonly DataStore _store;

    public ConfiguracionService(DataStore store)
    {
        _store = store;
    }

    public RespuestaConfig ProcesarConfiguracion(List<Cliente> clientesNuevos, List<Banco> bancosNuevos)
    {
        var clientesExistentes = _store.ObtenerClientes();
        var bancosExistentes = _store.ObtenerBancos();

        int clientesCreados = 0;
        int clientesActualizados = 0;
        int bancosCreados = 0;
        int bancosActualizados = 0;

        foreach (var nuevo in clientesNuevos)
        {
            var existente = clientesExistentes.FirstOrDefault(c =>
                c.NIT.Equals(nuevo.NIT, StringComparison.OrdinalIgnoreCase));

            if (existente == null)
            {
                clientesExistentes.Add(nuevo);
                clientesCreados++;
            }
            else
            {
                existente.Nombre = nuevo.Nombre;
                clientesActualizados++;
            }
        }

        foreach (var nuevo in bancosNuevos)
        {
            var existente = bancosExistentes.FirstOrDefault(b => b.Codigo == nuevo.Codigo);

            if (existente == null)
            {
                bancosExistentes.Add(nuevo);
                bancosCreados++;
            }
            else
            {
                existente.Nombre = nuevo.Nombre;
                bancosActualizados++;
            }
        }

        _store.GuardarClientes(clientesExistentes);
        _store.GuardarBancos(bancosExistentes);

        return new RespuestaConfig
        {
            ClientesCreados = clientesCreados,
            ClientesActualizados = clientesActualizados,
            BancosCreados = bancosCreados,
            BancosActualizados = bancosActualizados
        };
    }
}
