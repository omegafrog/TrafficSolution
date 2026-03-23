using TrafficForm.App;
using TrafficForm.Domain;
using TrafficForm.Port;

namespace TestProject1;

[TestClass]
public sealed class FavoriteServiceTest
{
    [TestMethod]
    public async Task AddHighwayFavoriteAsync_SavesFavorite()
    {
        InMemoryFavoriteStorePort store = new InMemoryFavoriteStorePort();
        FavoriteService service = new FavoriteService(store);

        await service.AddHighwayFavoriteAsync(new HighwayFavorite
        {
            HighwayNo = 1,
            Name = "1번 테스트",
            View = new MapViewSnapshot
            {
                Latitude = 37.55,
                Longitude = 126.98,
                ZoomLevel = 12,
                MinLongitude = 126.9,
                MinLatitude = 37.4,
                MaxLongitude = 127.1,
                MaxLatitude = 37.7
            }
        });

        UserFavorites favorites = await service.GetFavoritesAsync();
        Assert.AreEqual(1, favorites.HighwayFavorites.Count);
        Assert.AreEqual(1, favorites.HighwayFavorites[0].HighwayNo);
    }

    [TestMethod]
    public async Task AddCoordinateFavoriteAsync_EmptyName_ThrowsArgumentException()
    {
        InMemoryFavoriteStorePort store = new InMemoryFavoriteStorePort();
        FavoriteService service = new FavoriteService(store);

        await Assert.ThrowsAsync<ArgumentException>(() => service.AddCoordinateFavoriteAsync(new CoordinateFavorite
        {
            Name = " ",
            View = new MapViewSnapshot()
        }));
    }

    [TestMethod]
    public async Task RemoveHighwayFavoriteAsync_RemovesById()
    {
        InMemoryFavoriteStorePort store = new InMemoryFavoriteStorePort();
        FavoriteService service = new FavoriteService(store);

        HighwayFavorite favorite = new HighwayFavorite
        {
            FavoriteId = "favorite-1",
            HighwayNo = 35,
            Name = "35번",
            View = new MapViewSnapshot()
        };

        await service.AddHighwayFavoriteAsync(favorite);
        await service.RemoveHighwayFavoriteAsync("favorite-1");

        UserFavorites favorites = await service.GetFavoritesAsync();
        Assert.AreEqual(0, favorites.HighwayFavorites.Count);
    }

    private sealed class InMemoryFavoriteStorePort : IFavoriteStorePort
    {
        private UserFavorites _favorites = new UserFavorites();

        public Task<UserFavorites> LoadAsync()
        {
            return Task.FromResult(Clone(_favorites));
        }

        public Task SaveAsync(UserFavorites favorites)
        {
            _favorites = Clone(favorites);
            return Task.CompletedTask;
        }

        private static UserFavorites Clone(UserFavorites source)
        {
            UserFavorites clone = new UserFavorites();

            clone.HighwayFavorites.AddRange(source.HighwayFavorites.Select(item => new HighwayFavorite
            {
                FavoriteId = item.FavoriteId,
                HighwayNo = item.HighwayNo,
                Name = item.Name,
                SavedAt = item.SavedAt,
                View = new MapViewSnapshot
                {
                    Latitude = item.View.Latitude,
                    Longitude = item.View.Longitude,
                    ZoomLevel = item.View.ZoomLevel,
                    MinLongitude = item.View.MinLongitude,
                    MinLatitude = item.View.MinLatitude,
                    MaxLongitude = item.View.MaxLongitude,
                    MaxLatitude = item.View.MaxLatitude
                }
            }));

            clone.CoordinateFavorites.AddRange(source.CoordinateFavorites.Select(item => new CoordinateFavorite
            {
                FavoriteId = item.FavoriteId,
                Name = item.Name,
                SavedAt = item.SavedAt,
                View = new MapViewSnapshot
                {
                    Latitude = item.View.Latitude,
                    Longitude = item.View.Longitude,
                    ZoomLevel = item.View.ZoomLevel,
                    MinLongitude = item.View.MinLongitude,
                    MinLatitude = item.View.MinLatitude,
                    MaxLongitude = item.View.MaxLongitude,
                    MaxLatitude = item.View.MaxLatitude
                }
            }));

            return clone;
        }
    }
}
