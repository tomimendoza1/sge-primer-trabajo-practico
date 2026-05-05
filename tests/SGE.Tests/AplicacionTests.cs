using SGE.Aplicacion.Autorizacion;
using SGE.Aplicacion.Comun;
using SGE.Aplicacion.Expedientes;
using SGE.Aplicacion.Tramites;
using SGE.Dominio.Expedientes;
using SGE.Dominio.Tramites;
using SGE.Infraestructura.Autorizacion;
using SGE.Infraestructura.Expedientes;
using SGE.Infraestructura.Tramites;

namespace SGE.Tests;

public sealed class AplicacionTests
{
    private static readonly Guid Usuario = Guid.Parse("33333333-3333-3333-3333-333333333333");
    private static readonly DateTime Fecha = new(2026, 5, 5, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Caso_De_Uso_Crea_Expediente()
    {
        var contexto = CrearContexto();

        var response = contexto.AgregarExpediente.Ejecutar(new AgregarExpedienteRequest("Alta municipal", Usuario));

        Assert.Equal("Alta municipal", response.Expediente.Caratula);
        Assert.Single(contexto.ListarExpedientes.Ejecutar().Expedientes);
    }

    [Fact]
    public void Tramite_Resolucion_Cambia_Estado_Automaticamente()
    {
        var contexto = CrearContexto();
        var expediente = contexto.AgregarExpediente.Ejecutar(new AgregarExpedienteRequest("Alta municipal", Usuario)).Expediente;

        contexto.AgregarTramite.Ejecutar(new AgregarTramiteRequest(expediente.Id, EtiquetaTramite.Resolucion, "Resolucion aprobada", Usuario));

        var actualizado = contexto.ListarExpedientes.Ejecutar().Expedientes.Single();
        Assert.Equal(EstadoExpediente.ConResolucion, actualizado.Estado);
    }

    [Fact]
    public void Baja_De_Expediente_Elimina_Tramites_Asociados()
    {
        var contexto = CrearContexto();
        var expediente = contexto.AgregarExpediente.Ejecutar(new AgregarExpedienteRequest("Alta municipal", Usuario)).Expediente;
        contexto.AgregarTramite.Ejecutar(new AgregarTramiteRequest(expediente.Id, EtiquetaTramite.Despacho, "Despacho simple", Usuario));

        contexto.BajaExpediente.Ejecutar(new BajaExpedienteRequest(expediente.Id, Usuario));

        Assert.Empty(contexto.ListarExpedientes.Ejecutar().Expedientes);
        Assert.Empty(contexto.ListarTramites.Ejecutar(new ListarTramitesPorExpedienteRequest(expediente.Id)).Tramites);
    }

    [Fact]
    public void Mutacion_Sin_Permiso_Lanza_AutorizacionException()
    {
        var contexto = CrearContexto(new AutorizacionDenegada());

        Assert.Throws<AutorizacionException>(() =>
            contexto.AgregarExpediente.Ejecutar(new AgregarExpedienteRequest("No autorizado", Usuario)));
    }

    [Fact]
    public void Entidad_Inexistente_Lanza_EntidadNoEncontradaException()
    {
        var contexto = CrearContexto();

        Assert.Throws<EntidadNoEncontradaException>(() =>
            contexto.CambiarEstado.Ejecutar(new CambiarEstadoExpedienteRequest(Guid.NewGuid(), EstadoExpediente.Finalizado, Usuario)));
    }

    private static ContextoCasosDeUso CrearContexto(IAutorizacionService? autorizacion = null)
    {
        var carpeta = Path.Combine(Path.GetTempPath(), "sge-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(carpeta);

        var expedientes = new ExpedienteTxtRepository(Path.Combine(carpeta, "expedientes.txt"));
        var tramites = new TramiteTxtRepository(Path.Combine(carpeta, "tramites.txt"));
        autorizacion ??= new AutorizacionProvisionalService();
        var reloj = new RelojDeterministico(Fecha);
        var actualizacion = new ActualizacionEstadoExpedienteService(expedientes, tramites, reloj.Ahora);

        return new ContextoCasosDeUso(
            new AgregarExpedienteUseCase(expedientes, autorizacion, reloj.Ahora),
            new CambiarEstadoExpedienteUseCase(expedientes, autorizacion, reloj.Ahora),
            new BajaExpedienteUseCase(expedientes, tramites, autorizacion, reloj.Ahora),
            new ListarExpedientesUseCase(expedientes),
            new AgregarTramiteUseCase(expedientes, tramites, actualizacion, autorizacion, reloj.Ahora),
            new ListarTramitesPorExpedienteUseCase(tramites));
    }

    private sealed record ContextoCasosDeUso(
        AgregarExpedienteUseCase AgregarExpediente,
        CambiarEstadoExpedienteUseCase CambiarEstado,
        BajaExpedienteUseCase BajaExpediente,
        ListarExpedientesUseCase ListarExpedientes,
        AgregarTramiteUseCase AgregarTramite,
        ListarTramitesPorExpedienteUseCase ListarTramites);

    private sealed class RelojDeterministico
    {
        private DateTime _actual;

        public RelojDeterministico(DateTime inicial)
        {
            _actual = inicial;
        }

        public DateTime Ahora()
        {
            _actual = _actual.AddMinutes(1);
            return _actual;
        }
    }

    private sealed class AutorizacionDenegada : IAutorizacionService
    {
        public bool PoseeElPermiso(Guid idUsuario, Permiso permiso)
        {
            return false;
        }
    }
}
