﻿namespace Vrnz2.Pinger.Crosscutting.Settings
{
    public class AwsSqsSettings
    {
        public string Region { get; set; }
        public string QueueUrl { get; set; }
        public string AccessKey { get; set; }
        public string SecretKey { get; set; }
    }
}
