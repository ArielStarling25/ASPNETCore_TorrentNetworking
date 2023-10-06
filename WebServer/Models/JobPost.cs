namespace WebServer.Models
{
    public class JobPost
    {
        public string? JobId { get; set; }
        public int FromClient { get; set; }
        public int ToClient { get; set; }
        public string? Job { get; set; }
        public bool? JobSuccess { get; set; }
    }
}
