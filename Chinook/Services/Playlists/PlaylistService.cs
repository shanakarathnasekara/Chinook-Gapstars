using Chinook.ClientModels;
using Chinook.Models;
using Microsoft.EntityFrameworkCore;

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
            var favoritePlaylist = await _dbContext.Playlists.FirstOrDefaultAsync(p => p.Name == "Favorites");
            if (favoritePlaylist == null)
            {
                return null;
            }

            var track = await _dbContext.Tracks.FirstOrDefaultAsync(t => t.TrackId == trackId);
            if (track != null)
            {
                track.Playlists.Add(favoritePlaylist);
                await _dbContext.SaveChangesAsync();
            }

            return track;
        }

        public async Task<Track> RemoveTrackFromFavorites(long trackId)
        {
            var favoritePlaylist = await _dbContext.Playlists.FirstOrDefaultAsync(p => p.Name == "Favorites");
            if (favoritePlaylist == null)
            {
                // Handle the case when the "Favorites" playlist is not found
                return null; // Or throw an exception, return a default value, etc.
            }

            var track = await _dbContext.Tracks.Include(a => a.Playlists).FirstOrDefaultAsync(t => t.TrackId == trackId);
            if (track != null)
            {
                track.Playlists.Remove(favoritePlaylist);
                await _dbContext.SaveChangesAsync();
            }

            return track;
        }

        public async Task<ClientModels.Playlist> RetrieveSpecificPlaylist(long playlistId, string currentUserId)
        {
            var playlist = await _dbContext.Playlists
                .Include(p => p.Tracks)
                .ThenInclude(t => t.Album)
                .ThenInclude(a => a.Artist)
                .Where(p => p.PlaylistId == playlistId)
                .Select(p => new ClientModels.Playlist
                {
                    Name = p.Name,
                    Tracks = p.Tracks.Select(t => new ClientModels.PlaylistTrack
                    {
                        AlbumTitle = t.Album.Title,
                        ArtistName = t.Album.Artist.Name,
                        TrackId = t.TrackId,
                        TrackName = t.Name,
                        IsFavorite = t.Playlists
                            .Any(pl => pl.UserPlaylists
                            .Any(up => up.UserId == currentUserId && up.Playlist.Name == "Favorites"))
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

            this.RemovePlaylist(23, "19bb6f74-d083-498f-a188-d1cd6e7979f0");
        }

        public async Task AddTrackToPlaylist(PlaylistTrack selectedTrack, string playlistName)
        {
            var trackData = await _dbContext.Tracks.Where(t => t.TrackId == selectedTrack.TrackId).Include(a => a.Playlists).FirstOrDefaultAsync();
            var playList = await _dbContext.Playlists.Where(p => p.Name == playlistName).FirstOrDefaultAsync();
            trackData.Playlists.Add(playList);
            _dbContext.Tracks.Update(trackData);
            await _dbContext.SaveChangesAsync();
        }

        private async Task RemovePlaylist(long playlistId, string currentuserId)
        {
            var playlist = await _dbContext.Playlists.Where(p => p.PlaylistId == playlistId).FirstOrDefaultAsync();
            _dbContext.Playlists.Remove(playlist);
            await _dbContext.SaveChangesAsync();

            var userPlaylist = await _dbContext.UserPlaylists.Where(p => p.PlaylistId == playlistId).FirstOrDefaultAsync();
            _dbContext.UserPlaylists.Remove(userPlaylist);
            await _dbContext.SaveChangesAsync();
        }

        private async Task AddFavoritesPlaylist()
        {
            var favoritePlaylist = new Chinook.Models.Playlist()
            {
                PlaylistId = 19,
                Name = "Favorites"
            };
            _dbContext.Playlists.Add(favoritePlaylist);
            await _dbContext.SaveChangesAsync();
        }

        private async Task AddUserToPlaylist(string currentUserId)
        {
            UserPlaylist userPlaylist = new UserPlaylist()
            {
                UserId = currentUserId,
                PlaylistId = 15
            };
            _dbContext.UserPlaylists.Add(userPlaylist);
            await _dbContext.SaveChangesAsync();
        }
    }
}
