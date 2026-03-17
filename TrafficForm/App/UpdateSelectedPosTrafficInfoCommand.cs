using System;
using System.Collections.Generic;
using System.Text;

namespace TrafficForm.App
{
    public class UpdateSelectedPosTrafficInfoCommand
    {
        public static readonly double MIN_LATITUDE = 33.0;
        public static readonly double MAX_LATITUDE = 39.0;
        public static readonly double MIN_LONGITUDE = 125.0;
        public static readonly double MAX_LONGITUDE = 132.0;

        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public double MinLongitude {  get; set; }
        public double MinLatitude { get; set; }
        public double MaxLongitude { get; set; }
        public double MaxLatitude { get; set; }

        public UpdateSelectedPosTrafficInfoCommand(double latitude, double longitude)
        {
            Latitude = latitude;
            Longitude = longitude;
        }
    }
}
