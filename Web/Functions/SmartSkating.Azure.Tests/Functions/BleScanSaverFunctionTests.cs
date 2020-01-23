using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FunctionTestUtils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Sanet.SmartSkating.Dto.Models;
using Sanet.SmartSkating.Dto.Models.Responses;
using Sanet.SmartSkating.Dto.Services;
using Sanet.SmartSkating.Web.Functions;
using Xunit;

namespace Sanet.SmartSkating.Azure.Tests.Functions
{
    public class BleScanSaverFunctionTests
    {
        private readonly BleScanSaverFunction _sut;
        private readonly IDataService _dataService;
        private readonly List<BleScanResultDto> _scansStub = new List<BleScanResultDto>()
        {
            new BleScanResultDto
            {
                Id = "0",
                SessionId = "0",
                DeviceAddress = "6",
                Rssi = -87
            },
            new BleScanResultDto
            {
                Id = "1",
                SessionId = "0",
                DeviceAddress = "6",
                Rssi = -87
            }
        };

        public BleScanSaverFunctionTests()
        {
            _dataService = Substitute.For<IDataService>();
            _sut = new BleScanSaverFunction();
            _sut.SetService(_dataService);
        }

        [Fact]
        public async Task RunningFunctionCallsSaveBleScanForEveryItem()
        {
            await _sut.Run(Utils.CreateMockRequest(
                    _scansStub),
                Substitute.For<ILogger>());

            await _dataService.Received(2).SaveBleScanAsync(Arg.Any<BleScanResultDto>());
        }

        [Fact]
        public async Task RunningFunctionReturnsListOfSavedBleScsnsIds()
        {
            _dataService.SaveBleScanAsync(Arg.Any<BleScanResultDto>())
                .ReturnsForAnyArgs(Task.FromResult(true));
        
            var actionResult = await _sut.Run(Utils.CreateMockRequest(
                    _scansStub),
                Substitute.For<ILogger>()) as JsonResult;
        
            Assert.NotNull(actionResult);
            var response = actionResult.Value as SaveEntitiesResponse;
        
            Assert.NotNull(response?.SyncedIds);
            Assert.Equal(200, response.ErrorCode);
            Assert.Equal(2, response.SyncedIds.Count);
            Assert.Equal(_scansStub.First().Id,response.SyncedIds.First());
            Assert.Equal(_scansStub.Last().Id,response.SyncedIds.Last());
        }
        
        [Fact]
        public async Task ReturnsServiceErrorMessage_WhenSavingIsUnsuccessful()
        {
            const string errorMessage = "some error";
            _dataService.ErrorMessage.Returns(errorMessage);
            _dataService.SaveBleScanAsync(Arg.Any<BleScanResultDto>())
                .ReturnsForAnyArgs(Task.FromResult(false));
        
            var actionResult = await _sut.Run(Utils.CreateMockRequest(
                    _scansStub),
                Substitute.For<ILogger>()) as JsonResult;
        
            Assert.NotNull(actionResult);
            var response = actionResult.Value as SaveEntitiesResponse;
            Assert.NotNull(response);
            Assert.Equal(errorMessage, response.Message);
        }
        
        [Fact]
        public async Task RunningFunctionWithoutProperRequestReturnsBadRequestErrorCode()
        {
            await CommonFunctionsTests.RunningFunctionWithoutProperRequestReturnsBadRequestErrorCode(_sut);
        }
    }
}