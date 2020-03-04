using System;
using FluentAssertions;
using Sanet.SmartSkating.Backend.Azure.Models;
using Sanet.SmartSkating.Dto.Models;
using Xunit;

namespace Sanet.SmartSkating.Backend.Azure.Tests.Models
{
    public class WayPointEntityTests
    {
        [Fact]
        public void CanBeCreatedFromDto()
        {
            var dto = new WayPointDto
            {
                Id = "id",
                SessionId = "sessionId",
                Coordinate = new CoordinateDto
                {
                    Latitude = 1.2,
                    Longitude = 3.2
                }, 
                Time = DateTime.Now
            };
            var sut = new WayPointEntity(dto);

            sut.PartitionKey.Should().Be(dto.SessionId);
            sut.RowKey.Should().Be(dto.Id);
            sut.Latitude.Should().Be(dto.Coordinate.Latitude);
            sut.Longitude.Should().Be(dto.Coordinate.Longitude);
            sut.Time.Should().Be(dto.Time);
        }
    }
}