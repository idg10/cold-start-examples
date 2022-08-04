using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;

namespace ColdStart.AzureFunctions.DoNothingNoDi;

public static class FunctionUnderTest
{
    [FunctionName("DoNothingNoDi")]
    public static Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req)
    {
        return Task.FromResult<IActionResult>(new OkObjectResult($"{req.Path.Value}: Well that went well."));
    }
}