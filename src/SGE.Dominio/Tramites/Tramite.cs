using SGE.Dominio.Comun;

namespace SGE.Dominio.Tramites;

public sealed class Tramite
{
    private Tramite(
        Guid id,
        Guid expedienteId,
        EtiquetaTramite etiqueta,
        ContenidoTramite contenido,
        DateTime fechaCreacion,
        DateTime fechaUltimaModificacion,
        Guid usuarioUltimoCambio)
    {
        if (fechaUltimaModificacion < fechaCreacion)
        {
            throw new DominioException("La fecha de ultima modificacion no puede ser menor que la fecha de creacion.");
        }

        Id = id == Guid.Empty ? throw new DominioException("El id del tramite es obligatorio.") : id;
        ExpedienteId = expedienteId == Guid.Empty ? throw new DominioException("El expediente es obligatorio.") : expedienteId;
        Etiqueta = etiqueta;
        Contenido = contenido;
        FechaCreacion = fechaCreacion;
        FechaUltimaModificacion = fechaUltimaModificacion;
        UsuarioUltimoCambio = usuarioUltimoCambio == Guid.Empty ? throw new DominioException("El usuario es obligatorio.") : usuarioUltimoCambio;
    }

    public Guid Id { get; }
    public Guid ExpedienteId { get; }
    public EtiquetaTramite Etiqueta { get; private set; }
    public ContenidoTramite Contenido { get; private set; }
    public DateTime FechaCreacion { get; }
    public DateTime FechaUltimaModificacion { get; private set; }
    public Guid UsuarioUltimoCambio { get; private set; }

    public static Tramite Crear(Guid expedienteId, EtiquetaTramite etiqueta, ContenidoTramite contenido, Guid idUsuario, DateTime fecha)
    {
        return new Tramite(Guid.NewGuid(), expedienteId, etiqueta, contenido, fecha, fecha, idUsuario);
    }

    public static Tramite Reconstruir(
        Guid id,
        Guid expedienteId,
        EtiquetaTramite etiqueta,
        ContenidoTramite contenido,
        DateTime fechaCreacion,
        DateTime fechaUltimaModificacion,
        Guid usuarioUltimoCambio)
    {
        return new Tramite(id, expedienteId, etiqueta, contenido, fechaCreacion, fechaUltimaModificacion, usuarioUltimoCambio);
    }

    public void Modificar(EtiquetaTramite etiqueta, ContenidoTramite contenido, Guid idUsuario, DateTime fechaModificacion)
    {
        if (idUsuario == Guid.Empty)
        {
            throw new DominioException("El usuario es obligatorio.");
        }

        if (fechaModificacion < FechaCreacion)
        {
            throw new DominioException("La fecha de modificacion no puede ser menor que la fecha de creacion.");
        }

        Etiqueta = etiqueta;
        Contenido = contenido;
        UsuarioUltimoCambio = idUsuario;
        FechaUltimaModificacion = fechaModificacion;
    }
}
