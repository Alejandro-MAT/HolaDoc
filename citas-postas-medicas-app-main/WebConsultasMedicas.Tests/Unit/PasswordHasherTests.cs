using WebConsultasMedicas.Security;
using Xunit;

namespace WebConsultasMedicas.Tests.Unit;

/// <summary>
/// Pruebas unitarias puras (sin base de datos) para el hasher de contraseñas.
/// Verifica que el hash sea determinístico, sensible a mayúsculas/minúsculas
/// y siempre en formato hexadecimal en minúsculas de 64 caracteres (SHA-256).
/// </summary>
public class PasswordHasherTests
{
    [Fact]
    public void Sha256Hex_MismaEntrada_GeneraSiempreElMismoHash()
    {
        var hash1 = PasswordHasher.Sha256Hex("admin123");
        var hash2 = PasswordHasher.Sha256Hex("admin123");

        Assert.Equal(hash1, hash2);
    }

    [Fact]
    public void Sha256Hex_EntradasDistintas_GeneranHashesDistintos()
    {
        var hash1 = PasswordHasher.Sha256Hex("admin123");
        var hash2 = PasswordHasher.Sha256Hex("admin1234");

        Assert.NotEqual(hash1, hash2);
    }

    [Fact]
    public void Sha256Hex_EsSensibleAMayusculas()
    {
        var hashMinuscula = PasswordHasher.Sha256Hex("admin123");
        var hashMayuscula = PasswordHasher.Sha256Hex("ADMIN123");

        Assert.NotEqual(hashMinuscula, hashMayuscula);
    }

    [Theory]
    [InlineData("")]
    [InlineData("a")]
    [InlineData("contraseñaMuyLargaConMasDeSesentaYCuatroCaracteresParaProbarElLimite123456789")]
    public void Sha256Hex_SiempreDevuelve64CaracteresHexadecimales(string input)
    {
        var hash = PasswordHasher.Sha256Hex(input);

        Assert.Equal(64, hash.Length);
        Assert.Matches("^[0-9a-f]{64}$", hash);
    }

    [Fact]
    public void Sha256Hex_ValorConocido_CoincideConHashEsperado()
    {
        // Valor de referencia calculado independientemente para "admin123".
        // Si este test falla, algo cambió en el algoritmo de hashing.
        var hash = PasswordHasher.Sha256Hex("admin123");

        Assert.Equal("240be518fabd2724ddb6f04eeb1da5967448d7e831c08c8fa822809f74c720a9", hash);
    }
}
