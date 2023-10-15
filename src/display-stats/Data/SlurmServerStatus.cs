namespace display_stats.Data
{
    public class SlurmServerStatus : ServerStatus
    {
        public SlurmQueueColours QueueColours { get; private set; }
        private Dictionary<uint, SlurmJobInfo> jobs = new Dictionary<uint, SlurmJobInfo>();

        public uint[] Jobs => jobs.Keys.ToArray();
        public uint NumberOfNodes_Total { get; private set; }
        public uint NumberOfNodes_InUse { get; private set; }
        public uint NumberOfNodes_Draining { get; private set; }

        public string[] TrackedUsernames { get; private set; }

        public SlurmServerStatus() : base()
        {
            QueueColours = new SlurmQueueColours();
            TrackedUsernames = new string[0];
        }
        public new static SlurmServerStatus Offline() { return new SlurmServerStatus(); }

        public SlurmServerStatus(string[] displaynames, SlurmQueueColours queue_colours, uint all_nodes, uint nodes_in_use, uint draining_nodes) : base(true)
        {
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
    }
}
