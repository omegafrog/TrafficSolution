using System;
using System.Collections.Generic;
using System.Text;

namespace TrafficForm.Domain
{
    public class Location
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string Name { get; set; }

        public Location(double latitude, double longitude, string name)
        {
            Latitude = latitude;
            Longitude = longitude;
            Name = name;
        }

        public Location(double latitude, double longitude)
        {
            Latitude = latitude;
            Longitude = longitude;
            Name = string.Empty;
        }

        public override bool Equals(object? obj)
        {
            if(obj != null && obj is Location other)
            {
                return Latitude.Equals(other.Latitude) && Longitude.Equals(other.Longitude);
            }
            else
            {
                return false;
            }
        }
    }
}
