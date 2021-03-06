using System;
using System.Collections.Generic;
using System.Linq;
using Sanet.SmartSkating.Dto.Models;

namespace Sanet.SmartSkating.Models.Location
{
    public class BleScansStack
    {
        private const int MinimumRssiValue = -100;
        public string DeviceId { get; }
        public int AverageRssi { get; private set; } = MinimumRssiValue;

        public RssiTrends RssiTrend
        {
            get => _rssiTrend;
            private set
            {
                HasRssiTrendChanged = _rssiTrend != value;
                _rssiTrend = value;
            }
        }

        public bool HasRssiTrendChanged { get; private set; }
        public DateTime Time { get; private set; } = DateTime.MinValue;

        private readonly Queue<BleScanResultDto> _stack = new Queue<BleScanResultDto>();
        private RssiTrends _rssiTrend;

        public BleScansStack(string deviceId)
        {
            DeviceId = deviceId;
        }

        public void AddScan(BleScanResultDto scan)
        {
            if (scan.DeviceAddress!=DeviceId)
                return;

            if (_stack.Count == 2)
                _stack.Dequeue();
            
            _stack.Enqueue(scan);
            Time = scan.Time;
            var prevAverage = AverageRssi;
            AverageRssi = (int)_stack.Average(f => f.Rssi);
            var trend = scan.Rssi - prevAverage;
            if (trend > 0)
                RssiTrend = RssiTrends.Increase;
            else if (trend < 0)
                RssiTrend = RssiTrends.Decrease;
            else
                RssiTrend = RssiTrends.Same;
        }
    }
}