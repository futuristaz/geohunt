using System.Threading.Tasks;
using psi25_project.Gateways;
using psi25_project.Models.Dtos;
using psi25_project.Utils;

namespace psi25_project.Services
{
    public class DistanceService
    {
        private readonly GoogleMapsGateway _mapsGateway;

        public DistanceService(GoogleMapsGateway mapsGateway)
        {
            _mapsGateway = mapsGateway;
        }

        //--------------------------------------------------------------------------------------
        public async Task<object> CalculateDistanceAsync(string address1, string address2)
        {
            GeocodeResultDto coords1 = await _mapsGateway.GetCoordinatesAsync(address1);
            GeocodeResultDto coords2 = await _mapsGateway.GetCoordinatesAsync(address2);

            double distanceKm = DistanceCalculator.CalculateHaversineDistance(
                (coords1.Lat, coords1.Lng),
                (coords2.Lat, coords2.Lng),
                2
            );

            return new
            {
                address1,
                address2,
                coords = new
                {
                    first = new { lat = coords1.Lat, lng = coords1.Lng },
                    second = new { lat = coords2.Lat, lng = coords2.Lng }
                },
                distance_km = distanceKm
            };
        }
        //--------------------------------------------------------------------------------------
    }
}
