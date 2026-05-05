using SGE.Aplicacion.Expedientes;
using SGE.Dominio.Expedientes;
using SGE.Infraestructura.Comun;

namespace SGE.Infraestructura.Expedientes;

public sealed class ExpedienteTxtRepository : IExpedienteRepository
{
    private readonly string _ruta;

    public ExpedienteTxtRepository(string ruta)
    {
        _ruta = ruta;
        TextoPlano.AsegurarArchivo(_ruta);
    }

    public void Agregar(Expediente expediente)
    {
        var expedientes = ObtenerLista();
        if (expedientes.Any(x => x.Id == expediente.Id))
        {
            throw new RepositorioException("Ya existe un expediente con el mismo id.");
        }

        expedientes.Add(expediente);
        Guardar(expedientes);
    }

    public void Modificar(Expediente expediente)
    {
        var expedientes = ObtenerLista();
        var indice = expedientes.FindIndex(x => x.Id == expediente.Id);
        if (indice < 0)
        {
            throw new RepositorioException("No se encontro el expediente a modificar.");
        }

        expedientes[indice] = expediente;
        Guardar(expedientes);
    }

    public void Eliminar(Guid id)
    {
        var expedientes = ObtenerLista();
        var removidos = expedientes.RemoveAll(x => x.Id == id);
        if (removidos == 0)
        {
            throw new RepositorioException("No se encontro el expediente a eliminar.");
        }

        Guardar(expedientes);
    }

    public Expediente? ObtenerPorId(Guid id)
    {
        return ObtenerTodos().FirstOrDefault(x => x.Id == id);
    }

    public IEnumerable<Expediente> ObtenerTodos()
    {
        return ObtenerLista();
    }

    private List<Expediente> ObtenerLista()
    {
        TextoPlano.AsegurarArchivo(_ruta);
        return File.ReadAllLines(_ruta)
            .Where(linea => !string.IsNullOrWhiteSpace(linea))
            .Select(Deserializar)
            .ToList();
    }

    private void Guardar(IEnumerable<Expediente> expedientes)
    {
        File.WriteAllLines(_ruta, expedientes.Select(Serializar));
    }

    private static string Serializar(Expediente expediente)
    {
        return string.Join('|',
            expediente.Id,
            TextoPlano.Codificar(expediente.Caratula.Valor),
            TextoPlano.Fecha(expediente.FechaCreacion),
            TextoPlano.Fecha(expediente.FechaUltimaModificacion),
            expediente.UsuarioUltimoCambio,
            (int)expediente.Estado);
    }

    private static Expediente Deserializar(string linea)
    {
        var partes = linea.Split('|');
        if (partes.Length != 6)
        {
            throw new RepositorioException("El archivo de expedientes contiene una linea invalida.");
        }

        return Expediente.Reconstruir(
            Guid.Parse(partes[0]),
            new Caratula(TextoPlano.Decodificar(partes[1])),
            TextoPlano.LeerFecha(partes[2]),
            TextoPlano.LeerFecha(partes[3]),
            Guid.Parse(partes[4]),
            (EstadoExpediente)int.Parse(partes[5]));
    }
}
