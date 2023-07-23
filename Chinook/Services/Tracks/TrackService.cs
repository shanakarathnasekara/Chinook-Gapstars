using Chinook.ClientModels;
using Chinook.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Chinook.Services.Tracks
{
    public class TrackService: ITrackService
    {
        private ChinookContext _dbContext;

        public TrackService(ChinookContext dbContext)
        {
            _dbContext = dbContext;
        }

        // Retrieve tracks data
        public async Task<List<PlaylistTrack>> RetrieveTracksList(long artistId, string currentUserId)
        {
            var tracks = await _dbContext.Tracks.Where(a => a.Album.ArtistId == artistId)
            .Include(a => a.Album)
            .Include(a => a.Playlists).ThenInclude(p => p.UserPlaylists)
            .Select(t => new PlaylistTrack()
            {
                AlbumTitle = (t.Album == null ? "-" : t.Album.Title),
                TrackId = t.TrackId,
                TrackName = t.Name,
                IsFavorite = t.Playlists.Any(p => p.UserPlaylists.Any(up => up.UserId == currentUserId && up.Playlist.Name == "Favorites"))
            }).ToListAsync();

            if (tracks == null)
            {
                throw new CustomException
                {
                    CustomMessage = "Error occurred when retrieving tracks"
                };
            }

            return tracks;
        }
    }
}
