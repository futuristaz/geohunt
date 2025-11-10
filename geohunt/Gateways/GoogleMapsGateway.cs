using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using geohunt.Models.Dtos;

namespace geohunt.Gateways
{
    public class GoogleMapsGateway
    {
        private readonly string _apiKey;
        private readonly HttpClient _httpClient;

        public GoogleMapsGateway(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _apiKey = configuration["GoogleMaps:ApiKey"]
                      ?? throw new Exception("Google Maps API key not found in environment variables.");
        }

        // ------------------------------------------------------------------
        public async Task<GeocodeResultDto> GetCoordinatesAsync(string address)
        {
            string url = $"https://maps.googleapis.com/maps/api/geocode/json?address={Uri.EscapeDataString(address)}&key={_apiKey}";
            HttpResponseMessage response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            string content = await response.Content.ReadAsStringAsync();

            using var doc = JsonDocument.Parse(content);
            var location = doc.RootElement
                .GetProperty("results")[0]
                .GetProperty("geometry")
                .GetProperty("location");

            return new GeocodeResultDto
            {
                Lat = location.GetProperty("lat").GetDouble(),
                Lng = location.GetProperty("lng").GetDouble()
            };
        }

        // ------------------------------------------------------------------
        public async Task<StreetViewLocationDto?> GetStreetViewMetadataAsync(double lat, double lng)
        {
            string url = $"https://maps.googleapis.com/maps/api/streetview/metadata?location={lat},{lng}&key={_apiKey}";
            HttpResponseMessage response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            string content = await response.Content.ReadAsStringAsync();
            var metadata = JsonSerializer.Deserialize<StreetViewMetadataDto>(content);

            if (metadata?.Status == "OK" && metadata.Location != null)
            {
                return new StreetViewLocationDto
                {
                    Lat = metadata.Location.Latitude,
                    Lng = metadata.Location.Longitude,
                    PanoId = metadata.PanoId
                };
            }

            return null;
        }
    }
}
