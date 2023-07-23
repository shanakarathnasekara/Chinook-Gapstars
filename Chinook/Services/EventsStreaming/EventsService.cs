namespace Chinook.Services.EventsStreaming
{
    public class EventsService
    {
        public event Action PlaylistItemAdded;
        public void UpdatePlaylistItemAddedEvent()
        {
            PlaylistItemAdded?.Invoke();
        }
    }
}
