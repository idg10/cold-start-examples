using Endjin.Imm.Services;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ImmServicesServiceCollectionExtensions
    {
        public static IServiceCollection AddImmServices(this IServiceCollection services)
        {
            return services
                .AddImmSources()
                .AddHttpClient()
                .AddMemoryCache()
                .AddSingleton<ImmBadgeService>();
        }
    }
}
