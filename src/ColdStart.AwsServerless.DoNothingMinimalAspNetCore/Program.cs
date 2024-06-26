using Amazon.Lambda.Serialization.SystemTextJson;

using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// The AWS template includes this, but we're not going to use MVC, because that's a world of extra startup pain.
//// Add services to the container.
builder.Services.AddControllers();

// Add AWS Lambda support. When application is run in Lambda Kestrel is swapped out as the web server with Amazon.Lambda.AspNetCoreServer. This
// package will act as the webserver translating request and responses between the Lambda event source and ASP.NET Core.
#if AOT
builder.Services.AddAWSLambdaHosting(LambdaEventSource.RestApi, new SourceGeneratorLambdaJsonSerializer<LambdaFunctionJsonSerializerContext>());
#else
builder.Services.AddAWSLambdaHosting(LambdaEventSource.RestApi);
#endif

var app = builder.Build();


app.UseHttpsRedirection();
//app.UseAuthorization();

// See earlier comment on not using MVC
app.MapControllers();

// AWS template does this (which is pretty much also what the ASP.NET Core minimal template does), but
// it forces ASP.NET Core to load the MethodInfo for this delegate.
app.MapGet("/", () => "Welcome to running ASP.NET Core Minimal API on AWS Lambda");
//
// This table shows memory usage for the various options:
//
// | Mechanism                       | JIT  | R2R  | Native AOT  |
// |---------------------------------|------|------|-------------|
// | RequestDelegate (no reflection) | 83MB | 82MB | 83MB        |
// | Using reflection                | 84MB | 83MB | N/A         |
// | EnableRequestDelegateGenerator  | 83MB | 82MB | 83MB        |
// | MVC controllers enabled         | 91MB | 90MB | Unsupported |
//
// JIT:
//
// RequestDelegate (no reflection):
// Duration: 1910.60 ms	Billed Duration: 1911 ms	Memory Size: 128 MB	Max Memory Used: 83 MB	Init Duration: 341.95 ms
// Duration: 1851.83 ms	Billed Duration: 1852 ms	Memory Size: 128 MB	Max Memory Used: 83 MB	Init Duration: 339.86 ms
// Reflection:
// Duration: 2252.58 ms	Billed Duration: 2253 ms	Memory Size: 128 MB	Max Memory Used: 84 MB	Init Duration: 351.18 ms
// Duration: 2210.29 ms	Billed Duration: 2211 ms	Memory Size: 128 MB	Max Memory Used: 84 MB	Init Duration: 335.57 ms
// EnableRequestDelegateGenerator
// Duration: 1871.56 ms	Billed Duration: 1872 ms	Memory Size: 128 MB	Max Memory Used: 83 MB	Init Duration: 375.35 ms
// MVC:
// Duration: 1854.30 ms	Billed Duration: 1855 ms	Memory Size: 128 MB	Max Memory Used: 91 MB	Init Duration: 572.49 ms
// Duration: 1802.29 ms	Billed Duration: 1803 ms	Memory Size: 128 MB	Max Memory Used: 90 MB	Init Duration: 533.87 ms
//
// R2R:
// RequestDelegate (no reflection):
// Duration: 1554.96 ms	Billed Duration: 1555 ms	Memory Size: 128 MB	Max Memory Used: 82 MB	Init Duration: 325.96 ms
// Duration: 1612.69 ms	Billed Duration: 1613 ms	Memory Size: 128 MB	Max Memory Used: 82 MB	Init Duration: 341.00 ms
// Reflection:
// Duration: 1853.02 ms	Billed Duration: 1854 ms	Memory Size: 128 MB	Max Memory Used: 84 MB	Init Duration: 315.48 ms
// Duration: 1974.64 ms	Billed Duration: 1975 ms	Memory Size: 128 MB	Max Memory Used: 83 MB	Init Duration: 342.11 ms
// EnableRequestDelegateGenerator
// Duration: 1545.82 ms	Billed Duration: 1546 ms	Memory Size: 128 MB	Max Memory Used: 82 MB	Init Duration: 361.66 ms
// MVC:
// Duration: 1460.54 ms	Billed Duration: 1461 ms	Memory Size: 128 MB	Max Memory Used: 90 MB	Init Duration: 447.64 ms
// Duration: 1557.64 ms	Billed Duration: 1558 ms	Memory Size: 128 MB	Max Memory Used: 90 MB	Init Duration: 466.42 ms
//
// AOT:
// RequestDelegate (no reflection):
// Duration: 1593.54 ms	Billed Duration: 1594 ms	Memory Size: 128 MB	Max Memory Used: 82 MB	Init Duration: 418.83 ms
// Duration: 1900.34 ms	Billed Duration: 1901 ms	Memory Size: 128 MB	Max Memory Used: 83 MB	Init Duration: 361.59 ms
// EnableRequestDelegateGenerator (always enabled for Native AOT):
// Duration: 1577.10 ms	Billed Duration: 1578 ms	Memory Size: 128 MB	Max Memory Used: 82 MB	Init Duration: 312.28 ms
// Duration: 1900.35 ms	Billed Duration: 1901 ms	Memory Size: 128 MB	Max Memory Used: 83 MB	Init Duration: 353.42 ms
// MVC:
// Not supported



//app.MapGet(
//    "/",
//    async (context) =>
//    {
//        context.Response.ContentType ??= "text/plain; charset=utf-8";
//        await context.Response.WriteAsync("Welcome to running ASP.NET Core Minimal API on AWS Lambda").ConfigureAwait(false);
//    });

//app.Run();


#if AOT

[JsonSerializable(typeof(string))]
public partial class LambdaFunctionJsonSerializerContext : JsonSerializerContext
{
    // By using this partial class derived from JsonSerializerContext, we can generate reflection free JSON Serializer code at compile time
    // which can deserialize our class and properties. However, we must attribute this class to tell it what types to generate serialization code for.
    // See https://docs.microsoft.com/en-us/dotnet/standard/serialization/system-text-json-source-generation
}
#endif