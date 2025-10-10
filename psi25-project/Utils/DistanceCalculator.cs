namespace psi25_project.Utils;

public static class DistanceCalculator
{
    public static double CalculateHaversineDistance((double lat, double lng) coords1, (double lat, double lng) coords2, int precision)
    {
        const int R = 6371000; // Earth's radius in meters

        double phi1 = coords1.lat * Math.PI / 180;
        double phi2 = coords2.lat * Math.PI / 180;
        double dphi = (coords2.lat - coords1.lat) * Math.PI / 180;
        double dlambda = (coords2.lng - coords1.lng) * Math.PI / 180;

        double a = Math.Sin(dphi / 2) * Math.Sin(dphi / 2) +
                   Math.Cos(phi1) * Math.Cos(phi2) *
                   Math.Sin(dlambda / 2) * Math.Sin(dlambda / 2);
        double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

        return Math.Round(R * c / 1000, precision);
    }
}