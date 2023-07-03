namespace display_stats.Data
{
    public class RegularServerStatus : ServerStatus
    {
        public float TotalCPU { get; private set; }
        public float TotalMemory { get; private set; }

        public string[] ActiveTrackedUsernames { get; private set; }
        public float[] UserCPU { get; private set; }
        public float[] UserMemory { get; private set; }

        public RegularServerStatus() : base()
        {
            TotalCPU = 0;
            TotalMemory = 0;
            ActiveTrackedUsernames = new string[0];
            UserCPU = new float[0];
            UserMemory = new float[0];
        }
        public new static RegularServerStatus Offline() { return new RegularServerStatus(); }

        public RegularServerStatus(float cpu_usage_percentage, float memory_used, float memory_max, Dictionary<string, float[]> user_top_data, string[]? displaynames = null) : base(true)
        {
            TotalCPU = cpu_usage_percentage;
            TotalMemory = 100.0f * memory_used / memory_max;
            ActiveTrackedUsernames = (displaynames is not null) ? displaynames : user_top_data.Keys.ToArray();
            UserCPU = (from pair in user_top_data.Values select pair[0]).ToArray();
            UserMemory = (from pair in user_top_data.Values select pair[1]).ToArray();
        }
    }
}
