using Chinook.ClientModels;
using Chinook.Models;
using Microsoft.EntityFrameworkCore;
using NuGet.DependencyResolver;

namespace Chinook.Services.Playlist
{
    public class PlaylistService: IPlaylistService
    {
        private ChinookContext _dbContext;
        public PlaylistService(ChinookContext dbContext) 
        {
            this._dbContext = dbContext;
        }

        public async Task<Track> AddTrackToFavorites(long trackId)
        {
            var track = _dbContext.Tracks.FirstOrDefault(t => t.TrackId == trackId);
            var trackData = await _dbContext.Tracks.Where(t => t.TrackId == trackId).Include(a => a.Playlists).FirstOrDefaultAsync();
            var favoritePlaylist = await _dbContext.Playlists.Where(p => p.Name == "Favorites").FirstOrDefaultAsync();
            trackData.Playlists.Add(favoritePlaylist);
            _dbContext.Tracks.Update(trackData);
            await _dbContext.SaveChangesAsync();

            return trackData;

        }

        public async Task<Track> RemoveTrackFromFavorites(long trackId)
        {
            var track = _dbContext.Tracks.FirstOrDefault(t => t.TrackId == trackId);

            var trackData = await _dbContext.Tracks.Where(t => t.TrackId == trackId).Include(a => a.Playlists).FirstOrDefaultAsync();
            var favoritePlaylist = await _dbContext.Playlists.Where(p => p.Name == "Favorites").FirstOrDefaultAsync();

            trackData.Playlists.Remove(favoritePlaylist);
            _dbContext.Tracks.Update(trackData);
            await _dbContext.SaveChangesAsync();

            return trackData;
        }

        public async Task<List<PlaylistTrack>> RetrieveTracksList(long artistId, string currentUserId)
        {
            var tracks = await _dbContext.Tracks.Where(a => a.Album.ArtistId == artistId)
            .Include(a => a.Album)
            .Include(a => a.Playlists)
            .ThenInclude(p => p.UserPlaylists)
            .Select(t => new PlaylistTrack()
            {
                AlbumTitle = (t.Album == null ? "-" : t.Album.Title),
                TrackId = t.TrackId,
                TrackName = t.Name,
                IsFavorite = t.Playlists.Any(p => p.UserPlaylists.Any(up => up.UserId == currentUserId && up.Playlist.Name == "Favorites"))
            }).ToListAsync();

            return tracks;
        }
    }
}
