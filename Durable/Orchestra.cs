using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace Durable
{
  public static class Orchestra
  {
    private const string ORCHESTRA = nameof(Orchestra);
    private const string SAYHELLO = nameof(SayHello);
    private const string HTTPSTART = nameof(HttpStart);
    private const string ORCHESTRA_SAYHELLO = ORCHESTRA + "_" + SAYHELLO;
    private const string ORCHESTRA_HTTPSTART = ORCHESTRA + "_" + HTTPSTART;

    [FunctionName(ORCHESTRA)]
    public static async Task<List<string>> RunOrchestrator(
        [OrchestrationTrigger] IDurableOrchestrationContext context)
    {
      var outputs = new List<string>
      {
        // Replace "hello" with the name of your Durable Activity Function.
        await context.CallActivityAsync<string>(ORCHESTRA_SAYHELLO, "Edgar"),
        await context.CallActivityAsync<string>(ORCHESTRA_SAYHELLO, "Richard"),
        await context.CallActivityAsync<string>(ORCHESTRA_SAYHELLO, "Knapp")
      };

      return outputs;
    }

    [FunctionName(ORCHESTRA_SAYHELLO)]
    public static string SayHello([ActivityTrigger] string name, ILogger log)
    {
      log.LogInformation($"Saying hello to {name}.");
      return $"Hello {name}!";
    }

    [FunctionName(ORCHESTRA_HTTPSTART)]
    public static async Task<HttpResponseMessage> HttpStart(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequestMessage req,
        [DurableClient] IDurableOrchestrationClient starter,
        ILogger log)
    {
      // Function input comes from the request content.
      string instanceId = await starter.StartNewAsync(ORCHESTRA, null);

      log.LogInformation($"Started orchestration with ID = '{instanceId}'.");

      return starter.CreateCheckStatusResponse(req, instanceId);
    }
  }
}