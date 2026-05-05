using SGE.Dominio.Tramites;

namespace SGE.Aplicacion.Tramites;

public interface ITramiteRepository
{
    void Agregar(Tramite tramite);
    void Modificar(Tramite tramite);
    void Eliminar(Guid id);
    void EliminarPorExpediente(Guid expedienteId);
    Tramite? ObtenerPorId(Guid id);
    IEnumerable<Tramite> ObtenerPorExpediente(Guid expedienteId);
}
