# SGE - Sistema de Gestion de Expedientes

Primer Trabajo Practico de Taller de .NET.

Este proyecto implementa una primera version de un Sistema de Gestion de Expedientes (SGE), usando .NET 8, arquitectura limpia y criterios de diseno orientado al dominio. La aplicacion se ejecuta desde consola y simula los casos de uso principales pedidos en la consigna.

Repositorio publico:

```text
https://github.com/tomimendoza1/sge-primer-trabajo-practico
```

## Objetivo del trabajo

El objetivo fue desarrollar el nucleo logico de un sistema que permita administrar expedientes y sus tramites asociados. La solucion separa responsabilidades en capas para que las reglas de negocio no dependan de detalles externos como consola, archivos o servicios de autorizacion.

La primera entrega no usa base de datos. La persistencia se resolvio con archivos TXT desde la capa de infraestructura, como implementacion provisoria.

## Estructura de la solucion

La solucion se llama `SGE.sln` y contiene cinco proyectos:

```text
src/
  SGE.Dominio/
  SGE.Aplicacion/
  SGE.Infraestructura/
  SGE.Consola/
tests/
  SGE.Tests/
```

### SGE.Dominio

Contiene las reglas principales del negocio.

Elementos implementados:

- Entidad `Expediente`.
- Entidad `Tramite`.
- Value Object `Caratula`.
- Value Object `ContenidoTramite`.
- Enum `EstadoExpediente`.
- Enum `EtiquetaTramite`.
- Excepcion `DominioException`.

Reglas incluidas:

- Un expediente nuevo inicia siempre con estado `RecienIniciado`.
- La caratula no puede estar vacia.
- El contenido de un tramite no puede estar vacio.
- La fecha de ultima modificacion no puede ser menor que la fecha de creacion.
- El expediente puede modificar su caratula registrando usuario y fecha.
- El expediente puede cambiar de estado manualmente.
- El expediente puede actualizar su estado automaticamente segun el ultimo tramite.

Ejemplos de cambios automaticos:

- Tramite `Resolucion` cambia el expediente a `ConResolucion`.
- Tramite `PaseAEstudio` cambia el expediente a `ParaResolver`.
- Tramite `PaseAlArchivo` cambia el expediente a `Finalizado`.

### SGE.Aplicacion

Contiene los casos de uso y los contratos que necesita la aplicacion.

Elementos implementados:

- Interfaces `IExpedienteRepository` e `ITramiteRepository`.
- Interface `IAutorizacionService`.
- Enum `Permiso`.
- DTOs `ExpedienteDto` y `TramiteDto`.
- Excepciones `EntidadNoEncontradaException` y `AutorizacionException`.
- Casos de uso para expedientes.
- Casos de uso para tramites.

Casos de uso implementados:

- Alta de expediente.
- Modificacion de caratula.
- Cambio manual de estado.
- Baja de expediente.
- Listado de expedientes.
- Alta de tramite.
- Modificacion de tramite.
- Baja de tramite.
- Listado de tramites por expediente.

La capa de aplicacion coordina repositorios, autorizacion y dominio. No contiene persistencia concreta.

### SGE.Infraestructura

Contiene implementaciones concretas de servicios externos o tecnicos.

Elementos implementados:

- `ExpedienteTxtRepository`.
- `TramiteTxtRepository`.
- `AutorizacionProvisionalService`.
- `RepositorioException`.

Los repositorios guardan datos en archivos TXT. Al leer desde archivo usan metodos `Reconstruir` del dominio, para hidratar entidades existentes sin pasar por los constructores de creacion normal.

La autorizacion provisional devuelve siempre `true`. En `Program.cs` tambien se incluye una autorizacion denegada para probar el caso de error.

### SGE.Consola

Es la aplicacion ejecutable y tambien funciona como `Composition Root`.

En `Program.cs` se instancian:

- Repositorios TXT.
- Servicio de autorizacion.
- Servicio de actualizacion de estado.
- Casos de uso.

Luego se ejecutan escenarios de prueba por consola.

### SGE.Tests

Contiene pruebas unitarias con xUnit.

Pruebas incluidas:

- Validacion de `Caratula`.
- Validacion de `ContenidoTramite`.
- Estado inicial de expediente.
- Modificacion de caratula.
- Cambio automatico de estado.
- Creacion de expediente desde caso de uso.
- Cambio de estado al agregar tramite `Resolucion`.
- Baja de expediente con eliminacion de tramites asociados.
- Error por falta de permisos.
- Error por entidad inexistente.

