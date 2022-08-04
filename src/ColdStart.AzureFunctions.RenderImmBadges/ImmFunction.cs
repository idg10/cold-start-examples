using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Net.Http;
using Endjin.Imm.Contracts;
using Endjin.Imm.Domain;
using Endjin.Imm.Processing;
using Endjin.Badger;
using System.Net;
using System.Text;
using System.Net.Http.Headers;
using Endjin.Imm.Services;

namespace ColdStart.AzureFunctions.RenderImmBadges;

public class ImmFunction
{
    private readonly ImmBadgeService immBadgeService;

    public ImmFunction(
        ImmBadgeService immBadgeService)
    {
        this.immBadgeService = immBadgeService;
    }

    [FunctionName(nameof(GitHubImmTotalScore))]
    public async Task<HttpResponseMessage> GitHubImmTotalScore(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "imm/github/{org}/{project}/total")] HttpRequest request,
        string org,
        string project)
    {
        (string rulesObjectName, string projectObjectName) = GetGitHubBranchOrObjectNames(request);
        string svg = await this.immBadgeService.GitHubImmTotalScore(
            rulesObjectName,
            projectObjectName,
            org,
            project).ConfigureAwait(false);
        return CreateUncacheResponse(
            new ByteArrayContent(Encoding.ASCII.GetBytes(svg)),
            "image/svg+xml");
    }

    private static HttpResponseMessage CreateUncacheResponse(HttpContent content, string mediaType)
    {
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = content
        };
        response.Content.Headers.ContentType = new MediaTypeHeaderValue(mediaType);
        response.Content.Headers.TryAddWithoutValidation("expires", "-1");
        response.Headers.ETag = new EntityTagHeaderValue($"\"{Guid.NewGuid()}\"");
        response.Headers.Pragma.Add(new NameValueHeaderValue("no-cache"));
        response.Headers.CacheControl = new CacheControlHeaderValue()
        {
            NoCache = true,
            NoStore = true,
            Public = false,
            MustRevalidate = true,
        };

        return response;
    }

    private static (string RuleDefinitionsObjectName, string ProjectObjectName) GetGitHubBranchOrObjectNames(
        HttpRequest request)
    {
        var queryParams = request.GetQueryParameterDictionary();
        string definitionsObject = ObjectNameFromQuerystring("definitionsBranch");
        string projectObject = ObjectNameFromQuerystring("projectBranch");
        return (definitionsObject, projectObject);

        string ObjectNameFromQuerystring(string name) =>
            queryParams.TryGetValue(name, out string? value)
                ? value
                : "master";
    }
}
