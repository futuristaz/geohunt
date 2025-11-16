using psi25_project.Models;
using psi25_project.Models.Dtos;
using psi25_project.Repositories.Interfaces;
using psi25_project.Utils; 

namespace psi25_project.Services
{
    public class LocationService : ILocationService
    {
        private readonly ILocationRepository _locationRepository;
        private readonly ObjectValidator<Location> _validator = new ObjectValidator<Location>();

        public LocationService(ILocationRepository locationRepository)
        {
            _locationRepository = locationRepository;
        }

        public async Task<IEnumerable<Location>> GetAllLocationsAsync()
        {
            return await _locationRepository.GetAllAsync();
        }

        public async Task<IEnumerable<object>> GetRecentLocationsAsync()
        {
            var cutoffUtc = DateTime.UtcNow.AddMonths(-6);
            return await _locationRepository.GetRecentAsync(cutoffUtc);
        }

        public async Task<Location> CreateLocationAsync(LocationDto dto)
        {
            var location = _validator.CreateDefault();

            location.Latitude = dto.Latitude;
            location.Longitude = dto.Longitude;
            location.panoId = dto.panoId;
            location.CreatedAt = DateTime.UtcNow;
            location.LastPlayedAt = DateTime.UtcNow;
            location.Guesses = new List<Guess>();

            if (!_validator.ValidatePropertyNotNull(location, l => l.panoId))
                throw new Exception("Location panoId cannot be null!");

            await _locationRepository.AddAsync(location);
            return location;
        }

        public async Task<(bool success, object result, string? message)> UpdateLastPlayedAsync(int id)
        {
            var location = await _locationRepository.GetByIdAsync(id);
            if (location == null)
                return (false, null!, "Location not found");

            location.LastPlayedAt = DateTime.UtcNow;
            await _locationRepository.UpdateAsync(location);

            return (true, new
            {
                message = "LastPlayedAt updated successfully",
                location.Id,
                location.LastPlayedAt
            }, null);
        }
    }
}
