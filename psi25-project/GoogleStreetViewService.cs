using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

public class GoogleStreetViewService
{
    private readonly string _apiKey;
    private readonly HttpClient _httpClient;

    public GoogleStreetViewService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _apiKey = configuration["GoogleMaps:ApiKey"]
                  ?? throw new Exception("Google Maps API key not found in configuration.");
    }

    public async Task<byte[]> GetStreetViewAsync(double lat, double lng, int width = 600, int height = 400)
    {
        string url = $"https://maps.googleapis.com/maps/api/streetview?size={width}x{height}&location={lat},{lng}&key={_apiKey}";
        
        HttpResponseMessage response = await _httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadAsByteArrayAsync();
    }
}
