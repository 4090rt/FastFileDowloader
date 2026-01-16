using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileDowloader.MainDirectory
{
    public class MainClass
    {
        private readonly ILogger<MainClass> _logger;

        public  MainClass(ILogger<MainClass> logger)
        {
            _logger = logger;
        }

        public async Task<bool> MutexMethod()
        {
            bool createdNew;

            var mutex = new Mutex(initiallyOwned: true,
                name: "MyAppNewMutex",
                createdNew: out createdNew
                );

            if (!createdNew)
            {
               _logger.LogWarning("Приложение уже запущено");

                mutex.Dispose();

                return false;
            }

            try
            {
                _logger.LogWarning("Приложение запущено");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError("Возникло исключение" + ex.Message);
                return false;
            }
            finally
            { 
                mutex.ReleaseMutex();
                mutex.Dispose();
            }
        }
    }
}
