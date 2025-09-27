using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using System.Text.Json.Serialization;

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

    public async Task<(double lat, double lng)> GetCoordinatesAsync(string address)
    {
        string url = $"https://maps.googleapis.com/maps/api/geocode/json?address={Uri.EscapeDataString(address)}&key={_apiKey}";
        HttpResponseMessage response = await _httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();

        string content = await response.Content.ReadAsStringAsync();

        using var doc = JsonDocument.Parse(content);
        var root = doc.RootElement;

        var location = root
            .GetProperty("results")[0]
            .GetProperty("geometry")
            .GetProperty("location");

        double lat = location.GetProperty("lat").GetDouble();
        double lng = location.GetProperty("lng").GetDouble();
        return (lat, lng);
    }
// ----------------------------------------------------------------------------------------------------------------
    public async Task<StreetViewLocation?> GetStreetViewMetadataAsync(double lat, double lng)
    {
        string url = $"https://maps.googleapis.com/maps/api/streetview/metadata?location={lat},{lng}&key={_apiKey}";
        HttpResponseMessage response = await _httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();

        string content = await response.Content.ReadAsStringAsync();
        var metadata = JsonSerializer.Deserialize<StreetViewMetadata>(content);

        if (metadata?.Status == "OK" && metadata.Location != null)
        {
            return new StreetViewLocation
            {
                Lat = metadata.Location.Latitude,
                Lng = metadata.Location.Longitude,
                PanoId = metadata.PanoId
            };
        }

        return null;
    }
}
// ----------------------------------------------------------------------------------------------------------------
// Helper models
public class StreetViewMetadata
{
    [JsonPropertyName("status")]
    public string Status { get; set; }

    [JsonPropertyName("pano_id")]
    public string PanoId { get; set; }

    [JsonPropertyName("location")]
    public StreetViewLatLng Location { get; set; }
}

public class StreetViewLatLng
{
    [JsonPropertyName("lat")]
    public double Latitude { get; set; }

    [JsonPropertyName("lng")]
    public double Longitude { get; set; }
}

public class StreetViewLocation
{
    public double Lat { get; set; }
    public double Lng { get; set; }
    public string PanoId { get; set; }
}
