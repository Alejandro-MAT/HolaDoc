using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using WebConsultasMedicas.Data;
using WebConsultasMedicas.Hubs;

var builder = WebApplication.CreateBuilder(args);

// Acá matriculamos todos los servicios que va a usar la app (Inyección de dependencias).
builder.Services.AddControllersWithViews(options =>
{
    options.ModelBinderProviders.Insert(0, new WebConsultasMedicas.ModelBinders.DateOnlyModelBinderProvider());
    options.ModelBinderProviders.Insert(0, new WebConsultasMedicas.ModelBinders.TimeOnlyModelBinderProvider());
});
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Auth/Login";
        options.AccessDeniedPath = "/Auth/Denied";
        options.Cookie.Name = "SISCitasWeb.Auth";
        options.SlidingExpiration = true;
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
    });

builder.Services.AddAuthorization();
builder.Services.AddSignalR();

var app = builder.Build();

var supportedCultures = new[] { "es-PE" };
var localizationOptions = new RequestLocalizationOptions()
    .SetDefaultCulture(supportedCultures[0])
    .AddSupportedCultures(supportedCultures)
    .AddSupportedUICultures(supportedCultures);
app.UseRequestLocalization(localizationOptions);

await DbSeeder.SeedAsync(app.Services);

// Aquí configuramos cómo la app va a responder a las peticiones (el famoso pipeline).
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // En producción hay que asegurar la red con HSTS por 30 días, pa' que nadie se pase de vivo. Si lo mandas a producción real, checa la docu en: https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Auth}/{action=Login}/{id?}");

app.MapHub<CitasHub>("/hubs/citas");

app.Run();
