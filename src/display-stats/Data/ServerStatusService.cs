using Microsoft.VisualBasic;
using Renci.SshNet;
using Renci.SshNet.Security;
using System.Text;
using System.Threading;

namespace display_stats.Data
{
    public class ServerStatusService
    {
        public bool DataAvalible { get; private set; } = false;

        private Thread? runner;
        private bool running => (runner is null) ? false : runner.IsAlive;
        private CancellationTokenSource? cancellationTokenSource;
        private CancellationToken? cancellationToken;

        private Dictionary<string, ServerInfo> servers = new Dictionary<string, ServerInfo>();
        private Dictionary<string, Renci.SshNet.ConnectionInfo> server_connections = new Dictionary<string, Renci.SshNet.ConnectionInfo>();
        private TimeSpan poll_interval;
        private Dictionary<string, ServerStatus> statuses = new Dictionary<string, ServerStatus>();

        public string[] ServerNames => servers.Keys.ToArray();
        public string[] ServerDisplayNames => (from key in ServerNames select servers[key].DisplayName).ToArray();

        public ServerStatusService(IConfiguration configuration)
        {
            poll_interval = TimeSpan.FromMinutes(configuration.GetValue<float>("StatusPollIntervalMinutes"));
            IConfigurationSection connection_configs = configuration.GetSection("server_connections");
            foreach (IConfigurationSection server_connection_config in connection_configs.GetChildren())
            {
                servers.Add(server_connection_config.Key, new ServerInfo(server_connection_config));

                server_connections.Add(server_connection_config.Key,
                                       ((from child_item in server_connection_config.GetChildren() select child_item.Key).Contains("absolute_key_path")) ?
                                           new PrivateKeyConnectionInfo(server_connection_config.GetValue<string>("hostname"),
                                                                        server_connection_config.GetValue<string>("username"),
                                                                        new PrivateKeyFile(server_connection_config.GetValue<string>("absolute_key_path")))
                                           :
                                           new PasswordConnectionInfo(server_connection_config.GetValue<string>("hostname"),
                                                                      server_connection_config.GetValue<string>("username"),
                                                                      server_connection_config.GetValue<string>("password"))
                                       );
            }



            start().Wait();
        }

        ~ServerStatusService()
        {
            cancellationTokenSource?.Cancel();
        }

        private async Task start()
        {
            if (!running)
            {
                cancellationTokenSource = new CancellationTokenSource();
                cancellationToken = cancellationTokenSource.Token;
                runner = new Thread(new ThreadStart(run_loop));
                runner.Start();
                await Task.Delay(1000);
            }
        }

        private void run_loop()
        {
            if (cancellationToken is null)
            {
                throw new Exception();
            }

            update(cancellationToken.Value);
            DataAvalible = true;
            while (!cancellationToken.Value.IsCancellationRequested)
            {
                update(cancellationToken.Value);
                Task.Delay(poll_interval, cancellationToken.Value).Wait();
            }
        }

        private void update(CancellationToken cancellationToken)
        {
            Dictionary<string, ServerStatus>  new_statuses = new Dictionary<string, ServerStatus>();
            foreach (string key in servers.Keys)
            {
                try
                {
                    SshClient client = new SshClient(server_connections[key]);
                    client.Connect();
                    new_statuses.Add(key, create_status(client));
                    client.Disconnect();
                    client.Dispose();
                }
                catch (Exception)
                {
                    new_statuses.Add(key, ServerStatus.Offline());
                }
            }
            statuses = new_statuses;
        }

        private ServerStatus create_status(SshClient client)
        {
            SshCommand command = client.CreateCommand("ls");
            string result = command.Execute();
            command.Dispose();
            return new ServerStatus(result);
        }

        public ServerInfo GetServerInfo(string server_name)
        {
            return servers[server_name];
        }

        public async Task<ServerStatus> GetServerStatus(string server_name)
        {
            if (!DataAvalible)
            {
                if (!running)
                {
                    await start();
                    if (!running)
                    {
                        return ServerStatus.Offline();
                    }
                }

                await Task.Delay(1000);
            }

            return statuses[server_name];
        }
    }
}
