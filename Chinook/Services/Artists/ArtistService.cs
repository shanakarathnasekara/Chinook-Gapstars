using Chinook.Models;
using Microsoft.EntityFrameworkCore;

namespace Chinook.Services.Artists
{
    public class ArtistService: IArtistService
    {
        private ChinookContext _dbContext;

        public ArtistService(ChinookContext dbContext) 
        {
            _dbContext = dbContext;
        }

        public async Task<List<Artist>> GetArtistsFromDb()
        {
            return await _dbContext.Artists.Include(artist => artist.Albums).ToListAsync();
        }
    }
}
