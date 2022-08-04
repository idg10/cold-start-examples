var builder = WebApplication.CreateBuilder(args);

// The AWS template includes this, but we're not going to use MVC, because that's a world of extra startup pain.
//// Add services to the container.
//builder.Services.AddControllers();

// Add AWS Lambda support. When application is run in Lambda Kestrel is swapped out as the web server with Amazon.Lambda.AspNetCoreServer. This
// package will act as the webserver translating request and responses between the Lambda event source and ASP.NET Core.
builder.Services.AddAWSLambdaHosting(LambdaEventSource.RestApi);

var app = builder.Build();


app.UseHttpsRedirection();
//app.UseAuthorization();

// See earlier comment on not using MVC
//app.MapControllers();

// AWS template does this (which is pretty much also what the ASP.NET Core minimal template does), but
// it forces ASP.NET Core to load the MethodInfo for this delegate.
//app.MapGet("/", () => "Welcome to running ASP.NET Core Minimal API on AWS Lambda");
//
// This table shows memory usage for the various options:
//
// | Mechanism                       | R2R  | JIT  |
// |---------------------------------|------|------|
// | RequestDelegate (no reflection) | 72MB | 76MB |
// | Using reflection                | 76MB | 78MB
// | MVC controllers enabled         | 82MB | 85MB |
//

app.MapGet(
    "/",
    async (HttpContext context) =>
    {
        context.Response.ContentType ??= "text/plain; charset=utf-8";
        await context.Response.WriteAsync("Welcome to running ASP.NET Core Minimal API on AWS Lambda").ConfigureAwait(false);
    });

app.Run();
