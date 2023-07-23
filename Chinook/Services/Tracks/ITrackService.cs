using Chinook.ClientModels;

namespace Chinook.Services.Tracks
{
    public interface ITrackService
    {
        Task<List<PlaylistTrack>> RetrieveTracksList(long artistId, string currentUserId);
    }
}
