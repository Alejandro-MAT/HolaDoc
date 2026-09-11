using System.ComponentModel.DataAnnotations;
using WebConsultasMedicas.Models;
using Xunit;

namespace WebConsultasMedicas.Tests.Unit;

/// <summary>
/// Pruebas de validación de datos del modelo Usuario, centradas en el
/// formato de correo (EmailAddress) y la obligatoriedad de la clave.
/// </summary>
public class UsuarioValidationTests
{
    private static Usuario CrearUsuarioValido() => new()
    {
        Correo = "paciente@siscitasweb.local",
        ClaveHash = "hash-de-prueba",
        IdRol = 1
    };

    private static IList<ValidationResult> Validar(Usuario usuario)
    {
        var context = new ValidationContext(usuario);
        var results = new List<ValidationResult>();
        Validator.TryValidateObject(usuario, context, results, validateAllProperties: true);
        return results;
    }

    [Fact]
    public void Usuario_ConDatosValidos_NoGeneraErroresDeValidacion()
    {
        var usuario = CrearUsuarioValido();

        var errores = Validar(usuario);

        Assert.Empty(errores);
    }

    [Theory]
    [InlineData("correo-sin-arroba.com")]
    [InlineData("correo@")]
    [InlineData("@dominio.com")]
    public void Usuario_ConCorreoConFormatoInvalido_GeneraErrorDeValidacion(string correoInvalido)
    {
        var usuario = CrearUsuarioValido();
        usuario.Correo = correoInvalido;

        var errores = Validar(usuario);

        Assert.Contains(errores, e => e.MemberNames.Contains(nameof(Usuario.Correo)));
    }

    [Fact]
    public void Usuario_SinCorreo_GeneraErrorDeValidacion()
    {
        var usuario = CrearUsuarioValido();
        usuario.Correo = "";

        var errores = Validar(usuario);

        Assert.Contains(errores, e => e.MemberNames.Contains(nameof(Usuario.Correo)));
    }

    [Fact]
    public void Usuario_SinClaveHash_GeneraErrorDeValidacion()
    {
        var usuario = CrearUsuarioValido();
        usuario.ClaveHash = "";

        var errores = Validar(usuario);

        Assert.Contains(errores, e => e.MemberNames.Contains(nameof(Usuario.ClaveHash)));
    }

    [Fact]
    public void Usuario_ConCorreoDe101Caracteres_GeneraErrorDeValidacion()
    {
        // StringLength(100): 101 caracteres debe fallar (justo un carácter sobre el límite).
        var usuario = CrearUsuarioValido();
        usuario.Correo = new string('a', 90) + "@" + new string('b', 6) + ".com"; // > 100 caracteres en total

        var errores = Validar(usuario);

        Assert.Contains(errores, e => e.MemberNames.Contains(nameof(Usuario.Correo)));
    }
}
