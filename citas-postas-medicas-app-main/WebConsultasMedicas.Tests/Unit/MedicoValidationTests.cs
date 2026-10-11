using System.ComponentModel.DataAnnotations;
using WebConsultasMedicas.Models;
using Xunit;

namespace WebConsultasMedicas.Tests.Unit
{
    public class MedicoValidationTests
    {
        private IList<ValidationResult> ValidateModel(object model)
        {
            var validationResults = new List<ValidationResult>();
            var ctx = new ValidationContext(model, null, null);
            Validator.TryValidateObject(model, ctx, validationResults, true);
            return validationResults;
        }

        [Fact]
        public void Medico_CaminoFeliz_ModeloEsValido()
        {
            // Arrange
            var medico = new Medico
            {
                IdMedico = 1,
                IdUsuario = 5,
                CMP = "123456",
                Nombres = "Carlos",
                ApellidoPaterno = "Mendoza",
                ApellidoMaterno = "Salazar",
                IdEspecialidad = 1,
                Estado = true
            };

            // Act
            var results = ValidateModel(medico);

            // Assert
            Assert.Empty(results);
        }

        [Fact]
        public void Medico_CA19_SinCMP_DevuelveErrorDeValidacion()
        {
            // Arrange
            var medico = new Medico
            {
                IdMedico = 2,
                IdUsuario = 6,
                CMP = "", // CMP vacío
                Nombres = "Ana",
                ApellidoPaterno = "García",
                ApellidoMaterno = "Rios",
                IdEspecialidad = 1,
                Estado = true
            };

            // Act
            var results = ValidateModel(medico);

            // Assert
            Assert.NotEmpty(results);
        }
    }
}