using Chinook.ClientModels;
using Chinook.Models;
using Chinook.Pages;
using Chinook.Services.EventsStreaming;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace Chinook.Services.Playlist
{
    public class PlaylistService: IPlaylistService
    {
        private ChinookContext _dbContext;

        public PlaylistService(ChinookContext dbContext) 
        {
            this._dbContext = dbContext;
        }

        #region Public methods

        public async Task InitiatingFavoritePlaylist(string currentUserId)
        {
            var userWithFavoritePlaylist = await _dbContext.UserPlaylists.Include(u => u.Playlist).FirstOrDefaultAsync(p => p.UserId == currentUserId && p.Playlist.Name == "Favorites");
            if (userWithFavoritePlaylist == null)
            {
                var favPlayList = await AddFavoritesPlaylist();
                await AddUserToPlaylist(favPlayList.PlaylistId, currentUserId);
            }
        }
        public async Task<Track> AddTrackToFavorites(long trackId, string currentUserId)
        {
            var favoritePlaylist = await _dbContext.Playlists.Include(p => p.UserPlaylists).FirstOrDefaultAsync(p => p.Name == "Favorites" && p.UserPlaylists.Any(u => u.UserId == currentUserId));
            var track = await _dbContext.Tracks.Include(t => t.Playlists).FirstOrDefaultAsync(t => t.TrackId == trackId);
            if (track != null)
            {
                track.Playlists.Add(favoritePlaylist);
                _dbContext.Tracks.Update(track);
                await _dbContext.SaveChangesAsync();
            }

            return track;
        }

        public async Task<Track> RemoveTrackFromFavorites(long trackId, string currentUserId)
        {
            var favoritePlaylist = await _dbContext.Playlists.Include(p => p.UserPlaylists).FirstOrDefaultAsync(p => p.Name == "Favorites" && p.UserPlaylists.Any(u => u.UserId == currentUserId));
            if (favoritePlaylist == null)
            {
                return null;
            }

            var track = await _dbContext.Tracks.Include(a => a.Playlists).FirstOrDefaultAsync(t => t.TrackId == trackId);
            if (track != null)
            {
                track.Playlists.Remove(favoritePlaylist);
                _dbContext.Tracks.Update(track);
                await _dbContext.SaveChangesAsync();
            }

            return track;
        }

        public async Task<ClientModels.Playlist> RetrieveSpecificPlaylist(long playlistId, string currentUserId)
        {
            var playlist = await _dbContext.Playlists
            .Include(a => a.UserPlaylists).Include(a => a.Tracks).ThenInclude(a => a.Album).ThenInclude(a => a.Artist)
            .Where(p => p.PlaylistId == playlistId)
            .Select(p => new ClientModels.Playlist()
            {
                Name = p.Name,
                Tracks = p.Tracks.Select(t => new ClientModels.PlaylistTrack()
                {
                    AlbumTitle = t.Album.Title,
                    ArtistName = t.Album.Artist.Name,
                    TrackId = t.TrackId,
                    TrackName = t.Name,
                    IsFavorite = t.Playlists.Where(p => p.UserPlaylists.Any(up => up.UserId == currentUserId && up.Playlist.Name == "Favorites")).Any()
                }).ToList()
            })
            .FirstOrDefaultAsync();

            return playlist;
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

            await _dbContext.Playlists.AddAsync(playlist);
            await _dbContext.SaveChangesAsync();

            UserPlaylist userPlaylist = new UserPlaylist()
            {
                UserId = currentUserId,
                PlaylistId = playlist.PlaylistId
            };
            await _dbContext.UserPlaylists.AddAsync(userPlaylist);
            await _dbContext.SaveChangesAsync();


            var track = await _dbContext.Tracks.Where(t => t.TrackId == selectedTrack.TrackId).Include(a => a.Playlists).FirstOrDefaultAsync();
            var newPlayList = await _dbContext.Playlists.Include(p => p.UserPlaylists).FirstOrDefaultAsync(p => p.Name == newPlaylistName && p.UserPlaylists.Any(u => u.UserId == currentUserId));
            track.Playlists.Add(newPlayList);
            _dbContext.Tracks.Update(track);
            await _dbContext.SaveChangesAsync();
        }

        public async Task RemoveTrackFromPlaylist(long trackId, string playlistName, string currentUserId)
        {
            Track track = await _dbContext.Tracks.Where(t => t.TrackId == trackId).Include(a => a.Playlists).FirstOrDefaultAsync();
            Models.Playlist playList = await _dbContext.Playlists.Include(p => p.UserPlaylists).FirstOrDefaultAsync(p => p.Name == playlistName && p.UserPlaylists.Any(u => u.UserId == currentUserId));

            track.Playlists.Remove(playList);
            _dbContext.Tracks.Update(track);
            await _dbContext.SaveChangesAsync();

            //this.DeletePlaylist(23, "19bb6f74-d083-498f-a188-d1cd6e7979f0");
        }

        public async Task AddTrackToPlaylist(PlaylistTrack selectedTrack, string playlistName, string currentUserId)
        {
            var trackData = await _dbContext.Tracks.Where(t => t.TrackId == selectedTrack.TrackId).Include(a => a.Playlists).FirstOrDefaultAsync();
            Models.Playlist playlist = await _dbContext.Playlists.Include(p => p.UserPlaylists).FirstOrDefaultAsync(p => p.Name == playlistName && p.UserPlaylists.Any(u => u.UserId == currentUserId));
            trackData.Playlists.Add(playlist);
            _dbContext.Tracks.Update(trackData);
            await _dbContext.SaveChangesAsync();
        }

        public async Task<long> RetrieveUsersFavoritePlaylistId(string currentUserId)
        {
            var favoritePlaylist = await _dbContext.Playlists.Include(p => p.UserPlaylists).FirstOrDefaultAsync(p => p.Name == "Favorites" && p.UserPlaylists.Any(u => u.UserId == currentUserId));
            if (favoritePlaylist == null)
            {
                return 0;
            }

            return favoritePlaylist.PlaylistId;
        }
        #endregion

        #region Private methods
        // In the initial phase if favorites playlist doesnot exist
        private async Task<Models.Playlist> AddFavoritesPlaylist()
        {

            Models.Playlist favorites = new Models.Playlist()
            {
                Name = "Favorites",
                PlaylistId = _dbContext.Playlists.OrderBy(a => a.PlaylistId).Last().PlaylistId + 1
            };
            _dbContext.Playlists.Add(favorites);
            await _dbContext.SaveChangesAsync();
            return favorites;
        }

        // In the initial phase if favorites playlist doesnot exist
        private async Task AddUserToPlaylist(long playlistId, string currentUserId)
        {

            Models.UserPlaylist favorites = new Models.UserPlaylist()
            {
                UserId = currentUserId,
                PlaylistId = playlistId
            };
            _dbContext.UserPlaylists.Add(favorites);
            await _dbContext.SaveChangesAsync();
        }


        // In the future if needed the support to delete playlist
        private async Task DeletePlaylist(long playlistId, string currentuserId)
        {
            var playlist = await _dbContext.Playlists.Where(p => p.PlaylistId == playlistId).FirstOrDefaultAsync();
            _dbContext.Playlists.Remove(playlist);
            await _dbContext.SaveChangesAsync();

            var userPlaylist = await _dbContext.UserPlaylists.Where(p => p.PlaylistId == playlistId).FirstOrDefaultAsync();
            _dbContext.UserPlaylists.Remove(userPlaylist);
            await _dbContext.SaveChangesAsync();
        }
        #endregion
    }
}
