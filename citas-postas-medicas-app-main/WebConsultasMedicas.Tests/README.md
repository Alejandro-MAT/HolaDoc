# Pruebas automatizadas — WebConsultasMedicas.Tests

Proyecto de pruebas con **xUnit** para el sistema SISCitasWeb / WebConsultasMedicas.

## Qué contiene

| Carpeta | Contenido | Tipo de prueba |
|---|---|---|
| `Unit/PasswordHasherTests.cs` | Hash SHA-256 de contraseñas | Unitaria pura |
| `Unit/PacienteValidationTests.cs` | Data Annotations del modelo `Paciente` (DNI, Sexo, etc.) | Unitaria — equivalencia y valores límite |
| `Unit/UsuarioValidationTests.cs` | Data Annotations del modelo `Usuario` (correo, clave) | Unitaria — equivalencia y valores límite |
| `Integration/AuthControllerLoginTests.cs` | Login: credenciales válidas/inválidas, usuario inactivo, redirección por rol | Integración (controlador + BD en memoria) |
| `Integration/AuthControllerRegisterTests.cs` | Registro: DNI/correo duplicado, rol faltante, caso exitoso | Integración (controlador + BD en memoria) |

Las pruebas de integración usan el proveedor **InMemory** de Entity Framework Core, así que **no necesitas tu SQL Server corriendo** para ejecutarlas — cada prueba crea su propia base de datos aislada en memoria.

## Cómo integrarlo a tu solución

1. Copia la carpeta `WebConsultasMedicas.Tests` al mismo nivel que la carpeta `WebConsultasMedicas` (junto al `.sln`).
2. Agrega el proyecto a tu solución. Desde la carpeta raíz (donde está el `.sln`):
   ```bash
   dotnet sln WebConsultasMedicas.sln add WebConsultasMedicas.Tests/WebConsultasMedicas.Tests.csproj
   ```
   O en Visual Studio: clic derecho en la solución → **Agregar → Proyecto existente...** → selecciona el `.csproj` de `WebConsultasMedicas.Tests`.
3. Restaura los paquetes NuGet:
   ```bash
   dotnet restore
   ```
4. Corre las pruebas:
   ```bash
   dotnet test
   ```
   O en Visual Studio: **Ver → Explorador de pruebas (Test Explorer)** → botón "Ejecutar todas".

## Notas importantes

- **No pude compilar ni ejecutar estas pruebas yo mismo** (el entorno donde las generé no tiene el SDK de .NET ni acceso a internet para restaurar paquetes). Es muy probable que necesites ajustar algún detalle menor de sintaxis o nombres si algo no coincide exactamente con una versión distinta de tu código.
- Si `dotnet test` marca errores de compilación, copia el mensaje de error completo y lo revisamos juntos.
- Estas pruebas cubren una **muestra representativa** del sistema (autenticación, registro, validación de modelos), no el 100% de los controladores. Si tu profesor pide cobertura de otros módulos específicos (citas, horarios médicos, etc.), dímelo y seguimos ampliando con el mismo patrón.
