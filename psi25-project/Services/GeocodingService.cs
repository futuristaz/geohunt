using psi25_project.Gateways;
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
        private readonly ILocationService _locationService;
        private static readonly ConcurrentDictionary<string, Lazy<Task<GeocodeResultDto>>> _inFlightGeocodingRequests = new();
        private const int MaxTriesPerCity = 1000;
        private const int TimeoutSeconds = 3;

        public GeocodingService(IGoogleMapsGateway mapsGateway, IMemoryCache cache, ILocationService locationService)
        {
            _mapsGateway = mapsGateway;
            _cache = cache;
            _locationService = locationService;
        }

        private static string NormalizeAddressKey(string address) => address.ToLowerInvariant();

        public async Task<(bool success, object result)> GetValidCoordinatesAsync()
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(TimeoutSeconds));

            try
            {
                return await GetValidCoordinatesInternalAsync(cts.Token);
            } catch (OperationCanceledException)
            {
                var locationfromDb = await _locationService.GetOldestLocationAsync();

                if (locationfromDb == null) return (false, new { error = "Timeout: No locations available in the database" });

                return (true, new
                {
                    locationfromDb.id,
                    source = "database",
                    address = "fallback",
                    modifiedCoordinates = new
                    {
                        lat = locationfromDb.Latitude,
                        lng = locationfromDb.Longitude
                    },
                    panoID = locationfromDb.panoId
                });
            }
        }

        private async Task<(bool success, object result)> GetValidCoordinatesInternalAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
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
                    cancellationToken.ThrowIfCancellationRequested();

                    (lat, lng) = CoordinateModifier.ModifyCoordinates(lat, lng);

                    StreetViewLocationDto? streetView = await _mapsGateway.GetStreetViewMetadataAsync(lat, lng, cancellationToken);

                    if (streetView != null)
                    {
                        return (true, new
                        {
                            source = "generated",
                            address,
                            modifiedCoordinates = new { lat, lng },
                            panoID = streetView.PanoId,
                            attempts = attempt
                        });
                    }
                }
            }

            cancellationToken.ThrowIfCancellationRequested();
            return (false, null); // this line should never be reached
        }
    }
}
