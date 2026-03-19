namespace TrafficForm.Domain
{
    public enum TrafficLevel
    {
        Unknown = 0,
        Smooth = 1,
        Slow = 2,
        Heavy = 3,
        Congested = 4
    }
    
    public static class TrafficLevelExtensions
    {
    public static string ToDisplayString(this TrafficLevel level)
        {
            return level switch
            {
                TrafficLevel.Unknown => "확인불가",
                TrafficLevel.Smooth => "원활",
                TrafficLevel.Slow => "보통",
                TrafficLevel.Heavy => "혼잡",
                TrafficLevel.Congested => "정체",
                _ => level.ToString()
            };
        }
    }
    

}
