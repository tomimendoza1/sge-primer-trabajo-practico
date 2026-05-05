namespace SGE.Aplicacion.Comun;

public class AplicacionException : Exception
{
    public AplicacionException(string mensaje) : base(mensaje)
    {
    }
}

public sealed class EntidadNoEncontradaException : AplicacionException
{
    public EntidadNoEncontradaException(string mensaje) : base(mensaje)
    {
    }
}

public sealed class AutorizacionException : AplicacionException
{
    public AutorizacionException(string mensaje) : base(mensaje)
    {
    }
}
