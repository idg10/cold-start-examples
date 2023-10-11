#define USE_REFLECTION

using System.Diagnostics;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

// This causes ASP.NET Core to reflect against our delegate on startup to discover our
// input and output types.
#if USE_REFLECTION

app.MapGet("/", () => "Hello World!");

#else

// Process.Start took 00:00:00.0222986
// Response code: OK(200)
// 00:00:00.4233907
// This avoids reflection for that part:
app.MapGet(
    "/",
    async (HttpContext context) =>
    {
        context.Response.ContentType ??= "text/plain; charset=utf-8";
        await context.Response.WriteAsync("Hello World!").ConfigureAwait(false);
    });

#endif

app.Start();
//app.Run();
Console.ReadLine();
await app.StopAsync();
