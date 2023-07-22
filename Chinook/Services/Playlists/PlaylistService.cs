using Chinook.ClientModels;
using Chinook.Common.Models;
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

        // Method to create a favourite playlist by default
        public async Task InitiatingFavoritePlaylist(string currentUserId)
        {
            var userWithFavoritePlaylist = await _dbContext.UserPlaylists.Include(u => u.Playlist).FirstOrDefaultAsync(p => p.UserId == currentUserId && p.Playlist.Name == "Favorites");
            if (userWithFavoritePlaylist == null)
            {
                var favPlayList = await AddFavoritesPlaylist();
                await AddUserToPlaylist(favPlayList.PlaylistId, currentUserId);
            }
        }

        // Adding tracks to favorite playlist
        public async Task<Track> AddTrackToFavorites(long trackId, string currentUserId)
        {
            var favoritePlaylist = await _dbContext.Playlists.Include(p => p.UserPlaylists).FirstOrDefaultAsync(p => p.Name == "Favorites" && p.UserPlaylists.Any(u => u.UserId == currentUserId));
            var track = await _dbContext.Tracks
                .Include(t => t.Playlists)
                .Include(t => t.Album).ThenInclude(a => a.Artist)
                .FirstOrDefaultAsync(t => t.TrackId == trackId);

            if (track == null || favoritePlaylist == null)
            {
                throw new CustomException
                {
                    CustomMessage = "Error occurred when marking the track as favorite"
                };
            }

            if (track != null)
            {
                track.Playlists.Add(favoritePlaylist);
                _dbContext.Tracks.Update(track);
                await _dbContext.SaveChangesAsync();
            } 
            return track;
        }

        // Remove tracks from favorite playlist
        public async Task<Track> RemoveTrackFromFavorites(long trackId, string currentUserId)
        {
            var favoritePlaylist = await _dbContext.Playlists.Include(p => p.UserPlaylists).FirstOrDefaultAsync(p => p.Name == "Favorites" && p.UserPlaylists.Any(u => u.UserId == currentUserId));
            var track = await _dbContext.Tracks.Include(a => a.Playlists).FirstOrDefaultAsync(t => t.TrackId == trackId);

            if (favoritePlaylist == null || track == null)
            {
                throw new CustomException
                {
                    CustomMessage = "Error occurred when marking the track as unfavorite"
                };
            }

            if (track != null)
            {
                track.Playlists.Remove(favoritePlaylist);
                _dbContext.Tracks.Update(track);
                await _dbContext.SaveChangesAsync();
            }

            return track;
        }

        // Retrieve specific playlist data
        public async Task<ClientModels.Playlist> RetrieveSpecificPlaylist(long playlistId, string currentUserId)
        {
            var playlist = await _dbContext.Playlists
            .Include(a => a.UserPlaylists).Include(a => a.Tracks).ThenInclude(a => a.Album).ThenInclude(a => a.Artist)
            .Where(p => p.PlaylistId == playlistId)
            .Select(p => new ClientModels.Playlist()
            {
                Name = p.Name,
                Tracks = p.Tracks.Select(t => new PlaylistTrack()
                {
                    AlbumTitle = t.Album.Title,
                    ArtistName = t.Album.Artist.Name,
                    TrackId = t.TrackId,
                    TrackName = t.Name,
                    IsFavorite = t.Playlists.Where(p => p.UserPlaylists.Any(up => up.UserId == currentUserId && up.Playlist.Name == "Favorites")).Any()
                }).ToList()
            })
            .FirstOrDefaultAsync();

            if (playlist == null)
            {
                throw new CustomException
                {
                    CustomMessage = "Error occurred when retrieving playlist data"
                };
            }

            return playlist;
        }

        // Retrieve tracks data
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

            if (tracks == null)
            {
                throw new CustomException
                {
                    CustomMessage = "Error occurred when retrieving tracks"
                };
            }

            return tracks;
        }

        // Retrieve user specific list of playlists
        public async Task<List<ClientModels.Playlist>> RetrieveUsersListofPlaylist(string currentUserId)
        {
            var userPlaylists = await _dbContext.Playlists
                .Include(p => p.UserPlaylists)
                .Where(p => p.UserPlaylists.Any(up => up.UserId == currentUserId && up.Playlist.Name != "Favorites"))
                .Select(p => new ClientModels.Playlist()
                {
                    Name = p.Name                    
                })
                .ToListAsync();
            return userPlaylists;
        }

        // Retrieve user specific playlist navigation items list
        public async Task<List<PlaylistNavitem>> RetrieveUsersListPlaylistNavitems(string currentUserId)
        {
            var userPlaylists = await _dbContext.Playlists
                .Include(p => p.UserPlaylists)
                .Where(p => p.UserPlaylists.Any(up => up.UserId == currentUserId && up.Playlist.Name != "Favorites"))
                .Select(p => new PlaylistNavitem()
                {
                    Name = p.Name,
                    PlaylistId = p.PlaylistId
                })
                .ToListAsync();
            return userPlaylists;
        }


        // Add new playlist
        public async Task AddNewPlaylist(PlaylistTrack selectedTrack, string newPlaylistName, string currentUserId)
        {
            await ValidateNewPlaylistName(newPlaylistName, currentUserId);

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

        // Remove tracks from playlist
        public async Task RemoveTrackFromPlaylist(long trackId, string playlistName, string currentUserId)
        {
            Track track = await _dbContext.Tracks.Where(t => t.TrackId == trackId).Include(a => a.Playlists).FirstOrDefaultAsync();
            Models.Playlist playList = await _dbContext.Playlists.Include(p => p.UserPlaylists).FirstOrDefaultAsync(p => p.Name == playlistName && p.UserPlaylists.Any(u => u.UserId == currentUserId));

            track.Playlists.Remove(playList);
            _dbContext.Tracks.Update(track);
            await _dbContext.SaveChangesAsync();
        }

        // Add tracks to playlist
        public async Task AddTrackToPlaylist(PlaylistTrack selectedTrack, string playlistName, string currentUserId)
        {
            var trackData = await _dbContext.Tracks.Where(t => t.TrackId == selectedTrack.TrackId).Include(a => a.Playlists).FirstOrDefaultAsync();
            Models.Playlist playlist = await _dbContext.Playlists.Include(p => p.UserPlaylists).FirstOrDefaultAsync(p => p.Name == playlistName && p.UserPlaylists.Any(u => u.UserId == currentUserId));
            trackData.Playlists.Add(playlist);
            _dbContext.Tracks.Update(trackData);
            await _dbContext.SaveChangesAsync();
        }

        // Retrieve playlist id of user specific favorite playlist
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

            Models.UserPlaylist favorites = new UserPlaylist()
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
        
        private async Task ValidateNewPlaylistName(string playlistName, string currentUserId)
        {
            var duplicatePlaylistName = await _dbContext.Playlists.Include(p => p.UserPlaylists).FirstOrDefaultAsync(p => p.Name == playlistName && p.UserPlaylists.Any(u => u.UserId == currentUserId));

            if (duplicatePlaylistName != null)
            {
                throw new CustomException
                {
                    CustomMessage = "Playlist with the same name exists"
                };
            }

        }
        #endregion
    }
}