## Requisitos para ejecutar

- .NET SDK 8 instalado.
- PowerShell o terminal compatible.

Para verificar la instalacion:

```powershell
dotnet --version
```

## Como compilar

Ubicarse en la carpeta del proyecto:

```powershell
cd "C:\Users\mendo\OneDrive\Documentos\GitHub\proyecto .net\sge-primer-trabajo-practico"
```

Compilar la solucion completa:

```powershell
dotnet build SGE.sln -m:1
```

El parametro `-m:1` fuerza compilacion secuencial. Se usa para evitar problemas de bloqueo de archivos intermedios en Windows/OneDrive.

Salida esperada resumida:

```text
SGE.Dominio -> ...\SGE.Dominio.dll
SGE.Aplicacion -> ...\SGE.Aplicacion.dll
SGE.Infraestructura -> ...\SGE.Infraestructura.dll
SGE.Consola -> ...\SGE.Consola.dll
SGE.Tests -> ...\SGE.Tests.dll

Compilacion correcta.
    0 Advertencia(s)
    0 Errores
```

## Como ejecutar la aplicacion

Ejecutar la app de consola:

```powershell
dotnet run --project src\SGE.Consola\SGE.Consola.csproj
```

Si ya fue compilada y se quiere ejecutar mas rapido:

```powershell
dotnet run --project src\SGE.Consola\SGE.Consola.csproj --no-build
```

## Como correr los tests

```powershell
dotnet test SGE.sln -m:1
```

Salida esperada resumida:

```text
Correctas! - Con error: 0, Superado: 12, Omitido: 0, Total: 12
```

## Funcionalidad probada desde Program.cs

El archivo `src/SGE.Consola/Program.cs` ejecuta cuatro escenarios:

1. Camino feliz.
2. Error de dominio.
3. Entidad inexistente.
4. Autorizacion denegada.

### Inicializacion de dependencias

El programa crea archivos TXT temporales dentro de la carpeta de salida de la consola:

```csharp
var datos = Path.Combine(AppContext.BaseDirectory, "datos");
Directory.CreateDirectory(datos);
var expedientesTxt = Path.Combine(datos, "expedientes.txt");
var tramitesTxt = Path.Combine(datos, "tramites.txt");
File.WriteAllText(expedientesTxt, string.Empty);
File.WriteAllText(tramitesTxt, string.Empty);
```

Luego instancia repositorios, autorizacion y casos de uso:

```csharp
var expedienteRepository = new ExpedienteTxtRepository(expedientesTxt);
var tramiteRepository = new TramiteTxtRepository(tramitesTxt);
var autorizacion = new AutorizacionProvisionalService();
var actualizacionEstado = new ActualizacionEstadoExpedienteService(expedienteRepository, tramiteRepository);

var agregarExpediente = new AgregarExpedienteUseCase(expedienteRepository, autorizacion);
var cambiarEstado = new CambiarEstadoExpedienteUseCase(expedienteRepository, autorizacion);
var agregarTramite = new AgregarTramiteUseCase(expedienteRepository, tramiteRepository, actualizacionEstado, autorizacion);
```

Esto respeta la idea de arquitectura limpia: `Program.cs` conoce las implementaciones concretas, pero los casos de uso trabajan contra interfaces.

### Escenario 1: camino feliz

Codigo representativo:

```csharp
var expediente = agregarExpediente
    .Ejecutar(new AgregarExpedienteRequest("Solicitud de habilitacion comercial", usuario))
    .Expediente;

Console.WriteLine($"Expediente creado: {expediente.Id} - {expediente.Estado}");

agregarTramite.Ejecutar(new AgregarTramiteRequest(
    expediente.Id,
    EtiquetaTramite.EscritoPresentado,
    "Se presenta documentacion inicial.",
    usuario));

var resolucion = agregarTramite.Ejecutar(new AgregarTramiteRequest(
    expediente.Id,
    EtiquetaTramite.Resolucion,
    "Se resuelve favorablemente.",
    usuario)).Tramite;

var actualizado = expedienteRepository.ObtenerPorId(expediente.Id)!;
Console.WriteLine($"Estado automatico luego de resolucion: {actualizado.Estado}");
```

Que se prueba:

- Crear un expediente.
- Agregar un tramite inicial.
- Agregar un tramite de resolucion.
- Verificar que el expediente cambie automaticamente a `ConResolucion`.

Luego se prueba tambien:

