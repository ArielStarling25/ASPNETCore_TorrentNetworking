namespace WebServer.Models.DataModels
{
    public class JobPost
    {
        public int? JobId { get; set; }
        public int? FromClient { get; set; }
        public int? ToClient { get; set; }
        public string? Job { get; set; }
        public int? JobSuccess { get; set; }
        public string? JobResult { get; set; }
    }
}
