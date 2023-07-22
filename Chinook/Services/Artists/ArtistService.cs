using Chinook.Common.Models;
using Chinook.Models;
using Microsoft.CodeAnalysis.CSharp.Syntax;
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

        // Retrieve artists data from database
        public async Task<List<ClientModels.Artist>> GetArtistsFromDb()
        {
            List<ClientModels.Artist> artistData = await _dbContext.Artists
                .Include(artist => artist.Albums)
                .Select(a => new ClientModels.Artist()
                {
                    ArtistId = a.ArtistId,
                    ArtistName = a.Name,
                    Albums = a.Albums.Select (alb => new ClientModels.Album
                    {
                        AlbumId = alb.AlbumId,
                        ArtistId  = alb.ArtistId,
                        Title = alb.Title
                    }).ToList()
                })
                .ToListAsync();
            
            if (artistData == null)
            {
                throw new CustomException()
                {
                    CustomMessage = "Error occurred when retrieving artists data from Db"
                };
            }

            return artistData;
        }
    }
}
