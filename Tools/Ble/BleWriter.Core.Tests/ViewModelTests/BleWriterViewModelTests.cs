using System.Threading.Tasks;
using BleWriter.Services;
using BleWriter.ViewModels;
using FluentAssertions;
using NSubstitute;
using Sanet.SmartSkating.Dto.Models;
using Sanet.SmartSkating.Models.EventArgs;
using Xunit;

namespace BleWriter.Core.Tests.ViewModelTests
{
    public class BleWriterViewModelTests
    {
        private readonly BleWriterViewModel _sut;
        private readonly IBleWriterService _bleWriterService;

        public BleWriterViewModelTests()
        {
            _bleWriterService = Substitute.For<IBleWriterService>();
            _sut = new BleWriterViewModel(_bleWriterService);
        }
        
        [Fact]
        public void StartScanForBleDevices_WhenPageIsLoaded()
        {
            _sut.AttachHandlers();
            
            _bleWriterService.Received().StartBleScan();
        }

        [Fact]
        public void StopsBleScan_WhenStopCommandIsExecuted()
        {
            _sut.AttachHandlers();
            _sut.StopScanCommand.Execute(null);
            
            _bleWriterService.Received().StopBleScan();
        }

        [Fact]
        public void StopsScan_WhenFourDevicesAreReceived()
        {
            _sut.AttachHandlers();
            
            for (var i = 0; i<4; i++)
                _bleWriterService.NewBleDeviceFound += Raise.EventWith(new BleDeviceEventArgs(new BleDeviceDto()));
            
            _bleWriterService.Received().StopBleScan();
        }

        [Fact]
        public void AddsBleDeviceToCollection_WhenReceivesItFromService()
        {
            _sut.AttachHandlers();
            
            _bleWriterService.NewBleDeviceFound += Raise.EventWith(new BleDeviceEventArgs(new BleDeviceDto()));

            _sut.BleDevices.Count.Should().Be(1);
        }

        [Fact]
        public async Task PassesEveryDeviceToWriterService_WhenWriteIdsCommandIsExecuted()
        {
            var deviceStub = new BleDeviceDto();
            _sut.AttachHandlers();
            _bleWriterService.NewBleDeviceFound += Raise.EventWith(new BleDeviceEventArgs(deviceStub));
            _sut.StopScanCommand.Execute(null);
            
            _sut.WriteIdsCommand.Execute(null);

            await _bleWriterService.Received().WriteDeviceIdAsync(deviceStub);
        }

        [Fact]
        public void CannotWrite_WhenScanning()
        {
            _sut.AttachHandlers();

            _sut.CanWrite.Should().BeFalse();
        }
        
        [Fact]
        public void CannotWrite_WhenScanningIsStoppedButNoDevicesAreFound()
        {
            _sut.AttachHandlers();
            _sut.StopScanCommand.Execute(null);

            _sut.CanWrite.Should().BeFalse();
        }
        
        [Fact]
        public void CanWrite_WhenScanningIsStoppedAndDeviceIsFound()
        {
            _sut.AttachHandlers();
            _bleWriterService.NewBleDeviceFound += Raise.EventWith(new BleDeviceEventArgs(new BleDeviceDto()));
            _sut.StopScanCommand.Execute(null);

            _sut.CanWrite.Should().BeTrue();
        }

        [Fact]
        public void CannotStopScan_WhenScanIsNotStarted()
        {
            _sut.CanStopScanning.Should().BeFalse();
        }

        [Fact]
        public void CanStopScan_WhenScanIsStarted()
        {
            _sut.AttachHandlers();

            _sut.CanStopScanning.Should().BeTrue();
        }

        [Fact]
        public void CanNotStopScan_WhenAlreadyStopped()
        {
            _sut.AttachHandlers();
            _sut.StopScanCommand.Execute(true);

            _sut.CanStopScanning.Should().BeFalse();
        }
    }
}