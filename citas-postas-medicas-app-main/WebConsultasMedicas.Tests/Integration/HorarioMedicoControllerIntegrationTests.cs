using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Moq;
using WebConsultasMedicas.Controllers;
using WebConsultasMedicas.Models;
using WebConsultasMedicas.Tests.TestHelpers;
using Xunit;

namespace WebConsultasMedicas.Tests.Integration
{
    public class HorarioMedicoControllerIntegrationTests
    {
        private static void SetupControllerUserContext(ControllerBase controller, int usuarioId = 1, string rol = "Administrador")
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, usuarioId.ToString()),
                new Claim(ClaimTypes.Name, "adminuser"),
                new Claim(ClaimTypes.Role, rol)
            };
            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var claimsPrincipal = new ClaimsPrincipal(identity);

            var httpContext = new DefaultHttpContext { User = claimsPrincipal };
            var tempDataProvider = new Mock<ITempDataProvider>();

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };

            if (controller is Controller controllerWithTempData)
            {
                controllerWithTempData.TempData = new TempDataDictionary(httpContext, tempDataProvider.Object);
            }
        }

        [Fact]
        public async Task Create_CA14_CaminoFeliz_RegistraBloqueHorario()
        {
            // Arrange (Given)
            var context = DbContextFactory.CreateInMemoryContext();

            // Sembramos las relaciones para que LoadCombosAsync() no lance NullReference
            var especialidad = new Especialidad { IdEspecialidad = 2, Nombre = "Medicina General", Estado = true };
            var medico = new Medico
            {
                IdMedico = 1,
                Nombres = "Juan",
                ApellidoPaterno = "Perez",
                ApellidoMaterno = "Gomez",
                CMP = "123456",
                IdEspecialidad = 2,
                TiempoAtencionMin = 30, // Requerido por el controlador
                Estado = true
            };
            var turno = new Turno
            {
                IdTurno = 1,
                Nombre = "Mañana",
                HoraInicio = new TimeOnly(8, 0),
                HoraFin = new TimeOnly(12, 0),
                Estado = true
            };

            context.Especialidades.Add(especialidad);
            context.Medicos.Add(medico);
            context.Turnos.Add(turno);
            await context.SaveChangesAsync();

            var controller = new HorarioMedicoController(context);
            SetupControllerUserContext(controller);

            var nuevoHorario = new HorarioMedico
            {
                IdMedico = 1,
                IdTurno = 1,
                Fecha = new DateOnly(2026, 11, 15),
                HoraInicio = new TimeOnly(8, 0),
                HoraFin = new TimeOnly(10, 0),
                Estado = true
            };

            // Act (When)
            var result = await controller.Create(nuevoHorario);

            // Assert (Then)
            Assert.NotNull(result);
            Assert.True(context.HorariosMedicos.Any());
        }

        [Fact]
        public async Task Create_CA15_CasoBorde_DetectaCruceDeHorarios()
        {
            // Arrange (Given)
            var context = DbContextFactory.CreateInMemoryContext();

            var especialidad = new Especialidad { IdEspecialidad = 2, Nombre = "Medicina General", Estado = true };
            var medico = new Medico
            {
                IdMedico = 1,
                Nombres = "Juan",
                ApellidoPaterno = "Perez",
                ApellidoMaterno = "Gomez",
                CMP = "123456",
                IdEspecialidad = 2,
                TiempoAtencionMin = 30,
                Estado = true
            };
            var turno = new Turno
            {
                IdTurno = 1,
                Nombre = "Mañana",
                HoraInicio = new TimeOnly(8, 0),
                HoraFin = new TimeOnly(12, 0),
                Estado = true
            };

            // Sembramos un bloque preexistente de 08:00 a 08:30
            var horarioExistente = new HorarioMedico
            {
                IdHorarioMedico = 10,
                IdMedico = 1,
                IdEspecialidad = 2,
                IdTurno = 1,
                Fecha = new DateOnly(2026, 11, 20),
                HoraInicio = new TimeOnly(8, 0),
                HoraFin = new TimeOnly(8, 30),
                Estado = true
            };

            context.Especialidades.Add(especialidad);
            context.Medicos.Add(medico);
            context.Turnos.Add(turno);
            context.HorariosMedicos.Add(horarioExistente);
            await context.SaveChangesAsync();

            var controller = new HorarioMedicoController(context);
            SetupControllerUserContext(controller);

            // Intentamos crear un rango que incluye la hora 08:00
            var horarioSolapado = new HorarioMedico
            {
                IdMedico = 1,
                IdTurno = 1,
                Fecha = new DateOnly(2026, 11, 20),
                HoraInicio = new TimeOnly(8, 0),
                HoraFin = new TimeOnly(10, 0),
                Estado = true
            };

            // Act (When)
            var result = await controller.Create(horarioSolapado);

            // Assert (Then)
            Assert.NotNull(result);
            // Confirma que devolvió la vista con el error en TempData por horas existentes
            Assert.Equal(1, context.HorariosMedicos.Count()); // No se insertaron nuevos slots
        }
    }
}