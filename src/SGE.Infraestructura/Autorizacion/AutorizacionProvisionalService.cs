using SGE.Aplicacion.Autorizacion;

namespace SGE.Infraestructura.Autorizacion;

public sealed class AutorizacionProvisionalService : IAutorizacionService
{
    public bool PoseeElPermiso(Guid idUsuario, Permiso permiso)
    {
        return true;
    }
}
