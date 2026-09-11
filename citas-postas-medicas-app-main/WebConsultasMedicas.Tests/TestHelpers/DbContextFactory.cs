using Microsoft.EntityFrameworkCore;
using WebConsultasMedicas.Data;

namespace WebConsultasMedicas.Tests.TestHelpers;

/// <summary>
/// Crea un ApplicationDbContext respaldado por la base de datos en memoria de EF Core.
/// Cada prueba recibe una base con un nombre único (Guid) para que no se contaminen
/// entre sí, y así no dependemos de tener SQL Server corriendo para probar la lógica.
/// </summary>
public static class DbContextFactory
{
    public static ApplicationDbContext CreateInMemoryContext(string? dbName = null)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: dbName ?? Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }
}
