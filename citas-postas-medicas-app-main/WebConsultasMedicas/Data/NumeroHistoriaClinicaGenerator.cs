using Microsoft.EntityFrameworkCore;

namespace WebConsultasMedicas.Data;

public static class NumeroHistoriaClinicaGenerator
{
    public const string Prefix = "HCLIN";
    private const int Digits = 5;

    // Esta nota genera el código de historia clínica bien bacán.
    public static async Task<string> NextAsync(ApplicationDbContext context, CancellationToken cancellationToken = default)
    {
        // Usamos un tamaño fijo de ceros para que el orden alfabético cuadre igualito que el numérico.
        // O sea, va a salir algo como: HCLIN00001 .. HCLIN99999
        var last = await context.Pacientes.AsNoTracking()
            .Select(p => p.NumeroSIS)
            .Where(n => n.StartsWith(Prefix) && n.Length == Prefix.Length + Digits)
            .OrderByDescending(n => n)
            .FirstOrDefaultAsync(cancellationToken);

        var nextNumber = 1;
        if (!string.IsNullOrWhiteSpace(last))
        {
            var suffix = last.Substring(Prefix.Length, Digits);
            if (int.TryParse(suffix, out var parsed))
            {
                nextNumber = parsed + 1;
            }
        }

        if (nextNumber > 99999)
        {
            throw new InvalidOperationException("Se alcanzó el máximo de historias clínicas (99999).");
        }

        return $"{Prefix}{nextNumber:D5}";
    }
}

