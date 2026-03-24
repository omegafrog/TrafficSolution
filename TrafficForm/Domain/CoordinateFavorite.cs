namespace TrafficForm.Domain
{
    public sealed class CoordinateFavorite
    {
        public string FavoriteId { get; set; } = Guid.NewGuid().ToString("N");

        public string Name { get; set; } = string.Empty;

        public MapViewSnapshot View { get; set; } = new MapViewSnapshot();

        public DateTimeOffset SavedAt { get; set; } = DateTimeOffset.UtcNow;
    }
}
