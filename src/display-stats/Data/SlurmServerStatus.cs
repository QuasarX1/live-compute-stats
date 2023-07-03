namespace display_stats.Data
{
    public class SlurmServerStatus : ServerStatus
    {
        public SlurmQueueColours QueueColours { get; private set; }
        private Dictionary<uint, SlurmJobInfo> jobs = new Dictionary<uint, SlurmJobInfo>();

        public SlurmServerStatus() : base()
        {
            QueueColours = new SlurmQueueColours();
        }
        public new static SlurmServerStatus Offline() { return new SlurmServerStatus(); }

        public SlurmServerStatus(string test) : base(true)
        {
            QueueColours = new SlurmQueueColours();
        }

        public SlurmJobInfo GetJob(uint job_id)
        {
            return jobs[job_id];
        }
    }
}
