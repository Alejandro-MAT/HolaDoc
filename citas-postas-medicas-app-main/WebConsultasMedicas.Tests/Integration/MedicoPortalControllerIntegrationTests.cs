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
    public class MedicoPortalControllerIntegrationTests
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

        private static void SetupControllerUserContext(ControllerBase controller, int usuarioId = 1, string rol = "Medico")
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, usuarioId.ToString()),
                new Claim(ClaimTypes.Name, "medicouser"),
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
        public async Task Atender_CA17_CasoBorde_BloqueaAtencionDeCitaCancelada()
        {
            // Arrange
            var context = DbContextFactory.CreateInMemoryContext();
            var mockHub = GetMockHubContext();

            var citaCancelada = new CitaMedica
            {
                IdCita = 100,
                IdPaciente = 1,
                IdMedico = 1,
                IdEspecialidad = 1,
                IdHorarioMedico = 1,
                IdEstadoCita = 3, // Cancelada
                FechaCita = new DateOnly(2026, 10, 10),
                HoraCita = new TimeOnly(10, 0),
                FechaRegistro = DateTime.Now
            };

            context.CitasMedicas.Add(citaCancelada);
            await context.SaveChangesAsync();

            var controller = new MedicoPortalController(context, mockHub);
            SetupControllerUserContext(controller, usuarioId: 1, rol: "Medico");

            // Act
            var result = await controller.Atender(100);

            // Assert
            var citaEnDb = await context.CitasMedicas.FindAsync(100);
            Assert.NotNull(citaEnDb);
            Assert.Equal(3, citaEnDb.IdEstadoCita);
        }
    }
}