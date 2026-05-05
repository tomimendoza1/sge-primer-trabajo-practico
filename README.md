# SGE - Primer Trabajo Practico

Sistema de Gestion de Expedientes desarrollado para Taller de .NET.

## Estructura

- `SGE.Dominio`: entidades, value objects, enums y excepciones de dominio.
- `SGE.Aplicacion`: casos de uso, DTOs, contratos de repositorio y autorizacion.
- `SGE.Infraestructura`: repositorios TXT y autorizacion provisional.
- `SGE.Consola`: composition root y simulacion de ejecucion.
- `SGE.Tests`: pruebas unitarias de dominio y aplicacion.

## Ejecutar

```powershell
dotnet build
dotnet test
dotnet run --project src/SGE.Consola/SGE.Consola.csproj
```

## Persistencia

La primera fase usa archivos TXT en la capa de infraestructura. Los repositorios leen usando metodos `Reconstruir` del dominio para no ejecutar constructores de creacion normal al hidratar entidades persistidas.
