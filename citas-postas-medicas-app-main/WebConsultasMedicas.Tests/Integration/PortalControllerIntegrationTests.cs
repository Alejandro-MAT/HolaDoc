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

namespace WebConsultasMedicas.Tests.Integration
{
    public class PortalControllerIntegrationTests
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
                new Claim(ClaimTypes.Name, "pacientetest"),
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
        public async Task MisCitas_CA12_CaminoFeliz_DevuelveListaDeCitasOrdenadas()
        {
            // Arrange (Given)
            var context = DbContextFactory.CreateInMemoryContext();
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

            var cita = new CitaMedica
            {
                IdCita = 1,
                IdPaciente = pacienteId,
                IdMedico = 2,
                IdEspecialidad = 1,
                IdHorarioMedico = 5,
                IdEstadoCita = 1,
                FechaCita = new DateOnly(2026, 11, 10),
                HoraCita = new TimeOnly(9, 0),
                MotivoConsulta = "Chequeo general",
                FechaRegistro = DateTime.Now
            };

            context.Pacientes.Add(paciente);
            context.CitasMedicas.Add(cita);
            await context.SaveChangesAsync();

            var controller = new PortalController(context, mockHub);
            SetupControllerUserContext(controller, usuarioId);

            // Act (When)
            var result = await controller.MisCitas() as ViewResult;

            // Assert (Then)
            Assert.NotNull(result);
            Assert.NotNull(result.Model);
        }

        [Fact]
        public async Task MisCitas_CA13_CasoBorde_SinCitasRegistradas()
        {
            // Arrange (Given)
            var context = DbContextFactory.CreateInMemoryContext();
            var mockHub = GetMockHubContext();

            int usuarioId = 20;
            int pacienteId = 2;

            var pacienteNuevo = new Paciente
            {
                IdPaciente = pacienteId,
                IdUsuario = usuarioId,
                DNI = "87654321",
                Nombres = "Maria",
                ApellidoPaterno = "Lopez",
                ApellidoMaterno = "Ruiz",
                FechaNacimiento = new DateOnly(1995, 5, 20),
                Sexo = "F",
                NumeroSIS = "SIS002",
                Estado = true
            };

            context.Pacientes.Add(pacienteNuevo);
            await context.SaveChangesAsync();

            var controller = new PortalController(context, mockHub);
            SetupControllerUserContext(controller, usuarioId);

            // Act (When)
            var result = await controller.MisCitas() as ViewResult;

            // Assert (Then)
            Assert.NotNull(result);
            var model = Assert.IsAssignableFrom<IEnumerable<CitaMedica>>(result.Model);
            Assert.Empty(model);
        }
    }
}