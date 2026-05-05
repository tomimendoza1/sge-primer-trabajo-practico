using SGE.Dominio.Comun;

namespace SGE.Dominio.Expedientes;

public sealed record Caratula
{
    public string Valor { get; }

    public Caratula(string valor)
    {
        if (string.IsNullOrWhiteSpace(valor))
        {
            throw new DominioException("La caratula no puede estar vacia.");
        }

        Valor = valor.Trim();
    }

    public override string ToString() => Valor;
}
