using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using psi25_project.Data;
using psi25_project.Models.Dtos;

namespace psi25_project.Services
{
    public class LocationService
    {
        private readonly GeoHuntContext _context;

        public LocationService(GeoHuntContext context)
        {
            _context = context;
        }

        public async Task<LocationDto?> GetOldestLocationAsync()
        {
            var result = await _context.Locations
                .OrderBy(l => l.LastPlayedAt)
                .Select(l => new LocationDto
                {
                    Longitude = l.Longitude,
                    Latitude = l.Latitude,
                    panoId = l.panoId,
                })
                .FirstOrDefaultAsync();

            return result;
        }
    }
}