using Chinook.Common.Models;
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

        // Retrieve album data from database
        public async Task<List<Album>> GetAlbumsFromDb(long artistId)
        {
            List<Album> albumData =  await _dbContext.Albums.Where(album => album.ArtistId == artistId).ToListAsync();
            
            if (albumData == null)
            {
                throw new CustomException
                {
                    CustomMessage = "Error occurred when retrieving albums data from Db"
                };
            }            

            return albumData;
        }
    }
}
