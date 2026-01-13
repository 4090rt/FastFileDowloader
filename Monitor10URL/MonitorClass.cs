using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileDowloader.Monitor10URL
{
    public class MonitorClass
    {
        Queue<string> List = new Queue<string>();
        object Lock = new object();

        public void AddURL()
        {
            try
            {
                Console.WriteLine("Введите url");
                string urlnew = Console.ReadLine();

                lock (Lock)
                {
                    while (List.Count >= 10)
                    {
                        bool valid = Monitor.Wait(Lock, 5000);

                        if (!valid)
                        {
                            return;
                        }
                    }

                    List.Enqueue(urlnew);
                    Monitor.Pulse(Lock);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Возникло исключение" + ex.Message);
            }
        }

        public async Task WorkMerhod() 
        {
            while (true)
            {
                string urlworked;
                try
                {
                    lock (Lock)
                    {
                        while (List.Count == 0)
                        {
                            bool valid = Monitor.Wait(Lock,5000);

                            if (!valid)
                            {
                                break;
                            }
                        }
                        urlworked = List.Dequeue();
                        Monitor.Pulse(Lock);
                    }
                    await MethoRequest(urlworked);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Возникло исключение" + ex.Message);
                }
            }
        }
    }
}
