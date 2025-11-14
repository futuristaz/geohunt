using Xunit;
using Moq;
using psi25_project.Services;
using psi25_project.Models;
using psi25_project.Models.Dtos;
using psi25_project.Repositories.Interfaces;

namespace Geohunt.Tests.Services
{
    public class LocationServiceTests
    {
        private readonly Mock<ILocationRepository> _mockRepository;
        private readonly LocationService _service;

        public LocationServiceTests()
        {
            _mockRepository = new Mock<ILocationRepository>();
            _service = new LocationService(_mockRepository.Object);
        }

        [Fact]
        public async Task GetAllLocationsAsync_ReturnsAllLocations()
        {
            // Arrange
            var expectedLocations = new List<Location>
            {
                new Location { Id = 1, Latitude = 54.9, Longitude = 23.9 },
                new Location { Id = 2, Latitude = 55.0, Longitude = 24.0 }
            };
            _mockRepository.Setup(r => r.GetAllAsync())
                .ReturnsAsync(expectedLocations);

            // Act
            var result = await _service.GetAllLocationsAsync();

            // Assert
            Assert.Equal(2, result.Count());
            _mockRepository.Verify(r => r.GetAllAsync(), Times.Once);
        }

        [Fact]
        public async Task CreateLocationAsync_CreatesLocationWithCorrectData()
        {
            // Arrange
            var dto = new LocationDto
            {
                Latitude = 54.9,
                Longitude = 23.9,
                panoId = "test-pano-123"
            };

            Location capturedLocation = null;
            _mockRepository.Setup(r => r.AddAsync(It.IsAny<Location>()))
                .Callback<Location>(l => capturedLocation = l)
                .Returns(Task.CompletedTask);

            // Act
            var result = await _service.CreateLocationAsync(dto);

            // Assert
            Assert.NotNull(capturedLocation);
            Assert.Equal(dto.Latitude, capturedLocation.Latitude);
            Assert.Equal(dto.Longitude, capturedLocation.Longitude);
            Assert.Equal(dto.panoId, capturedLocation.panoId);
            Assert.NotEqual(default(DateTime), capturedLocation.CreatedAt);
            Assert.NotEqual(default(DateTime), capturedLocation.LastPlayedAt);
            _mockRepository.Verify(r => r.AddAsync(It.IsAny<Location>()), Times.Once);
        }

        [Fact]
        public async Task UpdateLastPlayedAsync_LocationExists_UpdatesAndReturnsSuccess()
        {
            // Arrange
            var locationId = 1;
            var existingLocation = new Location
            {
                Id = locationId,
                Latitude = 54.9,
                Longitude = 23.9,
                LastPlayedAt = DateTime.UtcNow.AddDays(-1)
            };

            _mockRepository.Setup(r => r.GetByIdAsync(locationId))
                .ReturnsAsync(existingLocation);
            _mockRepository.Setup(r => r.UpdateAsync(It.IsAny<Location>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _service.UpdateLastPlayedAsync(locationId);

            // Assert
            Assert.True(result.success);
            Assert.Null(result.message);
            Assert.NotNull(result.result);
            _mockRepository.Verify(r => r.UpdateAsync(existingLocation), Times.Once);
        }

        [Fact]
        public async Task UpdateLastPlayedAsync_LocationNotFound_ReturnsFailure()
        {
            // Arrange
            var locationId = 999;
            _mockRepository.Setup(r => r.GetByIdAsync(locationId))
                .ReturnsAsync((Location)null);

            // Act
            var result = await _service.UpdateLastPlayedAsync(locationId);

            // Assert
            Assert.False(result.success);
            Assert.Equal("Location not found", result.message);
            _mockRepository.Verify(r => r.UpdateAsync(It.IsAny<Location>()), Times.Never);
        }

        [Fact]
        public async Task GetRecentLocationsAsync_CallsRepositoryWithCorrectCutoff()
        {
            // Arrange
            var expectedLocations = new List<object>
            {
                new { Id = 1, Latitude = 54.9, Longitude = 23.9 }
            };
            
            _mockRepository.Setup(r => r.GetRecentAsync(It.IsAny<DateTime>()))
                .ReturnsAsync(expectedLocations);

            // Act
            var result = await _service.GetRecentLocationsAsync();

            // Assert
            Assert.Single(result);
            _mockRepository.Verify(r => r.GetRecentAsync(
                It.Is<DateTime>(d => d < DateTime.UtcNow && d > DateTime.UtcNow.AddMonths(-7))
            ), Times.Once);
        }
    }
}