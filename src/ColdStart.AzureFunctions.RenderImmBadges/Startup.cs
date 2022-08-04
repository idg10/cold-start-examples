using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

[assembly: FunctionsStartup(typeof(ColdStart.AzureFunctions.RenderImmBadges.Startup))]

namespace ColdStart.AzureFunctions.RenderImmBadges;

public class Startup : FunctionsStartup
{
    public override void Configure(IFunctionsHostBuilder builder)
    {
        builder.Services.AddImmServices();
    }
}