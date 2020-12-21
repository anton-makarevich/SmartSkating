using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using SmartSkating.Dashboard.Blazor.Services;

namespace SmartSkating.Dashboard.Blazor
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("app");
            builder.Services.AddAuthorizationCore();
            builder.Services.AddScoped<AuthorizationProvider>();
            builder.Services.AddScoped<AuthenticationStateProvider>(
                s => s.GetRequiredService<AuthorizationProvider>());

            builder.Services.AddTransient(
                sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

            await builder.Build().RunAsync();
        }
    }
}
