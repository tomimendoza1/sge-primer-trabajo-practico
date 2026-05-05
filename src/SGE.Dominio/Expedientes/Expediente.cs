using SGE.Dominio.Comun;
using SGE.Dominio.Tramites;

namespace SGE.Dominio.Expedientes;

public sealed class Expediente
{
    private Expediente(
        Guid id,
        Caratula caratula,
        DateTime fechaCreacion,
        DateTime fechaUltimaModificacion,
        Guid usuarioUltimoCambio,
        EstadoExpediente estado)
    {
        if (fechaUltimaModificacion < fechaCreacion)
        {
            throw new DominioException("La fecha de ultima modificacion no puede ser menor que la fecha de creacion.");
        }

        Id = id == Guid.Empty ? throw new DominioException("El id del expediente es obligatorio.") : id;
        Caratula = caratula;
        FechaCreacion = fechaCreacion;
        FechaUltimaModificacion = fechaUltimaModificacion;
        UsuarioUltimoCambio = usuarioUltimoCambio == Guid.Empty ? throw new DominioException("El usuario es obligatorio.") : usuarioUltimoCambio;
        Estado = estado;
    }

    public Guid Id { get; }
    public Caratula Caratula { get; private set; }
    public DateTime FechaCreacion { get; }
    public DateTime FechaUltimaModificacion { get; private set; }
    public Guid UsuarioUltimoCambio { get; private set; }
    public EstadoExpediente Estado { get; private set; }

    public static Expediente Crear(Caratula caratula, Guid idUsuario, DateTime fecha)
    {
        return new Expediente(Guid.NewGuid(), caratula, fecha, fecha, idUsuario, EstadoExpediente.RecienIniciado);
    }

    public static Expediente Reconstruir(
        Guid id,
        Caratula caratula,
        DateTime fechaCreacion,
        DateTime fechaUltimaModificacion,
        Guid usuarioUltimoCambio,
        EstadoExpediente estado)
    {
        return new Expediente(id, caratula, fechaCreacion, fechaUltimaModificacion, usuarioUltimoCambio, estado);
    }

    public void ModificarCaratula(Caratula nuevaCaratula, Guid idUsuario, DateTime fechaModificacion)
    {
        ValidarCambio(idUsuario, fechaModificacion);
        Caratula = nuevaCaratula;
        UsuarioUltimoCambio = idUsuario;
        FechaUltimaModificacion = fechaModificacion;
    }

    public void CambiarEstado(EstadoExpediente nuevoEstado, Guid idUsuario, DateTime fechaModificacion)
    {
        ValidarCambio(idUsuario, fechaModificacion);
        Estado = nuevoEstado;
        UsuarioUltimoCambio = idUsuario;
        FechaUltimaModificacion = fechaModificacion;
    }

    public bool ActualizarEstado(EtiquetaTramite ultimaEtiqueta, Guid idUsuario, DateTime fechaModificacion)
    {
        var nuevoEstado = ultimaEtiqueta switch
        {
            EtiquetaTramite.Resolucion => EstadoExpediente.ConResolucion,
            EtiquetaTramite.PaseAEstudio => EstadoExpediente.ParaResolver,
            EtiquetaTramite.PaseAlArchivo => EstadoExpediente.Finalizado,
            _ => TramiteSinCambio()
        };

        if (nuevoEstado == Estado)
        {
            return false;
        }

        CambiarEstado(nuevoEstado, idUsuario, fechaModificacion);
        return true;
    }

    public bool VolverARecienIniciadoSiNoTieneTramites(bool tieneTramites, Guid idUsuario, DateTime fechaModificacion)
    {
        if (tieneTramites || Estado == EstadoExpediente.RecienIniciado)
        {
            return false;
        }

        CambiarEstado(EstadoExpediente.RecienIniciado, idUsuario, fechaModificacion);
        return true;
    }

    private EstadoExpediente TramiteSinCambio() => Estado;

    private void ValidarCambio(Guid idUsuario, DateTime fechaModificacion)
    {
        if (idUsuario == Guid.Empty)
        {
            throw new DominioException("El usuario es obligatorio.");
        }

        if (fechaModificacion < FechaCreacion)
        {
            throw new DominioException("La fecha de modificacion no puede ser menor que la fecha de creacion.");
        }
    }
}
