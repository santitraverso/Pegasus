using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();

// Configura servicios de autenticación
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = GoogleDefaults.AuthenticationScheme;
})
.AddCookie()
.AddGoogle(options =>
{
    options.ClientId = "579011975819-tsmscfrfs7p4ai2n72rm5g9isvfqtubo.apps.googleusercontent.com";
    options.ClientSecret = "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";
    options.CallbackPath = "/auth/callback"; // Este es solo para redirigir dentro del app, no se debe usar en Google
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}



app.UseAuthentication();
app.UseAuthorization();

// Maneja la autenticación en la ruta principal
app.MapGet("/auth/callback", async context =>
{
    var result = await context.AuthenticateAsync();
    if (result.Succeeded)
    {
        // Aquí puedes redirigir al usuario a la página de inicio o guardar la sesión
        context.Response.Redirect("/"); // Cambia a la ruta principal de tu frontend
    }
    else
    {
        // Maneja errores
        context.Response.StatusCode = 500;
    }
});

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.MapRazorPages();

app.Run();
