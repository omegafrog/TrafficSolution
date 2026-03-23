namespace TrafficForm.Domain
{
    public sealed class UserFavorites
    {
        public List<HighwayFavorite> HighwayFavorites { get; set; } = new List<HighwayFavorite>();

        public List<CoordinateFavorite> CoordinateFavorites { get; set; } = new List<CoordinateFavorite>();
    }
}
