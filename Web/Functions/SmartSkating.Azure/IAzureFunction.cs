using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Sanet.SmartSkating.Dto.Services;

namespace Sanet.SmartSkating.Azure
{
    public interface IAzureFunction
    {
        void SetService(IDataService dataService);

        Task<IActionResult> Run(HttpRequest request, ILogger log);
    }
}