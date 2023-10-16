namespace display_stats.Data
{
    public struct SlurmJobInfo
    {
        public uint JobID;
        public string Username;
        public string JobName;
        public string QueueName;
        public TimeSpan MaxTime;
        public bool IsStarted;
        public TimeSpan Elapsed;
        public uint Nodes;
    }
}
