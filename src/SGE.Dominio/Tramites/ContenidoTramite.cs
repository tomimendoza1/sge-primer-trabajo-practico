using SGE.Dominio.Comun;

namespace SGE.Dominio.Tramites;

public sealed record ContenidoTramite
{
    public string Valor { get; }

    public ContenidoTramite(string valor)
    {
        if (string.IsNullOrWhiteSpace(valor))
        {
            throw new DominioException("El contenido del tramite no puede estar vacio.");
        }

        Valor = valor.Trim();
    }

    public override string ToString() => Valor;
}
