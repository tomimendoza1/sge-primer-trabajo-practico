using SGE.Aplicacion.Autorizacion;
using SGE.Aplicacion.Comun;
using SGE.Aplicacion.Expedientes;
using SGE.Dominio.Tramites;

namespace SGE.Aplicacion.Tramites;

public sealed class ActualizacionEstadoExpedienteService
{
    private readonly IExpedienteRepository _expedientes;
    private readonly ITramiteRepository _tramites;
    private readonly Func<DateTime> _reloj;

    public ActualizacionEstadoExpedienteService(IExpedienteRepository expedientes, ITramiteRepository tramites, Func<DateTime>? reloj = null)
    {
        _expedientes = expedientes;
        _tramites = tramites;
        _reloj = reloj ?? (() => DateTime.UtcNow);
    }

    public void ActualizarPorUltimoTramite(Guid expedienteId, Guid idUsuario)
    {
        var expediente = _expedientes.ObtenerPorId(expedienteId)
            ?? throw new EntidadNoEncontradaException("No se encontro el expediente asociado al tramite.");

        var ultimoTramite = _tramites.ObtenerPorExpediente(expedienteId)
            .OrderByDescending(x => x.FechaCreacion)
            .FirstOrDefault();

        bool cambio = ultimoTramite is null
            ? expediente.VolverARecienIniciadoSiNoTieneTramites(false, idUsuario, _reloj())
            : expediente.ActualizarEstado(ultimoTramite.Etiqueta, idUsuario, _reloj());

        if (cambio)
        {
            _expedientes.Modificar(expediente);
        }
    }
}

public sealed record AgregarTramiteRequest(Guid ExpedienteId, EtiquetaTramite Etiqueta, string Contenido, Guid IdUsuario);
public sealed record AgregarTramiteResponse(TramiteDto Tramite);

public sealed class AgregarTramiteUseCase : CasoDeUsoBase
{
    private readonly IExpedienteRepository _expedientes;
    private readonly ITramiteRepository _tramites;
    private readonly ActualizacionEstadoExpedienteService _actualizacionEstado;

    public AgregarTramiteUseCase(
        IExpedienteRepository expedientes,
        ITramiteRepository tramites,
        ActualizacionEstadoExpedienteService actualizacionEstado,
        IAutorizacionService autorizacion,
        Func<DateTime>? reloj = null)
        : base(autorizacion, reloj)
    {
        _expedientes = expedientes;
        _tramites = tramites;
        _actualizacionEstado = actualizacionEstado;
    }

    public AgregarTramiteResponse Ejecutar(AgregarTramiteRequest request)
    {
        ExigirPermiso(request.IdUsuario, Permiso.TramiteAlta);

        if (_expedientes.ObtenerPorId(request.ExpedienteId) is null)
        {
            throw new EntidadNoEncontradaException("No se encontro el expediente asociado al tramite.");
        }

        var tramite = Tramite.Crear(request.ExpedienteId, request.Etiqueta, new ContenidoTramite(request.Contenido), request.IdUsuario, Reloj());
        _tramites.Agregar(tramite);
        _actualizacionEstado.ActualizarPorUltimoTramite(request.ExpedienteId, request.IdUsuario);
        return new AgregarTramiteResponse(tramite.ToDto());
    }
}

public sealed record ModificarTramiteRequest(Guid TramiteId, EtiquetaTramite Etiqueta, string Contenido, Guid IdUsuario);
public sealed record ModificarTramiteResponse(TramiteDto Tramite);

public sealed class ModificarTramiteUseCase : CasoDeUsoBase
{
    private readonly ITramiteRepository _tramites;
    private readonly ActualizacionEstadoExpedienteService _actualizacionEstado;

    public ModificarTramiteUseCase(
        ITramiteRepository tramites,
        ActualizacionEstadoExpedienteService actualizacionEstado,
        IAutorizacionService autorizacion,
        Func<DateTime>? reloj = null)
        : base(autorizacion, reloj)
    {
        _tramites = tramites;
        _actualizacionEstado = actualizacionEstado;
    }

    public ModificarTramiteResponse Ejecutar(ModificarTramiteRequest request)
    {
        ExigirPermiso(request.IdUsuario, Permiso.TramiteModificacion);
        var tramite = _tramites.ObtenerPorId(request.TramiteId)
            ?? throw new EntidadNoEncontradaException("No se encontro el tramite solicitado.");

        tramite.Modificar(request.Etiqueta, new ContenidoTramite(request.Contenido), request.IdUsuario, Reloj());
        _tramites.Modificar(tramite);
        _actualizacionEstado.ActualizarPorUltimoTramite(tramite.ExpedienteId, request.IdUsuario);
        return new ModificarTramiteResponse(tramite.ToDto());
    }
}

public sealed record BajaTramiteRequest(Guid TramiteId, Guid IdUsuario);

public sealed class BajaTramiteUseCase : CasoDeUsoBase
{
    private readonly ITramiteRepository _tramites;
    private readonly ActualizacionEstadoExpedienteService _actualizacionEstado;

    public BajaTramiteUseCase(
        ITramiteRepository tramites,
        ActualizacionEstadoExpedienteService actualizacionEstado,
        IAutorizacionService autorizacion,
        Func<DateTime>? reloj = null)
        : base(autorizacion, reloj)
    {
        _tramites = tramites;
        _actualizacionEstado = actualizacionEstado;
    }

    public void Ejecutar(BajaTramiteRequest request)
    {
        ExigirPermiso(request.IdUsuario, Permiso.TramiteBaja);
        var tramite = _tramites.ObtenerPorId(request.TramiteId)
            ?? throw new EntidadNoEncontradaException("No se encontro el tramite solicitado.");

        _tramites.Eliminar(request.TramiteId);
        _actualizacionEstado.ActualizarPorUltimoTramite(tramite.ExpedienteId, request.IdUsuario);
    }
}

public sealed record ListarTramitesPorExpedienteRequest(Guid ExpedienteId);
public sealed record ListarTramitesPorExpedienteResponse(IReadOnlyCollection<TramiteDto> Tramites);

public sealed class ListarTramitesPorExpedienteUseCase
{
    private readonly ITramiteRepository _tramites;

    public ListarTramitesPorExpedienteUseCase(ITramiteRepository tramites)
    {
        _tramites = tramites;
    }

    public ListarTramitesPorExpedienteResponse Ejecutar(ListarTramitesPorExpedienteRequest request)
    {
        var tramites = _tramites.ObtenerPorExpediente(request.ExpedienteId)
            .OrderBy(x => x.FechaCreacion)
            .Select(x => x.ToDto())
            .ToList();

        return new ListarTramitesPorExpedienteResponse(tramites);
    }
}
