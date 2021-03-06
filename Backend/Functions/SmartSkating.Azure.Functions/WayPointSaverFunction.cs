using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
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
    public class WayPointSaverFunction:IAzureFunction
    {
        private readonly IDataService _dataService;

        private readonly StringBuilder _errorMessageBuilder = new StringBuilder();
        private readonly ISessionInfoHelper _sessionHelper;

        public WayPointSaverFunction(IDataService dataService, ISessionInfoHelper sessionHelper)
        {
            _dataService = dataService;
            _sessionHelper = sessionHelper;
        }

        [FunctionName("WayPointSaverFunction")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function,
                "post", 
                Route = ApiNames.WayPointsResource.Route)] HttpRequest request,
            IBinder binder,
            ILogger logger)
        {
            var responseObject = new SaveEntitiesResponse {SyncedIds = new List<string>()};
            var requestData = await new StreamReader(request.Body).ReadToEndAsync();
            var requestObject = JsonConvert.DeserializeObject<List<WayPointDto>?>(requestData);
            
            if (requestObject == null || requestObject.Count == 0)
            {
                responseObject.ErrorCode = (int)HttpStatusCode.BadRequest;
                _errorMessageBuilder.AppendLine(Constants.BadRequestErrorMessage);
            }
            else
            {
                responseObject.ErrorCode = (int)HttpStatusCode.OK;
                var signalR = await binder
                    .BindAsync<IAsyncCollector<SignalRMessage>>(new SignalRAttribute
                        {HubName = _sessionHelper.GetHubNameForSession(requestObject[0].SessionId) });
                foreach (var wayPoint in requestObject)
                {
                    if (wayPoint.Time.Year < 1601)
                    {
                        _errorMessageBuilder.AppendLine(Constants.DateTimeValidationErrorMessage);
                        continue;
                    }

                    if (_dataService == null || !await _dataService.SaveWayPointAsync(wayPoint)) continue;
                    
                    await signalR.AddAsync(
                        new SignalRMessage
                        {
                            Target = SyncHubMethodNames.AddWaypoint,
                            Arguments = new object[]{wayPoint}
                        });
                    responseObject.SyncedIds.Add(wayPoint.Id);
                }

                if (!string.IsNullOrEmpty(_dataService?.ErrorMessage)) 
                    _errorMessageBuilder.AppendLine(_dataService.ErrorMessage);
            }
            
            responseObject.Message = _errorMessageBuilder.ToString();
            if (responseObject.Message.Contains(Constants.DateTimeValidationErrorMessage)
                && responseObject.SyncedIds.Count == 0)
                responseObject.ErrorCode = (int)HttpStatusCode.BadRequest;
            return new JsonResult(responseObject);
        }
    }
}