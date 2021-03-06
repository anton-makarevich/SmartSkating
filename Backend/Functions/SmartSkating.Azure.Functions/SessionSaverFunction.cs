using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.SignalRService;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Sanet.SmartSkating.Backend.Azure;
using Sanet.SmartSkating.Dto;
using Sanet.SmartSkating.Dto.Models;
using Sanet.SmartSkating.Dto.Models.Responses;
using Sanet.SmartSkating.Dto.Services;

namespace Sanet.SmartSkating.Backend.Functions
{
    public class SessionSaverFunction : IAzureFunction
    {
        private readonly IDataService _dataService;
        private readonly ISessionInfoHelper _sessionHelper;

        public SessionSaverFunction(IDataService dataService, ISessionInfoHelper sessionHelper)
        {
            _dataService = dataService;
            _sessionHelper = sessionHelper;
        }

        [FunctionName("SessionSaverFunction")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, 
                "post",
                Route = ApiNames.SessionsResource.Route)] HttpRequest request,
            IBinder binder,
            ILogger logger)
        {
            var responseObject = new SaveEntitiesResponse {SyncedIds = new List<string>()};
            var requestData = await new StreamReader(request.Body).ReadToEndAsync();
            var requestObject = JsonConvert.DeserializeObject<List<SessionDto>?>(requestData);
            
            if (requestObject == null || requestObject.Count==0)
            {
                responseObject.ErrorCode = (int)HttpStatusCode.BadRequest;
                responseObject.Message = "Invalid request data";
            }
            else
            {
                responseObject.ErrorCode = (int)HttpStatusCode.OK;
                foreach (var session in requestObject)
                {
                    if (_dataService == null || !await _dataService.SaveSessionAsync(session)) continue;
                    if (session.IsCompleted)
                    {
                        var signalR = await binder
                                            .BindAsync<IAsyncCollector<SignalRMessage>>(new SignalRAttribute
                                                {HubName = _sessionHelper.GetHubNameForSession(session.Id) });
                        await signalR.AddAsync(
                            new SignalRMessage
                            {
                                Target = SyncHubMethodNames.EndSession,
                                Arguments = new object[] {session}
                            });
                    }

                    responseObject.SyncedIds.Add(session.Id);
                }

                if (_dataService != null) responseObject.Message = _dataService.ErrorMessage;
            }
            return new JsonResult(responseObject);
        }
    }
}