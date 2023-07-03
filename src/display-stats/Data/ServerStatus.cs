namespace display_stats.Data
{
    public class ServerStatus
    {
        public bool IsOnline { get; private set; }

        public ServerStatus(bool online = false)
        {
            IsOnline = online;
        }

        public static ServerStatus Offline() { return new ServerStatus(); }
    }
}
