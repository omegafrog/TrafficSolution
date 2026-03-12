using System;
using System.Collections.Generic;
using System.Text;

namespace TrafficForm.App
{
    public static class Util
    {
        public static double DistanceKm(
    double lat1, double lon1,
    double lat2, double lon2)
        {
            const double R = 6371.0; // Earth radius km

            double dLat = ToRadians(lat2 - lat1);
            double dLon = ToRadians(lon2 - lon1);

            lat1 = ToRadians(lat1);
            lat2 = ToRadians(lat2);

            double a =
                Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(lat1) * Math.Cos(lat2) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

            double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

            return R * c;
        }

        static double ToRadians(double deg)
        {
            return deg * Math.PI / 180;
        }
    }
}
