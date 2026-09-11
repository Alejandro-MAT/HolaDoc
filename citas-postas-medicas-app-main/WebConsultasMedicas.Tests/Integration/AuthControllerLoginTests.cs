using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using WebConsultasMedicas.Controllers;
using WebConsultasMedicas.Data;
using WebConsultasMedicas.Models;
using WebConsultasMedicas.Security;
using WebConsultasMedicas.Tests.TestHelpers;
using Xunit;

namespace WebConsultasMedicas.Tests.Integration;

/// <summary>
/// Pruebas de integración (controlador + base de datos en memoria) del flujo de login.
/// Cubren los casos: credenciales válidas, usuario inexistente, usuario inactivo,
/// contraseña incorrecta y modelo inválido — es decir, las particiones de
/// equivalencia principales de "iniciar sesión".
/// </summary>
public class AuthControllerLoginTests
{
    /// <summary>
    /// SignInAsync es un método de extensión que internamente pide IAuthenticationService
    /// al contenedor de servicios del HttpContext. Para probar el camino "feliz" del login
    /// sin depender de un servidor real, simulamos ese servicio con Moq y se lo inyectamos
    /// al HttpContext de prueba.
    /// </summary>
    private static (AuthController controller, Mock<IAuthenticationService> authServiceMock) CrearControllerConHttpContextFalso(ApplicationDbContext context)
    {
        var authServiceMock = new Mock<IAuthenticationService>();
        authServiceMock
            .Setup(s => s.SignInAsync(It.IsAny<HttpContext>(), It.IsAny<string>(), It.IsAny<System.Security.Claims.ClaimsPrincipal>(), It.IsAny<AuthenticationProperties>()))
            .Returns(Task.CompletedTask);

        var services = new ServiceCollection();
        services.AddSingleton(authServiceMock.Object);
        var serviceProvider = services.BuildServiceProvider();

        var httpContext = new DefaultHttpContext { RequestServices = serviceProvider };

        var controller = new AuthController(context)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            }
        };

        return (controller, authServiceMock);
    }

    private static async Task<(ApplicationDbContext context, Rol rolPaciente)> CrearContextoConUsuarioAsync(
        string correo, string password, bool estadoActivo = true, string rolNombre = "Paciente")
    {
        var context = DbContextFactory.CreateInMemoryContext();

        var rol = new Rol { Nombre = rolNombre, Estado = true };
        context.Roles.Add(rol);

        var usuario = new Usuario
        {
            Correo = correo,
            ClaveHash = PasswordHasher.Sha256Hex(password),
            Rol = rol,
            Estado = estadoActivo,
            FechaRegistro = DateTime.Now
        };
        context.Usuarios.Add(usuario);

        await context.SaveChangesAsync();

        return (context, rol);
    }

    [Fact]
    public async Task Login_ConCredencialesValidas_IniciaSesionYRedirigeSegunRol()
    {
        var (context, _) = await CrearContextoConUsuarioAsync("paciente@test.com", "clave123", rolNombre: "Paciente");
        var (controller, authServiceMock) = CrearControllerConHttpContextFalso(context);

        var resultado = await controller.Login(new LoginViewModel
        {
            EmailAddress = "paciente@test.com",
            Password = "clave123"
        });

        var redirect = Assert.IsType<RedirectToActionResult>(resultado);
        Assert.Equal("Buscar", redirect.ActionName);
        Assert.Equal("Portal", redirect.ControllerName);
        authServiceMock.Verify(
            s => s.SignInAsync(It.IsAny<HttpContext>(), It.IsAny<string>(), It.IsAny<System.Security.Claims.ClaimsPrincipal>(), It.IsAny<AuthenticationProperties>()),
            Times.Once);
    }

    [Theory]
    [InlineData("Admin", "Inicio", "Admin")]
    [InlineData("Medico", "MisCitas", "MedicoPortal")]
    [InlineData("Paciente", "Buscar", "Portal")]
    public async Task Login_ConCredencialesValidas_RedirigeAlDestinoCorrectoSegunElRol(string rolNombre, string accionEsperada, string controladorEsperado)
    {
        var (context, _) = await CrearContextoConUsuarioAsync("usuario@test.com", "clave123", rolNombre: rolNombre);
        var (controller, _) = CrearControllerConHttpContextFalso(context);

        var resultado = await controller.Login(new LoginViewModel
        {
            EmailAddress = "usuario@test.com",
            Password = "clave123"
        });

        var redirect = Assert.IsType<RedirectToActionResult>(resultado);
        Assert.Equal(accionEsperada, redirect.ActionName);
        Assert.Equal(controladorEsperado, redirect.ControllerName);
    }

    [Fact]
    public async Task Login_ConCorreoInexistente_DevuelveVistaConErrorDeCredenciales()
    {
        var context = DbContextFactory.CreateInMemoryContext();
        var (controller, authServiceMock) = CrearControllerConHttpContextFalso(context);

        var resultado = await controller.Login(new LoginViewModel
        {
            EmailAddress = "no-existe@test.com",
            Password = "cualquiera"
        });

        var view = Assert.IsType<ViewResult>(resultado);
        Assert.False(controller.ModelState.IsValid);
        authServiceMock.Verify(
            s => s.SignInAsync(It.IsAny<HttpContext>(), It.IsAny<string>(), It.IsAny<System.Security.Claims.ClaimsPrincipal>(), It.IsAny<AuthenticationProperties>()),
            Times.Never);
    }

    [Fact]
    public async Task Login_ConContrasenaIncorrecta_DevuelveVistaConErrorDeCredenciales()
    {
        var (context, _) = await CrearContextoConUsuarioAsync("paciente@test.com", "claveCorrecta");
        var (controller, _) = CrearControllerConHttpContextFalso(context);

        var resultado = await controller.Login(new LoginViewModel
        {
            EmailAddress = "paciente@test.com",
            Password = "claveIncorrecta"
        });

        Assert.IsType<ViewResult>(resultado);
        Assert.False(controller.ModelState.IsValid);
    }

    [Fact]
    public async Task Login_ConUsuarioInactivo_DevuelveVistaConErrorDeCredenciales()
    {
        var (context, _) = await CrearContextoConUsuarioAsync("paciente@test.com", "clave123", estadoActivo: false);
        var (controller, authServiceMock) = CrearControllerConHttpContextFalso(context);

        var resultado = await controller.Login(new LoginViewModel
        {
            EmailAddress = "paciente@test.com",
            Password = "clave123"
        });

        Assert.IsType<ViewResult>(resultado);
        Assert.False(controller.ModelState.IsValid);
        authServiceMock.Verify(
            s => s.SignInAsync(It.IsAny<HttpContext>(), It.IsAny<string>(), It.IsAny<System.Security.Claims.ClaimsPrincipal>(), It.IsAny<AuthenticationProperties>()),
            Times.Never);
    }

    [Fact]
    public async Task Login_ConCorreoEnMayusculasYMinusculasDistintas_IgualIniciaSesion()
    {
        // El controlador compara correos en minúsculas, así que el login no debería
        // ser sensible a mayúsculas/minúsculas.
        var (context, _) = await CrearContextoConUsuarioAsync("Paciente@Test.com", "clave123");
        var (controller, _) = CrearControllerConHttpContextFalso(context);

        var resultado = await controller.Login(new LoginViewModel
        {
            EmailAddress = "PACIENTE@TEST.COM",
            Password = "clave123"
        });

        Assert.IsType<RedirectToActionResult>(resultado);
    }

    [Fact]
    public async Task Login_ConModeloInvalido_DevuelveVistaSinConsultarBaseDeDatos()
    {
        var context = DbContextFactory.CreateInMemoryContext();
        var (controller, _) = CrearControllerConHttpContextFalso(context);
        controller.ModelState.AddModelError("Password", "La contraseña es requerida.");

        var resultado = await controller.Login(new LoginViewModel { EmailAddress = "x@test.com", Password = "" });

        Assert.IsType<ViewResult>(resultado);
    }
}
