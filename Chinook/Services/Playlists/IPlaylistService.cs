using Chinook.ClientModels;
using Chinook.Models;
using NuGet.DependencyResolver;

namespace Chinook.Services.Playlist
{
    public interface IPlaylistService
    {
        Task InitiatingFavoritePlaylist(string currentUserId);
        Task<Track> AddTrackToFavorites(long trackId, string currentUserId);
        Task<Track> RemoveTrackFromFavorites(long trackId, string currentUserId);
        Task<ClientModels.Playlist> RetrieveSpecificPlaylist(long playlistId, string currentUserId);
        Task<List<ClientModels.Playlist>> RetrieveUsersListofPlaylist(string currentUserId);
        Task AddNewPlaylist(PlaylistTrack selectedTrack, string newPlaylistName, string currentUserId);
        Task RemoveTrackFromPlaylist(long trackId, string playlistName, string currentUserId);
        Task AddTrackToPlaylist(PlaylistTrack selectedTrack, string newPlaylistName, string currentUserId);
        Task<long> RetrieveUsersFavoritePlaylistId(string currentUserId);
        Task<List<PlaylistNavitem>> RetrieveUsersListPlaylistNavitems(string currentUserId);
    }
}
