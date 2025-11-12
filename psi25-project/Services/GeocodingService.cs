using psi25_project.Gateways;
using psi25_project.Models.Dtos;

namespace psi25_project.Services
{
    public class GeocodingService
    {
        private readonly GoogleMapsGateway _mapsGateway;
        private readonly LocationService _locationService;
        private const int MaxTriesPerCity = 1000; // Hard limit for coordinate modification per city
        private const int TimeoutSeconds = 3;

        public GeocodingService(GoogleMapsGateway mapsGateway, LocationService locationService)
        {
            _mapsGateway = mapsGateway;
            _locationService = locationService;
        }

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

                GeocodeResultDto coords = await _mapsGateway.GetCoordinatesAsync(address, cancellationToken);

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
