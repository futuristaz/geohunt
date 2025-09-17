using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

public class GoogleMapsService
{
    private readonly string _apiKey;
    private readonly HttpClient _httpClient;

    public GoogleMapsService(HttpClient httpClient, IConfiguration configuration)
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
}
