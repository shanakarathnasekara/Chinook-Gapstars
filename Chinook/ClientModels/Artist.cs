using System.Net;

namespace Chinook.ClientModels
{
    public class Artist
    {
        public long ArtistId { get; set; }
        public string? ArtistName { get; set; }
        public List<Album>? Albums { get; set; }
    }
}
