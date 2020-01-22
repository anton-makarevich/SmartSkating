using System;
using System.Threading.Tasks;
using Sanet.SmartSkating.Models.EventArgs;

namespace Sanet.SmartSkating.Services.Location
{
    public interface IBleLocationService
    {
        event EventHandler<CheckPointEventArgs>? CheckPointPassed;
        void StartBleScan();
        void StopBleScan();
        Task LoadDevicesDataAsync();
    }
}