using Chinook.Models;

namespace Chinook.Services.Artists
{
    public interface IArtistService
    {
        Task<List<Artist>> GetArtistsFromDb();
    }
}
