using Chinook.ClientModels;
using Chinook.Models;
using NuGet.DependencyResolver;

namespace Chinook.Services.Playlist
{
    public interface IPlaylistService
    {
        Task<Track> AddTrackToFavorites(long trackId);
        Task<Track> RemoveTrackFromFavorites(long trackId);
        Task<List<PlaylistTrack>> RetrieveTracksList(long artistId, string currentUserId);
        Task<List<Models.Playlist>> RetrieveUsersListofPlaylist(string currentUserId);
        Task AddNewPlaylist(PlaylistTrack selectedTrack, string newPlaylistName, string currentUserId);
        Task RemoveTrackFromPlaylist(long trackId, string playlistName);
        Task AddTrackToPlaylist(PlaylistTrack selectedTrack, string newPlaylistName);
    }
}
