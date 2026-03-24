namespace TrafficForm.Domain
{
    public sealed class VdsTrafficObservation
    {
        public string VdsId { get; }
        public string CollectedDate { get; }
        public double Speed { get; }
        public int Volume { get; }
        public double Occupancy { get; }

        public VdsTrafficObservation(
            string vdsId,
            string collectedDate,
            double speed,
            int volume,
            double occupancy)
        {
            ArgumentNullException.ThrowIfNull(vdsId);
            ArgumentNullException.ThrowIfNull(collectedDate);

            VdsId = vdsId;
            CollectedDate = collectedDate;
            Speed = speed;
            Volume = volume;
            Occupancy = occupancy;
        }
    }
}
