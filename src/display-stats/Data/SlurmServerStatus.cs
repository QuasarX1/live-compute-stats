namespace display_stats.Data
{
    public class SlurmServerStatus : ServerStatus
    {
        public SlurmQueueColours QueueColours { get; private set; }
        private Dictionary<uint, SlurmJobInfo> jobs = new Dictionary<uint, SlurmJobInfo>();

        public readonly DateTime UpdateTimestamp;

        public uint[] Jobs => jobs.Keys.ToArray();
        public uint NumberOfNodes_Total { get; private set; }
        public uint NumberOfNodes_InUse { get; private set; }
        public uint NumberOfNodes_Draining { get; private set; }

        public string[] TrackedUsernames { get; private set; }

        public SlurmServerStatus() : base()
        {
            QueueColours = new SlurmQueueColours();
            TrackedUsernames = new string[0];
            UpdateTimestamp = DateTime.Now;
        }
        public new static SlurmServerStatus Offline() { return new SlurmServerStatus(); }

        public SlurmServerStatus(DateTime timestamp, string[] displaynames, SlurmQueueColours queue_colours, uint all_nodes, uint nodes_in_use, uint draining_nodes) : base(true)
        {
            UpdateTimestamp = timestamp;
            QueueColours = queue_colours;
            TrackedUsernames = displaynames;
            NumberOfNodes_Total = all_nodes;
            NumberOfNodes_InUse = nodes_in_use;
            NumberOfNodes_Draining = draining_nodes;
        }

        public SlurmJobInfo GetJob(uint job_id)
        {
            return jobs[job_id];
        }

        public void AddJob(SlurmJobInfo job)
        {
            jobs.Add(job.JobID, job);
        }
    }
}
