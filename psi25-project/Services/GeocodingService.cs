using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using psi25_project.Gateways.Interfaces;
using psi25_project.Models.Dtos;
using psi25_project.Services.Interfaces;

namespace psi25_project.Services
{
    public class GeocodingService : IGeocodingService
    {
        private readonly IGoogleMapsGateway _mapsGateway;
        private readonly IMemoryCache _cache;
        private const int MaxTriesPerCity = 1000;

        public GeocodingService(IGoogleMapsGateway mapsGateway, IMemoryCache cache)
        {
            _mapsGateway = mapsGateway;
            _cache = cache;
        }

        public async Task<(bool success, object result)> GetValidCoordinatesAsync()
        {
            while (true)
            {
                string address = AddressProvider.GetRandomAddress();

                // Use case-insensitive cache key to preserve original behavior
                var cacheKey = address.ToLowerInvariant();

                var coords = await _cache.GetOrCreateAsync(cacheKey, async entry =>
                {
                    entry.SetAbsoluteExpiration(TimeSpan.FromHours(1));
                    entry.SetSize(1);
                    return await _mapsGateway.GetCoordinatesAsync(address);
                });

                if (coords == null)
                {
                    throw new InvalidOperationException($"Failed to get coordinates for address: {address}");
                }

                double lat = coords.Lat;
                double lng = coords.Lng;

                for (int attempt = 1; attempt <= MaxTriesPerCity; attempt++)
                {
                    (lat, lng) = CoordinateModifier.ModifyCoordinates(lat, lng);

                    StreetViewLocationDto? streetView = await _mapsGateway.GetStreetViewMetadataAsync(lat, lng);

                    if (streetView != null)
                    {
                        return (true, new
                        {
                            address,
                            modifiedCoordinates = new { lat, lng },
                            panoID = streetView.PanoId,
                            attempts = attempt
                        });
                    }
                }
            }
        }
    }
}
