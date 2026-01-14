using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileDowloader.Pause
{
    public class PauseClass
    {
        private ManualResetEventSlim manualReset = new ManualResetEventSlim(true);
        private readonly ILogger _logger;
        private readonly CancellationTokenSource _cts = new CancellationTokenSource();
        private CancellationToken _cancellationToken => _cts.Token;
        public void Pause()
        {
            manualReset.Reset();
            _logger.LogInformation("Загрузки проастановлены");
        }

        public void Resume()
        { 
            manualReset.Set();
            _logger.LogInformation($"Загрузки возобнавлены");
            
        }

        private async Task workerLoop()
        {
            while (!_cancellationToken.IsCancellationRequested)
            {
                manualReset.Wait(_cancellationToken);
            }

        }
    }
}
