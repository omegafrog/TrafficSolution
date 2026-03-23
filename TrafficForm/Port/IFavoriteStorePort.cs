using TrafficForm.Domain;

namespace TrafficForm.Port
{
    public interface IFavoriteStorePort
    {
        Task<UserFavorites> LoadAsync();

        Task SaveAsync(UserFavorites favorites);
    }
}
