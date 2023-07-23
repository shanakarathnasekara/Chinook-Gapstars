using Chinook.Models;

namespace Chinook.Services.Artists
{
    public interface IArtistService
    {
        Task<List<ClientModels.Artist>> GetArtistsFromDb();
    }
}
