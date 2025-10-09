using System.Threading.Tasks;
using psi25_project.Gateways;
using psi25_project.Models.Dtos;

namespace psi25_project.Services
{
    public class GeocodingService
    {
        private readonly GoogleMapsGateway _mapsGateway;

        private const int MaxTriesPerCity = 50;     // Hardcoded per address
        private const int MaxTotalAttempts = 500;   // Safety limit to prevent infinite loop

        public GeocodingService(GoogleMapsGateway mapsGateway)
        {
            _mapsGateway = mapsGateway;
        }

        public async Task<(bool success, object result)> GetValidCoordinatesAsync()
        {
            int totalAttempts = 0;

            while (totalAttempts < MaxTotalAttempts)
            {
                string address = AddressProvider.GetRandomAddress();
                GeocodeResultDto coords = await _mapsGateway.GetCoordinatesAsync(address);

                double lat = coords.Lat;
                double lng = coords.Lng;

                int localAttempts = 0;
                StreetViewLocationDto? streetView = null;

                // Try multiple times for this address
                while (localAttempts < MaxTriesPerCity && totalAttempts < MaxTotalAttempts)
                {
                    totalAttempts++;
                    localAttempts++;

                    (lat, lng) = CoordinateModifier.ModifyCoordinates(lat, lng);
                    streetView = await _mapsGateway.GetStreetViewMetadataAsync(lat, lng);

                    if (streetView != null)
                    {
                        return (true, new
                        {
                            address,
                            modifiedCoordinates = new { lat, lng },
                            panoID = streetView.PanoId,
                            localAttempts,
                            totalAttempts
                        });
                    }
                }
            }

            return (false, new
            {
                totalAttempts,
                message = "No valid Street View found after multiple addresses and retries."
            });
        }
    }
}
