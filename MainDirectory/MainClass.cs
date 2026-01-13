using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileDowloader.MainDirectory
{
    public class MainClass
    {
        public void MutexMethod()
        {
            bool createdNew;

            var mutex = new Mutex(initiallyOwned: true,
                name: "MyAppNewMutex",
                createdNew: out createdNew
                );

            if (!createdNew)
            {
                Console.WriteLine("Приложение уже запущено");

                mutex.Dispose();

                return;
            }

            try
            {
                Console.WriteLine("Приложение запущено");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Возникло исключение" + ex.Message);
                return;
            }
            finally
            { 
                mutex.ReleaseMutex();
                mutex.Dispose();
            }
        }
    }
}
