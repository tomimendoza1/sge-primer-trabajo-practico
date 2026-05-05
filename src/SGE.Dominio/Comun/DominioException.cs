namespace SGE.Dominio.Comun;

public sealed class DominioException : Exception
{
    public DominioException(string mensaje) : base(mensaje)
    {
    }
}
