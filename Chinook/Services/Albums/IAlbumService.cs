using Chinook.Models;

namespace Chinook.Services.Albums
{
    public interface IAlbumService
    {
        Task<List<Album>> GetAlbumsFromDb(long artistId);
    }
}
