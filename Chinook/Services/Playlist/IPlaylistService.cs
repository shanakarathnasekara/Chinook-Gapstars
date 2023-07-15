using Chinook.ClientModels;
using Chinook.Models;

namespace Chinook.Services.Playlist
{
    public interface IPlaylistService
    {
        Task<Track> AddTrackToFavorites(long trackId);
        Task<Track> RemoveTrackFromFavorites(long trackId);
        Task<List<PlaylistTrack>> RetrieveTracksList(long artistId, string currentUserId);
    }
}
