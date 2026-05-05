using SGE.Aplicacion.Tramites;
using SGE.Dominio.Tramites;
using SGE.Infraestructura.Comun;

namespace SGE.Infraestructura.Tramites;

public sealed class TramiteTxtRepository : ITramiteRepository
{
    private readonly string _ruta;

    public TramiteTxtRepository(string ruta)
    {
        _ruta = ruta;
        TextoPlano.AsegurarArchivo(_ruta);
    }

    public void Agregar(Tramite tramite)
    {
        var tramites = ObtenerLista();
        if (tramites.Any(x => x.Id == tramite.Id))
        {
            throw new RepositorioException("Ya existe un tramite con el mismo id.");
        }

        tramites.Add(tramite);
        Guardar(tramites);
    }

    public void Modificar(Tramite tramite)
    {
        var tramites = ObtenerLista();
        var indice = tramites.FindIndex(x => x.Id == tramite.Id);
        if (indice < 0)
        {
            throw new RepositorioException("No se encontro el tramite a modificar.");
        }

        tramites[indice] = tramite;
        Guardar(tramites);
    }

    public void Eliminar(Guid id)
    {
        var tramites = ObtenerLista();
        var removidos = tramites.RemoveAll(x => x.Id == id);
        if (removidos == 0)
        {
            throw new RepositorioException("No se encontro el tramite a eliminar.");
        }

        Guardar(tramites);
    }

    public void EliminarPorExpediente(Guid expedienteId)
    {
        var tramites = ObtenerLista();
        tramites.RemoveAll(x => x.ExpedienteId == expedienteId);
        Guardar(tramites);
    }

    public Tramite? ObtenerPorId(Guid id)
    {
        return ObtenerPorExpedienteTodos().FirstOrDefault(x => x.Id == id);
    }

    public IEnumerable<Tramite> ObtenerPorExpediente(Guid expedienteId)
    {
        return ObtenerPorExpedienteTodos().Where(x => x.ExpedienteId == expedienteId);
    }

    private IEnumerable<Tramite> ObtenerPorExpedienteTodos()
    {
        return ObtenerLista();
    }

    private List<Tramite> ObtenerLista()
    {
        TextoPlano.AsegurarArchivo(_ruta);
        return File.ReadAllLines(_ruta)
            .Where(linea => !string.IsNullOrWhiteSpace(linea))
            .Select(Deserializar)
            .ToList();
    }

    private void Guardar(IEnumerable<Tramite> tramites)
    {
        File.WriteAllLines(_ruta, tramites.Select(Serializar));
    }

    private static string Serializar(Tramite tramite)
    {
        return string.Join('|',
            tramite.Id,
            tramite.ExpedienteId,
            (int)tramite.Etiqueta,
            TextoPlano.Codificar(tramite.Contenido.Valor),
            TextoPlano.Fecha(tramite.FechaCreacion),
            TextoPlano.Fecha(tramite.FechaUltimaModificacion),
            tramite.UsuarioUltimoCambio);
    }

    private static Tramite Deserializar(string linea)
    {
        var partes = linea.Split('|');
        if (partes.Length != 7)
        {
            throw new RepositorioException("El archivo de tramites contiene una linea invalida.");
        }

        return Tramite.Reconstruir(
            Guid.Parse(partes[0]),
            Guid.Parse(partes[1]),
            (EtiquetaTramite)int.Parse(partes[2]),
            new ContenidoTramite(TextoPlano.Decodificar(partes[3])),
            TextoPlano.LeerFecha(partes[4]),
            TextoPlano.LeerFecha(partes[5]),
            Guid.Parse(partes[6]));
    }
}
