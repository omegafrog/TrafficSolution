using TrafficForm.Domain;

namespace TestProject1
{
    [TestClass]
    public sealed class TrafficLevelPolicyTest
    {
        [TestMethod]
        public void CalculateTrafficLevel_Congested_WhenSpeedIsLow()
        {
            VdsTrafficResult trafficResult = new VdsTrafficResult
            {
                VdsId = "0010VDS00001",
                CollectedDate = "2000-01-01 00:00:00",
                Speed = 25,
                Volume = 100,
                Occupancy = 10,
                Location = new Location
                {
                    Latitude = 37.5,
                    Longitude = 127.0,
                    Name = string.Empty
                }
            };

            TrafficLevel level = TrafficLevelPolicy.CalculateTrafficLevel(trafficResult);

            Assert.AreEqual(TrafficLevel.Congested, level);
            Assert.AreEqual("#d93025", TrafficLevelPolicy.GetColorHex(level));
        }

        [TestMethod]
        public void CalculateTrafficLevel_Smooth_WhenSpeedAndOccupancyAreGood()
        {
            VdsTrafficResult trafficResult = new VdsTrafficResult
            {
                VdsId = "0010VDS00002",
                CollectedDate = "2000-01-01 00:00:00",
                Speed = 90,
                Volume = 120,
                Occupancy = 15,
                Location = new Location
                {
                    Latitude = 37.6,
                    Longitude = 127.1,
                    Name = string.Empty
                }
            };

            TrafficLevel level = TrafficLevelPolicy.CalculateTrafficLevel(trafficResult);

            Assert.AreEqual(TrafficLevel.Smooth, level);
            Assert.AreEqual("#1f8a3b", TrafficLevelPolicy.GetColorHex(level));
        }
    }
}
