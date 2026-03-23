using TrafficForm.Domain;
using TrafficForm.Port;

namespace TrafficForm.App
{
    public sealed class FavoriteService
    {
        private readonly IFavoriteStorePort _favoriteStorePort;
        private readonly SemaphoreSlim _sync = new SemaphoreSlim(1, 1);

        public FavoriteService(IFavoriteStorePort favoriteStorePort)
        {
            _favoriteStorePort = favoriteStorePort ?? throw new ArgumentNullException(nameof(favoriteStorePort));
        }

        public async Task<UserFavorites> GetFavoritesAsync()
        {
            await _sync.WaitAsync();
            try
            {
                UserFavorites favorites = await _favoriteStorePort.LoadAsync();
                return Clone(favorites);
            }
            finally
            {
                _sync.Release();
            }
        }

        public async Task AddHighwayFavoriteAsync(HighwayFavorite favorite)
        {
            if (favorite == null)
            {
                throw new ArgumentNullException(nameof(favorite));
            }

            if (favorite.HighwayNo <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(favorite), "고속도로 번호는 1 이상이어야 합니다.");
            }

            await _sync.WaitAsync();
            try
            {
                UserFavorites favorites = await _favoriteStorePort.LoadAsync();
                favorites.HighwayFavorites.Add(favorite);
                await _favoriteStorePort.SaveAsync(favorites);
            }
            finally
            {
                _sync.Release();
            }
        }

        public async Task AddCoordinateFavoriteAsync(CoordinateFavorite favorite)
        {
            if (favorite == null)
            {
                throw new ArgumentNullException(nameof(favorite));
            }

            if (string.IsNullOrWhiteSpace(favorite.Name))
            {
                throw new ArgumentException("좌표 즐겨찾기 이름은 비어 있을 수 없습니다.", nameof(favorite));
            }

            await _sync.WaitAsync();
            try
            {
                UserFavorites favorites = await _favoriteStorePort.LoadAsync();
                favorites.CoordinateFavorites.Add(favorite);
                await _favoriteStorePort.SaveAsync(favorites);
            }
            finally
            {
                _sync.Release();
            }
        }

        public async Task RemoveHighwayFavoriteAsync(string favoriteId)
        {
            if (string.IsNullOrWhiteSpace(favoriteId))
            {
                return;
            }

            await _sync.WaitAsync();
            try
            {
                UserFavorites favorites = await _favoriteStorePort.LoadAsync();
                favorites.HighwayFavorites.RemoveAll(item => string.Equals(item.FavoriteId, favoriteId, StringComparison.Ordinal));
                await _favoriteStorePort.SaveAsync(favorites);
            }
            finally
            {
                _sync.Release();
            }
        }

        public async Task RemoveCoordinateFavoriteAsync(string favoriteId)
        {
            if (string.IsNullOrWhiteSpace(favoriteId))
            {
                return;
            }

            await _sync.WaitAsync();
            try
            {
                UserFavorites favorites = await _favoriteStorePort.LoadAsync();
                favorites.CoordinateFavorites.RemoveAll(item => string.Equals(item.FavoriteId, favoriteId, StringComparison.Ordinal));
                await _favoriteStorePort.SaveAsync(favorites);
            }
            finally
            {
                _sync.Release();
            }
        }

        private static UserFavorites Clone(UserFavorites source)
        {
            UserFavorites clone = new UserFavorites();

            foreach (HighwayFavorite item in source.HighwayFavorites)
            {
                clone.HighwayFavorites.Add(new HighwayFavorite
                {
                    FavoriteId = item.FavoriteId,
                    HighwayNo = item.HighwayNo,
                    Name = item.Name,
                    SavedAt = item.SavedAt,
                    View = Clone(item.View)
                });
            }

            foreach (CoordinateFavorite item in source.CoordinateFavorites)
            {
                clone.CoordinateFavorites.Add(new CoordinateFavorite
                {
                    FavoriteId = item.FavoriteId,
                    Name = item.Name,
                    SavedAt = item.SavedAt,
                    View = Clone(item.View)
                });
            }

            return clone;
        }

        private static MapViewSnapshot Clone(MapViewSnapshot source)
        {
            return new MapViewSnapshot
            {
                Latitude = source.Latitude,
                Longitude = source.Longitude,
                ZoomLevel = source.ZoomLevel,
                MinLongitude = source.MinLongitude,
                MinLatitude = source.MinLatitude,
                MaxLongitude = source.MaxLongitude,
                MaxLatitude = source.MaxLatitude
            };
        }
    }
}
