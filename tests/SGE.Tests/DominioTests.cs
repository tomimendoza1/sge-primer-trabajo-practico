using SGE.Dominio.Comun;
using SGE.Dominio.Expedientes;
using SGE.Dominio.Tramites;

namespace SGE.Tests;

public sealed class DominioTests
{
    private static readonly Guid Usuario = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly DateTime Fecha = new(2026, 5, 5, 10, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Caratula_Rechaza_Texto_Vacio()
    {
        Assert.Throws<DominioException>(() => new Caratula(" "));
    }

    [Fact]
    public void ContenidoTramite_Rechaza_Texto_Vacio()
    {
        Assert.Throws<DominioException>(() => new ContenidoTramite(""));
    }

    [Fact]
    public void Expediente_Nuevo_Inicia_Recien_Iniciado()
    {
        var expediente = Expediente.Crear(new Caratula("Expediente inicial"), Usuario, Fecha);

        Assert.Equal(EstadoExpediente.RecienIniciado, expediente.Estado);
        Assert.Equal(Fecha, expediente.FechaCreacion);
        Assert.Equal(Fecha, expediente.FechaUltimaModificacion);
    }

    [Fact]
    public void Modificar_Caratula_Actualiza_Datos_De_Auditoria()
    {
        var expediente = Expediente.Crear(new Caratula("Original"), Usuario, Fecha);
        var otroUsuario = Guid.Parse("22222222-2222-2222-2222-222222222222");
        var fechaModificacion = Fecha.AddHours(2);

        expediente.ModificarCaratula(new Caratula("Corregida"), otroUsuario, fechaModificacion);

        Assert.Equal("Corregida", expediente.Caratula.Valor);
        Assert.Equal(otroUsuario, expediente.UsuarioUltimoCambio);
        Assert.Equal(fechaModificacion, expediente.FechaUltimaModificacion);
    }

    [Theory]
    [InlineData(EtiquetaTramite.Resolucion, EstadoExpediente.ConResolucion)]
    [InlineData(EtiquetaTramite.PaseAEstudio, EstadoExpediente.ParaResolver)]
    [InlineData(EtiquetaTramite.PaseAlArchivo, EstadoExpediente.Finalizado)]
    public void Actualizar_Estado_Automatico_Segun_Ultimo_Tramite(EtiquetaTramite etiqueta, EstadoExpediente esperado)
    {
        var expediente = Expediente.Crear(new Caratula("Expediente"), Usuario, Fecha);

        var cambio = expediente.ActualizarEstado(etiqueta, Usuario, Fecha.AddMinutes(1));

        Assert.True(cambio);
        Assert.Equal(esperado, expediente.Estado);
    }
}
