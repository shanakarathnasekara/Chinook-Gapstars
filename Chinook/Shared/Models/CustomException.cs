namespace Chinook.Shared.Models
{
    public class CustomException : Exception
    {
        public string CustomMessage { get; set; }
    }
}
