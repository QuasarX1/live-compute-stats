﻿using Microsoft.VisualBasic;
using Renci.SshNet;
using Renci.SshNet.Security;
using System.Linq;
using System.Text;
using System.Threading;

namespace display_stats.Data
{
    public class ServerStatusService
    {
        private TimeSpan ParseSlurmTime(string value)
        {
            string[] sections = value.Split(new char[] { '-', ':' });

            if (sections.Length == 2)
            {
                return TimeSpan.ParseExact(value, "m\\:ss", null);
            }
            else if (sections.Length == 3)
            {
                return TimeSpan.ParseExact(value, "h\\:mm\\:ss", null);
            }
            else if (sections.Length == 4)
            {
                return TimeSpan.ParseExact(value, "d\\-hh\\:mm\\:ss", null);
            }
            else
            {
                throw new ArgumentException("Unrecognised time span format.");
            }
        }

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

        public string[] TrackedUsernames { get; private set; }
        public Dictionary<string, string> TrackedUserDisplayNames { get; private set; } = new Dictionary<string, string>();
        //public string[] TrackedUserDisplayNames { get; private set; }

        public ServerStatusService(IConfiguration configuration)
        {
            poll_interval = TimeSpan.FromMinutes(configuration.GetValue<float>("StatusPollIntervalMinutes"));
            TrackedUsernames = (from child in configuration.GetSection("tracked_user_displaynames").GetChildren() select child.Key).ToArray();
            TrackedUserDisplayNames = new Dictionary<string, string>((from child in configuration.GetSection("tracked_user_displaynames").GetChildren() select KeyValuePair.Create(child.Key, child.Value)));
            //TrackedUserDisplayNames = (Dictionary<string, string>)(from child in configuration.GetSection("tracked_user_displaynames").GetChildren() select KeyValuePair.Create(child.Key, child.Value));
            IConfigurationSection connection_configs = configuration.GetSection("server_connections");
            AuthenticationMethod auth;
            Renci.SshNet.ConnectionInfo connectionInfo;
            foreach (IConfigurationSection server_connection_config in connection_configs.GetChildren())
            {
                servers.Add(server_connection_config.Key, new ServerInfo(server_connection_config));
                //statuses.Add(server_connection_config.Key, ServerStatus.Offline());

                if ((from child_item in server_connection_config.GetChildren() select child_item.Key).Contains("absolute_key_path"))
                {
                    auth = new PrivateKeyAuthenticationMethod(server_connection_config.GetValue<string>("username"),
                                                              new PrivateKeyFile(server_connection_config.GetValue<string>("absolute_key_path")));
                }
                else
                {
                    auth = new PasswordAuthenticationMethod(server_connection_config.GetValue<string>("username"),
                                                            server_connection_config.GetValue<string>("password"));
                }

                if ((from child_item in server_connection_config.GetChildren() select child_item.Key).Contains("port"))
                {
                    connectionInfo = new Renci.SshNet.ConnectionInfo(server_connection_config.GetValue<string>("hostname"),
                                                                     server_connection_config.GetValue<int>("port"),
                                                                     server_connection_config.GetValue<string>("username"),
                                                                     auth);
                }
                else
                {
                    connectionInfo = new Renci.SshNet.ConnectionInfo(server_connection_config.GetValue<string>("hostname"),
                                                                     server_connection_config.GetValue<string>("username"),
                                                                     auth);
                }
                //connectionInfo.MaxSessions = 8;

                server_connections.Add(server_connection_config.Key, connectionInfo);

                //server_connections.Add(server_connection_config.Key,
                //                       ((from child_item in server_connection_config.GetChildren() select child_item.Key).Contains("absolute_key_path")) ?
                //                           new PrivateKeyConnectionInfo(server_connection_config.GetValue<string>("hostname"),
                //                                                        server_connection_config.GetValue<string>("username"),
                //                                                        new PrivateKeyFile(server_connection_config.GetValue<string>("absolute_key_path")))
                //                           :
                //                           new PasswordConnectionInfo(server_connection_config.GetValue<string>("hostname"),
                //                                                      server_connection_config.GetValue<string>("username"),
                //                                                      server_connection_config.GetValue<string>("password"))
                //                       );
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
                    //client.KeepAliveInterval = TimeSpan.FromSeconds(15);//TODO: Move to configuration
                    client.Connect();
                    new_statuses.Add(key, create_status(key, client));
                    client.Disconnect();
                    client.Dispose();
                }
                catch (Exception e)
                {
                    new_statuses.Add(key, (servers[key].HasSlurm) ? SlurmServerStatus.Offline() : RegularServerStatus.Offline());
                }
            }
            statuses = new_statuses;
        }

