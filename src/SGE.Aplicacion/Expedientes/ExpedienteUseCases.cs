using SGE.Aplicacion.Autorizacion;
using SGE.Aplicacion.Comun;
using SGE.Aplicacion.Tramites;
using SGE.Dominio.Expedientes;

namespace SGE.Aplicacion.Expedientes;

public sealed record AgregarExpedienteRequest(string Caratula, Guid IdUsuario);
public sealed record AgregarExpedienteResponse(ExpedienteDto Expediente);

public sealed class AgregarExpedienteUseCase : CasoDeUsoBase
{
    private readonly IExpedienteRepository _expedientes;

    public AgregarExpedienteUseCase(IExpedienteRepository expedientes, IAutorizacionService autorizacion, Func<DateTime>? reloj = null)
        : base(autorizacion, reloj)
    {
        _expedientes = expedientes;
    }

    public AgregarExpedienteResponse Ejecutar(AgregarExpedienteRequest request)
    {
        ExigirPermiso(request.IdUsuario, Permiso.ExpedienteAlta);
        var expediente = Expediente.Crear(new Caratula(request.Caratula), request.IdUsuario, Reloj());
        _expedientes.Agregar(expediente);
        return new AgregarExpedienteResponse(expediente.ToDto());
    }
}

public sealed record ModificarCaratulaExpedienteRequest(Guid ExpedienteId, string Caratula, Guid IdUsuario);
public sealed record ModificarCaratulaExpedienteResponse(ExpedienteDto Expediente);

public sealed class ModificarCaratulaExpedienteUseCase : CasoDeUsoBase
{
    private readonly IExpedienteRepository _expedientes;

    public ModificarCaratulaExpedienteUseCase(IExpedienteRepository expedientes, IAutorizacionService autorizacion, Func<DateTime>? reloj = null)
        : base(autorizacion, reloj)
    {
        _expedientes = expedientes;
    }

    public ModificarCaratulaExpedienteResponse Ejecutar(ModificarCaratulaExpedienteRequest request)
    {
        ExigirPermiso(request.IdUsuario, Permiso.ExpedienteModificacion);
        var expediente = _expedientes.ObtenerPorId(request.ExpedienteId)
            ?? throw new EntidadNoEncontradaException("No se encontro el expediente solicitado.");

        expediente.ModificarCaratula(new Caratula(request.Caratula), request.IdUsuario, Reloj());
        _expedientes.Modificar(expediente);
        return new ModificarCaratulaExpedienteResponse(expediente.ToDto());
    }
}

public sealed record CambiarEstadoExpedienteRequest(Guid ExpedienteId, EstadoExpediente Estado, Guid IdUsuario);
public sealed record CambiarEstadoExpedienteResponse(ExpedienteDto Expediente);

public sealed class CambiarEstadoExpedienteUseCase : CasoDeUsoBase
{
    private readonly IExpedienteRepository _expedientes;

    public CambiarEstadoExpedienteUseCase(IExpedienteRepository expedientes, IAutorizacionService autorizacion, Func<DateTime>? reloj = null)
        : base(autorizacion, reloj)
    {
        _expedientes = expedientes;
    }

    public CambiarEstadoExpedienteResponse Ejecutar(CambiarEstadoExpedienteRequest request)
    {
        ExigirPermiso(request.IdUsuario, Permiso.ExpedienteModificacion);
        var expediente = _expedientes.ObtenerPorId(request.ExpedienteId)
            ?? throw new EntidadNoEncontradaException("No se encontro el expediente solicitado.");

        expediente.CambiarEstado(request.Estado, request.IdUsuario, Reloj());
        _expedientes.Modificar(expediente);
        return new CambiarEstadoExpedienteResponse(expediente.ToDto());
    }
}

public sealed record BajaExpedienteRequest(Guid ExpedienteId, Guid IdUsuario);

public sealed class BajaExpedienteUseCase : CasoDeUsoBase
{
    private readonly IExpedienteRepository _expedientes;
    private readonly ITramiteRepository _tramites;

    public BajaExpedienteUseCase(IExpedienteRepository expedientes, ITramiteRepository tramites, IAutorizacionService autorizacion, Func<DateTime>? reloj = null)
        : base(autorizacion, reloj)
    {
        _expedientes = expedientes;
        _tramites = tramites;
    }

    public void Ejecutar(BajaExpedienteRequest request)
    {
        ExigirPermiso(request.IdUsuario, Permiso.ExpedienteBaja);

        if (_expedientes.ObtenerPorId(request.ExpedienteId) is null)
        {
            throw new EntidadNoEncontradaException("No se encontro el expediente solicitado.");
        }

        _tramites.EliminarPorExpediente(request.ExpedienteId);
        _expedientes.Eliminar(request.ExpedienteId);
    }
}

public sealed record ListarExpedientesResponse(IReadOnlyCollection<ExpedienteDto> Expedientes);

public sealed class ListarExpedientesUseCase
{
    private readonly IExpedienteRepository _expedientes;

    public ListarExpedientesUseCase(IExpedienteRepository expedientes)
    {
        _expedientes = expedientes;
    }

    public ListarExpedientesResponse Ejecutar()
    {
        return new ListarExpedientesResponse(_expedientes.ObtenerTodos().Select(x => x.ToDto()).ToList());
    }
}
