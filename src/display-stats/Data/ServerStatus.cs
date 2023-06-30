namespace display_stats.Data
{
    public class ServerStatus
    {
        public bool IsOnline { get; private set; }

        private ServerStatus()
        {
            IsOnline = false;
        }
        public static ServerStatus Offline() { return new ServerStatus(); }

        public ServerStatus(string test)
        {
            IsOnline = true;
        }
    }
}
