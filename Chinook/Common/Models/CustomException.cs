namespace Chinook.Common.Models
{
    public class CustomException: Exception
    {
        public string CustomMessage { get; set; }
    }
}
