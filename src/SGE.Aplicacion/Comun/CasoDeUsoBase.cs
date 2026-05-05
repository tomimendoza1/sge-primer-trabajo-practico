using SGE.Aplicacion.Autorizacion;

namespace SGE.Aplicacion.Comun;

public abstract class CasoDeUsoBase
{
    private readonly IAutorizacionService _autorizacionService;

    protected CasoDeUsoBase(IAutorizacionService autorizacionService, Func<DateTime>? reloj = null)
    {
        _autorizacionService = autorizacionService;
        Reloj = reloj ?? (() => DateTime.UtcNow);
    }

    protected Func<DateTime> Reloj { get; }

    protected void ExigirPermiso(Guid idUsuario, Permiso permiso)
    {
        if (!_autorizacionService.PoseeElPermiso(idUsuario, permiso))
        {
            throw new AutorizacionException($"El usuario no posee el permiso requerido: {permiso}.");
        }
    }
}
