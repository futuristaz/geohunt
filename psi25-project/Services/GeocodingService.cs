using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using psi25_project.Gateways.Interfaces;
using psi25_project.Models.Dtos;
using psi25_project.Services.Interfaces;
using psi25_project.Utils;

namespace psi25_project.Services
{
    public class GeocodingService : IGeocodingService
    {
        private readonly IGoogleMapsGateway _mapsGateway;
        private readonly IMemoryCache _cache;
        private static readonly ConcurrentDictionary<string, Lazy<Task<GeocodeResultDto>>> _inFlightGeocodingRequests = new();
        private const int MaxTriesPerCity = 1000;

        public GeocodingService(IGoogleMapsGateway mapsGateway, IMemoryCache cache)
        {
            _mapsGateway = mapsGateway;
            _cache = cache;
        }

        private static string NormalizeAddressKey(string address) => address.ToLowerInvariant();

        public async Task<(bool success, object result)> GetValidCoordinatesAsync()
        {
            while (true)
            {
                string address = AddressProvider.GetRandomAddress();

                // Use normalized cache key for both cache and in-flight tracking
                var cacheKey = NormalizeAddressKey(address);

                GeocodeResultDto? coords;

                // Check cache first
                if (!_cache.TryGetValue(cacheKey, out coords))
                {
                    // Deduplicate in-flight requests using Lazy<Task<T>>
                    var lazyTask = _inFlightGeocodingRequests.GetOrAdd(
                        cacheKey,
                        key => new Lazy<Task<GeocodeResultDto>>(
                            async () => await _mapsGateway.GetCoordinatesAsync(address),
                            LazyThreadSafetyMode.ExecutionAndPublication
                        )
                    );

                    try
                    {
                        coords = await lazyTask.Value;

                        // Store in cache
                        _cache.Set(cacheKey, coords, new MemoryCacheEntryOptions
                        {
                            AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1),
                            Size = 1
                        });
                    }
                    finally
                    {
                        // Always remove from in-flight tracking
                        _inFlightGeocodingRequests.TryRemove(cacheKey, out _);
                    }
                }

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
