namespace SGE.Infraestructura.Comun;

public sealed class RepositorioException : Exception
{
    public RepositorioException(string mensaje) : base(mensaje)
    {
    }
}
