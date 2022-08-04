var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

// This causes ASP.NET Core to reflect against our delegate on startup to discover our
// input and output types.
//app.MapGet("/", () => "Hello World!");

// This avoids reflection for that part:
app.MapGet(
    "/",
    async (HttpContext context) =>
    {
        context.Response.ContentType ??= "text/plain; charset=utf-8";
        await context.Response.WriteAsync("Hello World!").ConfigureAwait(false);
    });


app.Start();
//app.Run();
Console.ReadLine();
await app.StopAsync();
