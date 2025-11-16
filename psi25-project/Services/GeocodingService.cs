using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using psi25_project.Gateways.Interfaces;
using psi25_project.Models.Dtos;
using psi25_project.Services.Interfaces;

namespace psi25_project.Services
{
    public class GeocodingService : IGeocodingService
    {
        private readonly IGoogleMapsGateway _mapsGateway;
        private const int MaxTriesPerCity = 1000;

        private static readonly ConcurrentDictionary<string, Lazy<Task<GeocodeResultDto>>> _geocodeCache =
            new(StringComparer.OrdinalIgnoreCase);

        public GeocodingService(IGoogleMapsGateway mapsGateway)
        {
            _mapsGateway = mapsGateway;
        }

        private Task<GeocodeResultDto> GetGeocodeAsync(string address) =>
            _geocodeCache.GetOrAdd(address,
                key => new Lazy<Task<GeocodeResultDto>>(
                    () => _mapsGateway.GetCoordinatesAsync(key),
                    LazyThreadSafetyMode.ExecutionAndPublication)).Value;

        public async Task<(bool success, object result)> GetValidCoordinatesAsync()
        {
            while (true)
            {
                string address = AddressProvider.GetRandomAddress();

                GeocodeResultDto coords;
                try
                {
                    coords = await GetGeocodeAsync(address);
                }
                catch
                {
                    _geocodeCache.TryRemove(address, out _);
                    throw;
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
