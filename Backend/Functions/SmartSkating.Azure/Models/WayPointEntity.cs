using System;
using Microsoft.WindowsAzure.Storage.Table;
using Sanet.SmartSkating.Dto.Models;

namespace Sanet.SmartSkating.Backend.Azure.Models
{
    public class WayPointEntity:TableEntity
    {
        public WayPointEntity()
        {
        }

        public WayPointEntity(WayPointDto wayPoint)
        {
            PartitionKey = wayPoint.SessionId;
            RowKey = wayPoint.Id;

            Latitude = wayPoint.Coordinate.Latitude;
            Longitude = wayPoint.Coordinate.Longitude;
            Time = wayPoint.Time;
            DeviceId = wayPoint.DeviceId;
        }

        public double Longitude { get; set; }

        public double Latitude { get; set; }

        public DateTime Time { get; set; }

        public string DeviceId { get; set; } = "";
        
        public WayPointDto ToDto()
        {
            return new WayPointDto
            {
                Id = RowKey,
                SessionId = PartitionKey,
                DeviceId = DeviceId,
                Time = Time,
                Coordinate = new CoordinateDto()
                {
                    Latitude = Latitude,
                    Longitude = Longitude
                }
            };
        }
    }
}
