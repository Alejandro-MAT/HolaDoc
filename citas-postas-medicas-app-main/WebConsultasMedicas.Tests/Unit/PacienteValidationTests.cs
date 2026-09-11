using System.ComponentModel.DataAnnotations;
using WebConsultasMedicas.Models;
using Xunit;

namespace WebConsultasMedicas.Tests.Unit;

/// <summary>
/// Pruebas de validación de datos (Data Annotations) del modelo Paciente,
/// aplicando técnicas de partición de equivalencia y análisis de valores límite
/// sobre el campo DNI (obligatorio, exactamente 8 dígitos).
/// </summary>
public class PacienteValidationTests
{
    private static Paciente CrearPacienteValido() => new()
    {
        IdUsuario = 1,
        DNI = "12345678",
        Nombres = "Juan",
        ApellidoPaterno = "Pérez",
        ApellidoMaterno = "Gómez",
        FechaNacimiento = new DateOnly(1990, 1, 1),
        Sexo = "M",
        NumeroSIS = "HCLIN00001"
    };

    private static IList<ValidationResult> Validar(Paciente paciente)
    {
        var context = new ValidationContext(paciente);
        var results = new List<ValidationResult>();
        Validator.TryValidateObject(paciente, context, results, validateAllProperties: true);
        return results;
    }

    [Fact]
    public void Paciente_ConDatosValidos_NoGeneraErroresDeValidacion()
    {
        var paciente = CrearPacienteValido();

        var errores = Validar(paciente);

        Assert.Empty(errores);
    }

    [Theory]
    [InlineData("1234567")]   // 7 dígitos: límite inferior inválido
    [InlineData("123456789")] // 9 dígitos: límite superior inválido
    [InlineData("")]          // vacío
    public void Paciente_ConDniDeLongitudInvalida_GeneraErrorDeValidacion(string dniInvalido)
    {
        var paciente = CrearPacienteValido();
        paciente.DNI = dniInvalido;

        var errores = Validar(paciente);

        Assert.Contains(errores, e => e.MemberNames.Contains(nameof(Paciente.DNI)));
    }

    [Fact]
    public void Paciente_ConDniDeExactamente8Digitos_EsValido()
    {
        // Caso límite exacto (boundary value analysis): 8 es el valor válido justo en el borde.
        var paciente = CrearPacienteValido();
        paciente.DNI = "87654321";

        var errores = Validar(paciente);

        Assert.DoesNotContain(errores, e => e.MemberNames.Contains(nameof(Paciente.DNI)));
    }

    [Fact]
    public void Paciente_SinNombres_GeneraErrorDeValidacion()
    {
        var paciente = CrearPacienteValido();
        paciente.Nombres = null!;

        var errores = Validar(paciente);

        Assert.Contains(errores, e => e.MemberNames.Contains(nameof(Paciente.Nombres)));
    }

    [Theory]
    [InlineData("M")]
    [InlineData("F")]
    public void Paciente_ConSexoValido_NoGeneraErrorEnEseCampo(string sexo)
    {
        var paciente = CrearPacienteValido();
        paciente.Sexo = sexo;

        var errores = Validar(paciente);

        Assert.DoesNotContain(errores, e => e.MemberNames.Contains(nameof(Paciente.Sexo)));
    }

    [Fact]
    public void Paciente_ConSexoDeDosCaracteres_GeneraErrorDeValidacion()
    {
        // La anotación exige StringLength(1, MinimumLength = 1).
        var paciente = CrearPacienteValido();
        paciente.Sexo = "MX";

        var errores = Validar(paciente);

        Assert.Contains(errores, e => e.MemberNames.Contains(nameof(Paciente.Sexo)));
    }

    [Fact]
    public void Paciente_SinNumeroSIS_GeneraErrorDeValidacion()
    {
        var paciente = CrearPacienteValido();
        paciente.NumeroSIS = null!;

        var errores = Validar(paciente);

        Assert.Contains(errores, e => e.MemberNames.Contains(nameof(Paciente.NumeroSIS)));
    }
}