```csharp
cambiarEstado.Ejecutar(new CambiarEstadoExpedienteRequest(
    expediente.Id,
    EstadoExpediente.EnNotificacion,
    usuario));

modificarCaratula.Ejecutar(new ModificarCaratulaExpedienteRequest(
    expediente.Id,
    "Solicitud de habilitacion comercial corregida",
    usuario));

modificarTramite.Ejecutar(new ModificarTramiteRequest(
    resolucion.Id,
    EtiquetaTramite.Notificacion,
    "Se notifica la resolucion.",
    usuario));
```

Que se prueba:

- Cambio manual de estado.
- Modificacion de caratula.
- Modificacion de tramite.
- Listado final de expediente y tramites.

Salida esperada:

```text
== Camino feliz ==
Expediente creado: <guid generado> - RecienIniciado
Estado automatico luego de resolucion: ConResolucion
Listado expediente: Solicitud de habilitacion comercial corregida - EnNotificacion
Tramite: EscritoPresentado - Se presenta documentacion inicial.
Tramite: Notificacion - Se notifica la resolucion.
```

El valor `<guid generado>` cambia en cada ejecucion porque se genera dinamicamente.

### Escenario 2: error de dominio

Codigo:

```csharp
agregarExpediente.Ejecutar(new AgregarExpedienteRequest("   ", usuario));
```

Que se prueba:

- La caratula vacia no es valida.
- El value object `Caratula` lanza `DominioException`.

Salida esperada:

```text
== Error de dominio ==
DominioException: La caratula no puede estar vacia.
```

### Escenario 3: entidad inexistente

Codigo:

```csharp
cambiarEstado.Ejecutar(new CambiarEstadoExpedienteRequest(
    Guid.NewGuid(),
    EstadoExpediente.Finalizado,
    usuario));
```

Que se prueba:

- No se puede modificar un expediente que no existe.
- El caso de uso lanza `EntidadNoEncontradaException`, capturada como `AplicacionException`.

Salida esperada:

```text
== Entidad inexistente ==
AplicacionException: No se encontro el expediente solicitado.
```

### Escenario 4: autorizacion denegada

Codigo:

```csharp
var autorizacionDenegada = new AutorizacionDenegadaService();
var caso = new AgregarExpedienteUseCase(expedienteRepository, autorizacionDenegada);
caso.Ejecutar(new AgregarExpedienteRequest("No deberia crearse", usuario));
```

La clase usada para simular la denegacion:

```csharp
internal sealed class AutorizacionDenegadaService : IAutorizacionService
{
    public bool PoseeElPermiso(Guid idUsuario, Permiso permiso)
    {
        return false;
    }
}
```

Que se prueba:

- Los casos de uso mutantes validan permisos.
- Si el usuario no tiene permiso, se lanza `AutorizacionException`.

Salida esperada:

```text
== Autorizacion denegada ==
AplicacionException: El usuario no posee el permiso requerido: ExpedienteAlta.
```

## Salida completa esperada de la consola

Ejecutando:

```powershell
dotnet run --project src\SGE.Consola\SGE.Consola.csproj
```

Se obtiene una salida como la siguiente:

```text
SGE - Simulacion de casos de uso

== Camino feliz ==
Expediente creado: f6dbe875-f6f3-4bb4-828c-0e39771b2da4 - RecienIniciado
Estado automatico luego de resolucion: ConResolucion
Listado expediente: Solicitud de habilitacion comercial corregida - EnNotificacion
Tramite: EscritoPresentado - Se presenta documentacion inicial.
Tramite: Notificacion - Se notifica la resolucion.

== Error de dominio ==
DominioException: La caratula no puede estar vacia.

== Entidad inexistente ==
AplicacionException: No se encontro el expediente solicitado.

== Autorizacion denegada ==
AplicacionException: El usuario no posee el permiso requerido: ExpedienteAlta.
```

Nota: el GUID del expediente sera distinto en cada ejecucion.

## Observaciones de diseno

- El dominio no depende de infraestructura ni de consola.
- La aplicacion usa interfaces para los repositorios.
- La infraestructura implementa persistencia TXT de forma reemplazable.
- `Program.cs` es el unico lugar donde se instancian implementaciones concretas.
- Las mutaciones pasan por casos de uso y validan permisos.
- Los errores se modelan con excepciones especificas segun la capa.
- Los tests permiten verificar reglas de dominio y comportamiento de casos de uso sin depender de interaccion manual.
