using System.Text.Json;
using TrafficForm.Domain;
using TrafficForm.Port;

namespace TrafficForm.Adapter
{
    public sealed class JsonFavoriteStoreAdapter : IFavoriteStorePort
    {
        private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            WriteIndented = true
        };

        private readonly string _storePath;

        public JsonFavoriteStoreAdapter()
        {
            string baseDirectory = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "TrafficSolution");

            _storePath = Path.Combine(baseDirectory, "user-favorites.json");
        }

        public async Task<UserFavorites> LoadAsync()
        {
            if (!File.Exists(_storePath))
            {
                return new UserFavorites();
            }

            await using FileStream stream = new FileStream(_storePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            UserFavorites? favorites = await JsonSerializer.DeserializeAsync<UserFavorites>(stream, JsonOptions);
            return favorites ?? new UserFavorites();
        }

        public async Task SaveAsync(UserFavorites favorites)
        {
            if (favorites == null)
            {
                throw new ArgumentNullException(nameof(favorites));
            }

            string? directory = Path.GetDirectoryName(_storePath);
            if (!string.IsNullOrWhiteSpace(directory))
            {
                Directory.CreateDirectory(directory);
            }

            await using FileStream stream = new FileStream(_storePath, FileMode.Create, FileAccess.Write, FileShare.None);
            await JsonSerializer.SerializeAsync(stream, favorites, JsonOptions);
            await stream.FlushAsync();
        }
    }
}