        private ServerStatus create_status(string server_key, SshClient client)
        {
            ServerStatus new_status;
            string command_result;

            if (servers[server_key].HasSlurm)
            {
                DateTime jobs_timestamp;

                SshCommand command__n_nodes = client.CreateCommand("sinfo -O \"nodes\" --noheader");
                command_result = command__n_nodes.Execute();
                command__n_nodes.Dispose();
                uint number_of_nodes = System.Convert.ToUInt32(command_result);

                SshCommand command__squeue = client.CreateCommand("squeue --noheader --Format=\"JobID:10,UserName:100,Name:100,Partition:20,TimeLimit,State:100,TimeUsed,NumNodes\"|awk '{print $1\"|\"$2\"|\"$3\"|\"$4\"|\"$5\"|\"$6\"|\"$7\"|\"$8}'");
                command_result = command__squeue.Execute();
                jobs_timestamp = DateTime.Now;
                command__squeue.Dispose();

                if (command_result.EndsWith("\n"))
                {
                    command_result = command_result[..^1];
                }
                string[] lines = command_result.Split("\n");
                List<SlurmJobInfo> jobs = new List<SlurmJobInfo>();
                uint n_nodes_in_use = 0;
                uint n_job_nodes;
                bool running;
                for (int i = 0; i < lines.Length; i++)
                {
                    string[] sections = lines[i].Split("|");
                    running = sections[5] == "RUNNING";
                    n_job_nodes = System.Convert.ToUInt32(sections[7]);

                    if (running)
                    {
                        n_nodes_in_use += n_job_nodes;
                    }

                    if (TrackedUsernames.Contains(sections[1]))
                    {
                        jobs.Add(new SlurmJobInfo
                        {
                            JobID = System.Convert.ToUInt32(sections[0]),
                            Username = sections[1],
                            JobName = sections[2],
                            QueueName = sections[3],
                            MaxTime = ParseSlurmTime(sections[4]),
                            IsStarted = running,
                            Elapsed = ParseSlurmTime(sections[6]),
                            Nodes = n_job_nodes
                        });
                    }
                }

                new_status = new SlurmServerStatus(timestamp: jobs_timestamp,
                                                   displaynames: TrackedUsernames,
                                                   queue_colours: servers[server_key].QueueColours,
                                                   all_nodes: number_of_nodes,
                                                   nodes_in_use: n_nodes_in_use,
                                                   draining_nodes: 0);

                foreach (SlurmJobInfo job in jobs)
                {
                    ((SlurmServerStatus)new_status).AddJob(job);
                }
            }
            else
            {
                SshCommand command__number_cpus = client.CreateCommand("nproc --all");
                command_result = command__number_cpus.Execute();
                command__number_cpus.Dispose();
                int number_of_cpus = System.Convert.ToInt32(command_result);

                SshCommand command__cpu_usage = client.CreateCommand("echo $[100 - $(vmstat 1 2|tail -1|awk '{print $15}')]");//TODO: -n -1 ???
                command_result = command__cpu_usage.Execute();
                command__cpu_usage.Dispose();
                float result__cpu_usage_percentage = (float)System.Convert.ToDouble(command_result);

                SshCommand command__max_mem = client.CreateCommand("free -k | head -n 2 | tail -n 1 | awk '{print $2}'");
                command_result = command__max_mem.Execute();
                command__max_mem.Dispose();
                float max_memory = (float)(System.Convert.ToDouble(command_result) / 1024 / 1024);// in GB

                SshCommand command__used_mem = client.CreateCommand("free -k | head -n 2 | tail -n 1 | awk '{print $3}'");
                command_result = command__used_mem.Execute();
                command__used_mem.Dispose();
                float used_memory = (float)(System.Convert.ToDouble(command_result) / 1024 / 1024);// in GB

                SshCommand command__user_processes = client.CreateCommand($"top -b -n 1 -E k | tail -n +8 | awk '{{print $2\"|\"$9\"|\"$10}}'");
                command_result = command__user_processes.Execute();
                command__user_processes.Dispose();
                Dictionary<string, float[]> user_top_data = new Dictionary<string, float[]>();
                float[] empty_array = new float[2] { 0.0f, 0.0f };
                foreach (string username in TrackedUsernames)
                {
                    user_top_data.Add(username, new float[2] { 0.0f, 0.0f });
                }
                string[] lines = command_result.Split("\n");
                foreach (string line in lines)
                {
                    string[] sections = line.Split("|");
                    if (user_top_data.ContainsKey(sections[0]))
                    {
                        user_top_data[sections[0]][0] += (float)(System.Convert.ToDouble(sections[1]));
                        user_top_data[sections[0]][1] += (float)(System.Convert.ToDouble(sections[2]));
                    }
                }
                // Remove users that have no impact
                user_top_data = new Dictionary<string, float[]>((from mapping in user_top_data where mapping.Value != empty_array select mapping));
                foreach (string username in user_top_data.Keys)
                {
                    user_top_data[username][0] /= (float)number_of_cpus;
                }

                new_status = new RegularServerStatus(result__cpu_usage_percentage, used_memory, max_memory, user_top_data, displaynames: TrackedUsernames);
            }

            return new_status;
        }

        public ServerInfo GetServerInfo(string server_name)
        {
            return servers[server_name];
        }

        public async Task<ServerStatus?> GetServerStatus(string server_name)
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

            if (!statuses.ContainsKey(server_name))
            {
                return null;
            }

            return statuses[server_name];
        }
    }
}
