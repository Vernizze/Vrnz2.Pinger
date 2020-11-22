namespace Vrnz2.Pinger.Crosscutting.Shared.Models
{
    public class PingQueueMessage
    {        
        public string ServiceId { get; set; }
        public string MessageId { get; set; }
        public double EventUnixTimestamp { get; set; }
        public string Message { get; set; }
    }
}
