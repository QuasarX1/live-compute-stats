namespace display_stats.Data
{
    public class ServerInfo
    {
        public string Name { get; private set; }
        public string DisplayName { get; private set; }
        public bool HasSlurm { get; private set; }
        public SlurmQueueColours? QueueColours { get; private set; } = null;

        public ServerInfo(IConfigurationSection details)
        {
            Name = details.Key;
            DisplayName = details.GetValue<string>("displayname");
            //DisplayName = details.GetValue<string>("displayname");
            HasSlurm = details.GetValue<bool>("has_slurm");

            if (HasSlurm)
            {
                IConfigurationSection colours_section = details.GetSection("queue_colours");
                QueueColours = new SlurmQueueColours((from v in colours_section.GetChildren() select v.Key).ToArray(),
                                                     (from v in colours_section.GetChildren() select v.Value).ToArray());
            }
        }
    }
}
