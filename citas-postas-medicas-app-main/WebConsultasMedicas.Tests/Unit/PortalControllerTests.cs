using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Moq;
using WebConsultasMedicas.Controllers;
using WebConsultasMedicas.Hubs;
using WebConsultasMedicas.Models;
using WebConsultasMedicas.Tests.TestHelpers;
using Xunit;

namespace WebConsultasMedicas.Tests.Unit
{
    public class PortalControllerTests
    {
        private IHubContext<CitasHub> GetMockHubContext()
        {
            var mockHubContext = new Mock<IHubContext<CitasHub>>();
            var mockClients = new Mock<IHubClients>();
            var mockClientProxy = new Mock<IClientProxy>();

            mockHubContext.Setup(h => h.Clients).Returns(mockClients.Object);
            mockClients.Setup(c => c.All).Returns(mockClientProxy.Object);

            return mockHubContext.Object;
        }

        private static void SetupControllerUserContext(ControllerBase controller, int usuarioId, string rol = "Paciente")
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, usuarioId.ToString()),
                new Claim(ClaimTypes.Name, "testuser"),
                new Claim(ClaimTypes.Role, rol)
            };
            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var claimsPrincipal = new ClaimsPrincipal(identity);

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = claimsPrincipal }
            };
        }

        [Fact]
        public void PortalController_InstanciacionCorrecta()
        {
            // Arrange
            using var context = DbContextFactory.CreateInMemoryContext();
            var mockHub = GetMockHubContext();

            // Act
            var controller = new PortalController(context, mockHub);

            // Assert
            Assert.NotNull(controller);
        }

        [Fact]
        public async Task Buscar_CaminoFeliz_DevuelveVistaConMedicos()
        {
            // Arrange
            using var context = DbContextFactory.CreateInMemoryContext();
            var mockHub = GetMockHubContext();
            var controller = new PortalController(context, mockHub);

            // Act
            var result = await controller.Buscar(null, null) as ViewResult;

            // Assert
            Assert.NotNull(result);
            Assert.IsType<PortalBuscarMedicosViewModel>(result.Model);
        }

        [Fact]
        public async Task MisCitas_CA12_CaminoFeliz_DevuelveCitasDelPaciente()
        {
            // Arrange
            using var context = DbContextFactory.CreateInMemoryContext();
            var mockHub = GetMockHubContext();

            int usuarioId = 10;
            int pacienteId = 1;

            var paciente = new Paciente
            {
                IdPaciente = pacienteId,
                IdUsuario = usuarioId,
                DNI = "12345678",
                Nombres = "Juan",
                ApellidoPaterno = "Pérez",
                ApellidoMaterno = "Gómez",
                FechaNacimiento = new DateOnly(1990, 1, 1),
                Sexo = "M",
                NumeroSIS = "SIS001",
                Estado = true
            };

            context.Pacientes.Add(paciente);
            await context.SaveChangesAsync();

            var controller = new PortalController(context, mockHub);
            SetupControllerUserContext(controller, usuarioId);

            // Act
            var result = await controller.MisCitas() as ViewResult;

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.Model);
        }
    }
}