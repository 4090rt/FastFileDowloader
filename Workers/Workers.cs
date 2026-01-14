using FileDowloader.Pause;
using FileDowloader.Request;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FileDowloader
{
    public class Workers
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<Requestt> _logger;
        private readonly Queue<string> List = new Queue<string>();
        object Lock = new object();
        private bool _isRunning = false;
        private readonly List<Task> _workerTasks = new List<Task>();
        private readonly CancellationTokenSource _cts = new CancellationTokenSource();
        private readonly PauseClass _pauseclass;
        private ManualResetEventSlim manualReset = new ManualResetEventSlim(true);

        private CancellationToken _cancellationToken => _cts.Token;

        public void AddURL(string url)
        {
            try
            {
                manualReset.Wait(_cancellationToken);

                lock (Lock)
                {
                    while (List.Count >= 10 && !_cancellationToken.IsCancellationRequested)
                    {
                        bool valid = Monitor.Wait(Lock, 5000);

                        if (!valid)
                        {
                            return;
                        }
                    }

                    if (_cancellationToken.IsCancellationRequested)
                    {
                        return;
                    }

                    List.Enqueue(url);
                    Monitor.PulseAll(Lock);
                }
            }
            catch(Exception ex) 
            {
            
            }
        }


        public async Task WorkMerhod1()
        {
            try
            {
                while (!_cancellationToken.IsCancellationRequested) 
                {
                    manualReset.Wait();
                    string url = null;

                    if (_cancellationToken.IsCancellationRequested)
                        break;

                    lock (Lock)
                    {
                        while (List.Count == 0 && !_cancellationToken.IsCancellationRequested)
                        {
                            var valid = Monitor.Wait(Lock, 1000);

                            if (!valid)
                            {
                                continue;
                            }
                        }
                        if (List.Count > 0)
                        {
                            url = List.Dequeue();

                            Monitor.Pulse(Lock);
                        }
                    }

                    if (url != null)
                    {
                        Requestt request = new Requestt(_httpClientFactory, _logger);
                        await request.Semaphore(url, _cancellationToken);
                    }
                }
            }
            catch (Exception ex)
            { 
            
            }
        }

        public void Start(int workerCount = 2)
        {
            if (_isRunning)
                return;

            _isRunning = true;

            for (int i = 0; i < workerCount; i++)
            {
                var task = Task.Run(() => WorkMerhod1());
                _workerTasks.Add(task);
            }
        }

        public async Task StopAsync()
        {
            _cts.Cancel();
            await Task.WhenAll(_workerTasks);
            _isRunning = false;
            Console.WriteLine("Менеджер остановлен");
        }
    }
}
