using Microsoft.AspNetCore.Hosting.Server;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;

namespace display_stats.Data
{
    public class TestDataService
    {
        private TestData currentValue = new TestData();
        private Thread runner;
        private CancellationTokenSource cancellationTokenSource;
        private CancellationToken cancellationToken;

        public TestDataService()
        {
            cancellationTokenSource = new CancellationTokenSource();
            cancellationToken = cancellationTokenSource.Token;
            runner = new Thread(new ThreadStart(run));
            runner.Start();
        }

        ~TestDataService()
        {
            cancellationTokenSource.Cancel();
        }

        private void run()
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                currentValue = new TestData();
                Thread.Sleep(5000);
            }
        }

        public TestData GetData()
        {
            return currentValue;
        }
    }
}