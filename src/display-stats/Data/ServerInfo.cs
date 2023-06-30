namespace display_stats.Data
{
    public class ServerInfo
    {
        public string Name { get; private set; }
        public string DisplayName { get; private set; }
        public bool HasSlurm { get; private set; }

        public ServerInfo(IConfigurationSection details)
        {
            Name = details.Key;
            DisplayName = details.GetValue<string>("displayname");
            DisplayName = details.GetValue<string>("displayname");
            HasSlurm = details.GetValue<bool>("has_slurm");
        }
    }
}
