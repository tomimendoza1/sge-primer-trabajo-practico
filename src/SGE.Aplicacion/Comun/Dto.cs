using SGE.Dominio.Expedientes;
using SGE.Dominio.Tramites;

namespace SGE.Aplicacion.Comun;

public sealed record ExpedienteDto(
    Guid Id,
    string Caratula,
    EstadoExpediente Estado,
    DateTime FechaCreacion,
    DateTime FechaUltimaModificacion,
    Guid UsuarioUltimoCambio);

public sealed record TramiteDto(
    Guid Id,
    Guid ExpedienteId,
    EtiquetaTramite Etiqueta,
    string Contenido,
    DateTime FechaCreacion,
    DateTime FechaUltimaModificacion,
    Guid UsuarioUltimoCambio);

public static class DtoMapper
{
    public static ExpedienteDto ToDto(this Expediente expediente)
    {
        return new ExpedienteDto(
            expediente.Id,
            expediente.Caratula.Valor,
            expediente.Estado,
            expediente.FechaCreacion,
            expediente.FechaUltimaModificacion,
            expediente.UsuarioUltimoCambio);
    }

    public static TramiteDto ToDto(this Tramite tramite)
    {
        return new TramiteDto(
            tramite.Id,
            tramite.ExpedienteId,
            tramite.Etiqueta,
            tramite.Contenido.Valor,
            tramite.FechaCreacion,
            tramite.FechaUltimaModificacion,
            tramite.UsuarioUltimoCambio);
    }
}
