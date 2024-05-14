using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;

using System.Net;

namespace ColdStart.AzureFunctions.DoNothing.Isolated.Net80
{
    public static class FunctionUnderTest
    {
        [Function("DoNothingNoDi")]
        public static async Task<HttpResponseData> RunAsync([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequestData req)
        {
            var response = req.CreateResponse(HttpStatusCode.OK);
            response.Headers.Add("Content-Type", "text/plain; charset=utf-8");

            await response.WriteStringAsync($"{req.Url.AbsolutePath}: Well that went well.");

            return response;
        }
    }
}
