using System;
using System.Collections.Generic;
using System.Text;

namespace TrafficForm.App
{
    public class UpdateSelectedPosTrafficInfoCommand
    {
        public static readonly double MIN_LATITUDE = 33.0;
        public static readonly double MAX_LATITUDE = 38.6;
        public static readonly double MIN_LONGITUDE = 125.0;
        public static readonly double MAX_LONGITUDE = 129.8;

        public double Latitude { get; set; }
        public double Longitude { get; set; }

        public UpdateSelectedPosTrafficInfoCommand(double latitude, double longitude)
        {
            Latitude = latitude;
            Longitude = longitude;
        }
    }
}
