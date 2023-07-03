namespace display_stats.Data
{
    public struct SlurmJobInfo
    {
        public uint JobID;
        public string Username;
        public string JobName;
        public string QueueName;
        public DateTime MaxTime;
        public bool IsStarted;
        public DateTime? Elapsed;
        public string[] NodeList;
    }
}
