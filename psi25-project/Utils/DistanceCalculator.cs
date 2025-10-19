using psi25_project.Models.Dtos;
using psi25_project.Extensions;

public static class DistanceCalculator
{
    public static double CalculateHaversineDistance(GeocodeResultDto coords1, GeocodeResultDto coords2, int precision = 4)
    {
        const int R = 6371000; // Earth's radius in meters

        double phi1 = coords1.Lat.ToRadians();
        double phi2 = coords2.Lat.ToRadians();
        double dphi = (coords2.Lat - coords1.Lat).ToRadians();
        double dlambda = (coords2.Lng - coords1.Lng).ToRadians();
            
        double a = Math.Sin(dphi / 2) * Math.Sin(dphi / 2) +
                   Math.Cos(phi1) * Math.Cos(phi2) *
                   Math.Sin(dlambda / 2) * Math.Sin(dlambda / 2);
        double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

        return Math.Round(R * c / 1000, precision);
    }
}