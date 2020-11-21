namespace Vrnz2.Pinger.Shared.Models
{
    public class QueueMessage
    {        
        public string ServiceId { get; set; }
        public string MessageId { get; set; }
        public double EventUnixTimestamp { get; set; }
        public string Message { get; set; }
    }
}
