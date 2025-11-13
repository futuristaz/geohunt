using System;
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

        public GeocodingService(IGoogleMapsGateway mapsGateway)
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
