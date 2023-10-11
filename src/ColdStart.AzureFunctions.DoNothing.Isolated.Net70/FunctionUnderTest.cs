using System.Net;

using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;

namespace ColdStart.AzureFunctions.DoNothing.Isolated.Net70
{
    public static class FunctionUnderTest
    {
        [Function("DoNothingNoDi")]
        public static HttpResponseData Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequestData req)
        {
            var response = req.CreateResponse(HttpStatusCode.OK);
            response.Headers.Add("Content-Type", "text/plain; charset=utf-8");

            response.WriteString($"{req.Url.AbsolutePath}: Well that went well.");

            return response;
        }
    }
}
