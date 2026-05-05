using SGE.Aplicacion.Autorizacion;
using SGE.Aplicacion.Comun;
using SGE.Aplicacion.Expedientes;
using SGE.Aplicacion.Tramites;
using SGE.Dominio.Comun;
using SGE.Dominio.Expedientes;
using SGE.Dominio.Tramites;
using SGE.Infraestructura.Autorizacion;
using SGE.Infraestructura.Expedientes;
using SGE.Infraestructura.Tramites;

var datos = Path.Combine(AppContext.BaseDirectory, "datos");
Directory.CreateDirectory(datos);
var expedientesTxt = Path.Combine(datos, "expedientes.txt");
var tramitesTxt = Path.Combine(datos, "tramites.txt");
File.WriteAllText(expedientesTxt, string.Empty);
File.WriteAllText(tramitesTxt, string.Empty);

var expedienteRepository = new ExpedienteTxtRepository(expedientesTxt);
var tramiteRepository = new TramiteTxtRepository(tramitesTxt);
var autorizacion = new AutorizacionProvisionalService();
var actualizacionEstado = new ActualizacionEstadoExpedienteService(expedienteRepository, tramiteRepository);

var agregarExpediente = new AgregarExpedienteUseCase(expedienteRepository, autorizacion);
var modificarCaratula = new ModificarCaratulaExpedienteUseCase(expedienteRepository, autorizacion);
var cambiarEstado = new CambiarEstadoExpedienteUseCase(expedienteRepository, autorizacion);
var bajaExpediente = new BajaExpedienteUseCase(expedienteRepository, tramiteRepository, autorizacion);
var listarExpedientes = new ListarExpedientesUseCase(expedienteRepository);

var agregarTramite = new AgregarTramiteUseCase(expedienteRepository, tramiteRepository, actualizacionEstado, autorizacion);
var modificarTramite = new ModificarTramiteUseCase(tramiteRepository, actualizacionEstado, autorizacion);
var bajaTramite = new BajaTramiteUseCase(tramiteRepository, actualizacionEstado, autorizacion);
var listarTramites = new ListarTramitesPorExpedienteUseCase(tramiteRepository);

var usuario = Guid.NewGuid();

Console.WriteLine("SGE - Simulacion de casos de uso");
Console.WriteLine();

Ejecutar("Camino feliz", () =>
{
    var expediente = agregarExpediente.Ejecutar(new AgregarExpedienteRequest("Solicitud de habilitacion comercial", usuario)).Expediente;
    Console.WriteLine($"Expediente creado: {expediente.Id} - {expediente.Estado}");

    agregarTramite.Ejecutar(new AgregarTramiteRequest(expediente.Id, EtiquetaTramite.EscritoPresentado, "Se presenta documentacion inicial.", usuario));
    var resolucion = agregarTramite.Ejecutar(new AgregarTramiteRequest(expediente.Id, EtiquetaTramite.Resolucion, "Se resuelve favorablemente.", usuario)).Tramite;

    var actualizado = expedienteRepository.ObtenerPorId(expediente.Id)!;
    Console.WriteLine($"Estado automatico luego de resolucion: {actualizado.Estado}");

    cambiarEstado.Ejecutar(new CambiarEstadoExpedienteRequest(expediente.Id, EstadoExpediente.EnNotificacion, usuario));
    modificarCaratula.Ejecutar(new ModificarCaratulaExpedienteRequest(expediente.Id, "Solicitud de habilitacion comercial corregida", usuario));
    modificarTramite.Ejecutar(new ModificarTramiteRequest(resolucion.Id, EtiquetaTramite.Notificacion, "Se notifica la resolucion.", usuario));

    foreach (var item in listarExpedientes.Ejecutar().Expedientes)
    {
        Console.WriteLine($"Listado expediente: {item.Caratula} - {item.Estado}");
    }

    foreach (var item in listarTramites.Ejecutar(new ListarTramitesPorExpedienteRequest(expediente.Id)).Tramites)
    {
        Console.WriteLine($"Tramite: {item.Etiqueta} - {item.Contenido}");
    }

    bajaTramite.Ejecutar(new BajaTramiteRequest(resolucion.Id, usuario));
    bajaExpediente.Ejecutar(new BajaExpedienteRequest(expediente.Id, usuario));
});

Ejecutar("Error de dominio", () =>
{
    agregarExpediente.Ejecutar(new AgregarExpedienteRequest("   ", usuario));
});

Ejecutar("Entidad inexistente", () =>
{
    cambiarEstado.Ejecutar(new CambiarEstadoExpedienteRequest(Guid.NewGuid(), EstadoExpediente.Finalizado, usuario));
});

Ejecutar("Autorizacion denegada", () =>
{
    var autorizacionDenegada = new AutorizacionDenegadaService();
    var caso = new AgregarExpedienteUseCase(expedienteRepository, autorizacionDenegada);
    caso.Ejecutar(new AgregarExpedienteRequest("No deberia crearse", usuario));
});

static void Ejecutar(string titulo, Action accion)
{
    Console.WriteLine($"== {titulo} ==");
    try
    {
        accion();
    }
    catch (DominioException ex)
    {
        Console.WriteLine($"DominioException: {ex.Message}");
    }
    catch (AplicacionException ex)
    {
        Console.WriteLine($"AplicacionException: {ex.Message}");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error general: {ex.Message}");
    }

    Console.WriteLine();
}

internal sealed class AutorizacionDenegadaService : IAutorizacionService
{
    public bool PoseeElPermiso(Guid idUsuario, Permiso permiso)
    {
        return false;
    }
}
