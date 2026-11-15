using Microsoft.EntityFrameworkCore;
using SportsEvents.Mvc.Data;
using SportsEvents.Mvc.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<SportsEventsDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("SportsEvents")
        ?? throw new InvalidOperationException("ConnectionStrings:SportsEvents ayarını tanımlayın.")));
builder.Services.AddScoped<IEventService, EventService>();

var app = builder.Build();
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Events/Error");
    app.UseHsts();
    app.UseHttpsRedirection();
}

app.Use(async (context, next) =>
{
    context.Response.Headers["X-Content-Type-Options"] = "nosniff";
    context.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
    context.Response.Headers["Content-Security-Policy"] =
        "default-src 'self'; script-src 'self'; style-src 'self'; object-src 'none'; base-uri 'self'; frame-ancestors 'none'; form-action 'self'";
    await next();
});

app.UseStaticFiles();
app.MapControllers();
app.MapControllerRoute("default", "{controller=Events}/{action=Index}/{id?}");
app.Run();
