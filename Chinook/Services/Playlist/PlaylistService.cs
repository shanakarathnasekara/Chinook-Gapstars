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

        public async Task<List<Models.Playlist>> RetrieveUsersListofPlaylist(string currentUserId)
        {
            var userPlaylists = await _dbContext.Playlists.Include(p => p.UserPlaylists).Where(p => p.UserPlaylists.Any(up => up.UserId == currentUserId && up.Playlist.Name != "Favorites")).ToListAsync();
            return userPlaylists;
        }

        public async Task AddNewPlaylist(PlaylistTrack selectedTrack, string newPlaylistName, string currentUserId)
        {
            Models.Playlist playlist = new Models.Playlist()
            {
                Name = newPlaylistName,
                PlaylistId = _dbContext.Playlists.OrderBy(a => a.PlaylistId).Last().PlaylistId + 1
            };

            _dbContext.Playlists.Add(playlist);
            await _dbContext.SaveChangesAsync();


            var track = _dbContext.Tracks.FirstOrDefault(t => t.TrackId == selectedTrack.TrackId);
            var trackData = await _dbContext.Tracks.Where(t => t.TrackId == selectedTrack.TrackId).Include(a => a.Playlists).FirstOrDefaultAsync();
            var favoritePlaylist = await _dbContext.Playlists.Where(p => p.Name == newPlaylistName).FirstOrDefaultAsync();
            trackData.Playlists.Add(favoritePlaylist);
            _dbContext.Tracks.Update(trackData);

            UserPlaylist userPlaylist = new UserPlaylist()
            {
                UserId = currentUserId,
                PlaylistId = playlist.PlaylistId
            };
            _dbContext.UserPlaylists.Add(userPlaylist);
            await _dbContext.SaveChangesAsync();

        }

        public async Task RemoveTrackFromPlaylist(long trackId, string playlistName)
        {
            Track track = await _dbContext.Tracks.Where(t => t.TrackId == trackId).Include(a => a.Playlists).FirstOrDefaultAsync();
            Models.Playlist playList = await _dbContext.Playlists.Where(p => p.Name == playlistName).FirstOrDefaultAsync();

            track.Playlists.Remove(playList);
            _dbContext.Tracks.Update(track);
            await _dbContext.SaveChangesAsync();
        }

        public async Task AddTrackToPlaylist(PlaylistTrack selectedTrack, string playlistName)
        {
            var trackData = await _dbContext.Tracks.Where(t => t.TrackId == selectedTrack.TrackId).Include(a => a.Playlists).FirstOrDefaultAsync();
            var playList = await _dbContext.Playlists.Where(p => p.Name == playlistName).FirstOrDefaultAsync();
            trackData.Playlists.Add(playList);
            _dbContext.Tracks.Update(trackData);
            await _dbContext.SaveChangesAsync();
        }

    }
}
