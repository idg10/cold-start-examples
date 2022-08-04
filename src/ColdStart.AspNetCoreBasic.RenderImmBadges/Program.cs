using Endjin.Imm.Services;

using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddImmServices();
var app = builder.Build();

app.MapGet(
    "/imm/github/{org}/{project}/total",
    async (
        [FromServices] ImmBadgeService immBadgeService,
        [FromRoute] string org,
        [FromRoute] string project,
        [FromQuery] string? definitionsBranch,
        [FromQuery] string? projectBranch,
        HttpContext context) =>
    {
        string svg = await immBadgeService.GitHubImmTotalScore(
            definitionsBranch ?? "master",
            projectBranch ?? "main",
            org,
            project).ConfigureAwait(false);
        context.Response.Headers.ETag = $"\"{Guid.NewGuid()}\"";
        context.Response.Headers.Expires = "-1";
        context.Response.Headers.Pragma = "no-cache";
        return Results.Text(svg, "image/svg+xml");
    });

app.Start();
//app.Run();
Console.ReadLine();
await app.StopAsync();
