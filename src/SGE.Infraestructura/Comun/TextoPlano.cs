using System.Globalization;
using System.Text;

namespace SGE.Infraestructura.Comun;

internal static class TextoPlano
{
    public static string Codificar(string valor)
    {
        return Convert.ToBase64String(Encoding.UTF8.GetBytes(valor));
    }

    public static string Decodificar(string valor)
    {
        return Encoding.UTF8.GetString(Convert.FromBase64String(valor));
    }

    public static string Fecha(DateTime valor)
    {
        return valor.ToUniversalTime().ToString("O", CultureInfo.InvariantCulture);
    }

    public static DateTime LeerFecha(string valor)
    {
        return DateTime.Parse(valor, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);
    }

    public static void AsegurarArchivo(string ruta)
    {
        var directorio = Path.GetDirectoryName(ruta);
        if (!string.IsNullOrWhiteSpace(directorio))
        {
            Directory.CreateDirectory(directorio);
        }

        if (!File.Exists(ruta))
        {
            File.WriteAllText(ruta, string.Empty);
        }
    }
}
