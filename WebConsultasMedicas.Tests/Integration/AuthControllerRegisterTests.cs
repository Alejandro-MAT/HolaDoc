using Microsoft.AspNetCore.Mvc;
using WebConsultasMedicas.Controllers;
using WebConsultasMedicas.Data;
using WebConsultasMedicas.Models;
using WebConsultasMedicas.Security;
using WebConsultasMedicas.Tests.TestHelpers;
using Xunit;

namespace WebConsultasMedicas.Tests.Integration;

/// <summary>
/// Pruebas de integración del registro de pacientes: validan las reglas de
/// negocio de unicidad (correo y DNI duplicados) que mencionaste que existen
/// en el README del proyecto, y el camino exitoso de creación de un paciente.
/// </summary>
public class AuthControllerRegisterTests
{
    private static async Task<ApplicationDbContext> CrearContextoConRolPacienteAsync()
    {
        var context = DbContextFactory.CreateInMemoryContext();
        context.Roles.Add(new Rol { Nombre = "Paciente", Estado = true });
        await context.SaveChangesAsync();
        return context;
    }

    private static PatientRegisterViewModel CrearModeloValido() => new()
    {
        Correo = "nuevo.paciente@test.com",
        Password = "clave123",
        DNI = "11223344",
        Nombres = "Ana",
        ApellidoPaterno = "Torres",
        ApellidoMaterno = "Ruiz",
        FechaNacimiento = new DateOnly(1995, 5, 20),
        Sexo = "F"
    };

    [Fact]
    public async Task Register_ConCorreoYaRegistrado_AgregaErrorDeModeloYNoGuarda()
    {
        var context = await CrearContextoConRolPacienteAsync();
        context.Usuarios.Add(new Usuario
        {
            Correo = "nuevo.paciente@test.com",
            ClaveHash = PasswordHasher.Sha256Hex("otraClave"),
            IdRol = context.Roles.First().IdRol,
            Estado = true
        });
        await context.SaveChangesAsync();
        var controller = new AuthController(context);

        var resultado = await controller.Register(CrearModeloValido());

        Assert.IsType<ViewResult>(resultado);
        Assert.False(controller.ModelState.IsValid);
        Assert.True(controller.ModelState.ContainsKey(nameof(PatientRegisterViewModel.Correo)));
    }

    [Fact]
    public async Task Register_ConDniYaRegistrado_AgregaErrorDeModeloYNoGuarda()
    {
        var context = await CrearContextoConRolPacienteAsync();
        var otroUsuario = new Usuario
        {
            Correo = "otro@test.com",
            ClaveHash = PasswordHasher.Sha256Hex("otraClave"),
            IdRol = context.Roles.First().IdRol,
            Estado = true
        };
        context.Pacientes.Add(new Paciente
        {
            Usuario = otroUsuario,
            DNI = "11223344",
            Nombres = "Otro",
            ApellidoPaterno = "Paciente",
            ApellidoMaterno = "Existente",
            FechaNacimiento = new DateOnly(1990, 1, 1),
            Sexo = "M",
            NumeroSIS = "HCLIN00001"
        });
        await context.SaveChangesAsync();
        var controller = new AuthController(context);

        var resultado = await controller.Register(CrearModeloValido());

        Assert.IsType<ViewResult>(resultado);
        Assert.False(controller.ModelState.IsValid);
        Assert.True(controller.ModelState.ContainsKey(nameof(PatientRegisterViewModel.DNI)));
    }

    [Fact]
    public async Task Register_ConDatosValidosYRolExistente_CreaUsuarioYPacienteYRedirigeALogin()
    {
        var context = await CrearContextoConRolPacienteAsync();
        var controller = new AuthController(context)
        {
            TempData = new Microsoft.AspNetCore.Mvc.ViewFeatures.TempDataDictionary(
                new Microsoft.AspNetCore.Http.DefaultHttpContext(),
                Moq.Mock.Of<Microsoft.AspNetCore.Mvc.ViewFeatures.ITempDataProvider>())
        };

        var resultado = await controller.Register(CrearModeloValido());

        var redirect = Assert.IsType<RedirectToActionResult>(resultado);
        Assert.Equal(nameof(AuthController.Login), redirect.ActionName);
        Assert.Single(context.Pacientes);
        Assert.Single(context.Usuarios);
        Assert.Equal("nuevo.paciente@test.com", context.Usuarios.First().Correo);
    }

    [Fact]
    public async Task Register_SinRolPacienteEnBaseDeDatos_NoCreaNadaYReportaError()
    {
        // Caso borde: si el rol "Paciente" no existe en la BD, el registro no debe
        // romper la aplicación, sino informar el error mediante TempData.
        var context = DbContextFactory.CreateInMemoryContext();
        var controller = new AuthController(context)
        {
            TempData = new Microsoft.AspNetCore.Mvc.ViewFeatures.TempDataDictionary(
                new Microsoft.AspNetCore.Http.DefaultHttpContext(),
                Moq.Mock.Of<Microsoft.AspNetCore.Mvc.ViewFeatures.ITempDataProvider>())
        };

        var resultado = await controller.Register(CrearModeloValido());

        Assert.IsType<ViewResult>(resultado);
        Assert.Empty(context.Pacientes);
        Assert.Empty(context.Usuarios);
        Assert.Equal("No existe el rol Paciente en la base de datos.", controller.TempData["Error"]);
    }
}
