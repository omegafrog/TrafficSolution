namespace TrafficForm.Domain
{
    public static class TrafficLevelPolicy
    {
        public static TrafficLevel CalculateTrafficLevel(VdsTrafficResult trafficResult)
        {
            if (trafficResult == null)
            {
                return TrafficLevel.Unknown;
            }

            if (trafficResult.Speed < 0 || trafficResult.Occupancy < 0)
            {
                return TrafficLevel.Unknown;
            }

            if (trafficResult.Speed < 30 || trafficResult.Occupancy >= 70)
            {
                return TrafficLevel.Congested;
            }

            if (trafficResult.Speed < 50 || trafficResult.Occupancy >= 55)
            {
                return TrafficLevel.Heavy;
            }

            if (trafficResult.Speed < 70 || trafficResult.Occupancy >= 35)
            {
                return TrafficLevel.Slow;
            }

            return TrafficLevel.Smooth;
        }

        public static string GetColorHex(TrafficLevel trafficLevel)
        {
            return trafficLevel switch
            {
                TrafficLevel.Smooth => "#1f8a3b",
                TrafficLevel.Slow => "#f4b400",
                TrafficLevel.Heavy => "#ef6c00",
                TrafficLevel.Congested => "#d93025",
                _ => "#6d6d6d"
            };
        }
    }
}
