using Chinook.Models;
using Microsoft.EntityFrameworkCore;

namespace Chinook.Services.Albums
{
    public class AlbumService: IAlbumService
    {
        private ChinookContext _dbContext;

        public AlbumService(ChinookContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<List<Album>> GetAlbumsFromDb(long artistId)
        {
            return await _dbContext.Albums.Where(album => album.ArtistId == artistId).ToListAsync();
        }
    }
}
