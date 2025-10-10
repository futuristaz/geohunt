using System.Threading.Tasks;
using psi25_project.Gateways;
using psi25_project.Models.Dtos;

namespace psi25_project.Services
{
    public class GeocodingService
    {
        private readonly GoogleMapsGateway _mapsGateway;
        private const int MaxTriesPerCity = 1000; // Hard limit for coordinate modification per city

        public GeocodingService(GoogleMapsGateway mapsGateway)
        {
            _mapsGateway = mapsGateway;
        }

        public async Task<(bool success, object result)> GetValidCoordinatesAsync()
        {
            while (true)
            {
                string address = AddressProvider.GetRandomAddress();

                GeocodeResultDto coords = await _mapsGateway.GetCoordinatesAsync(address);

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
