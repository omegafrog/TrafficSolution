namespace TrafficForm.Domain
{
    public sealed class HighwayFavorite
    {
        public string FavoriteId { get; set; } = Guid.NewGuid().ToString("N");

        public int HighwayNo { get; set; }

        public string Name { get; set; } = string.Empty;

        public MapViewSnapshot View { get; set; } = new MapViewSnapshot();

        public DateTimeOffset SavedAt { get; set; } = DateTimeOffset.UtcNow;
    }
}
