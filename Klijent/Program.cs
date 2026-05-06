using Common;
using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.Threading;

namespace Klijent
{
    internal class Program
    {
        static void Main(string[] args)
        {
            TestirajDispose();
            Console.ReadLine();
        }
        private static void TestirajDispose()
        {
            Console.WriteLine("Testiranje Dispose pattern-a (simulacija prekida veze)...");
            try
            {
                using (ChannelFactory<ISensorService> fabrika =
                    new ChannelFactory<ISensorService>("SensorService"))
                {
                    IClientChannel proksi = fabrika.CreateChannel() as IClientChannel;
                    using (proksi)
                    {
                        ISensorService servis = proksi as ISensorService;

                        var meta = new SessionMeta
                        {
                            SessionId = Guid.NewGuid().ToString(),
                            StartTime = DateTime.Now
                        };

                        Console.WriteLine(servis.StartSession(meta));

                        CsvLoader ucitavac = new CsvLoader();
                        var uzorci = ucitavac.LoadCsv(out List<string> nevalidniRedovi, 130);

                        int brojac = 0;
                        foreach (var uzorak in uzorci)
                        {
                            if (brojac == 10)
                            {
                                Console.WriteLine("Simulacija: gubitak konekcije usred prenosa...");
                                throw new Exception("Simulirani prekid veze");
                            }

                            servis.PushSample(uzorak);
                            Thread.Sleep(100);
                            brojac++;
                        }

                        Console.WriteLine(servis.EndSession());
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Izuzetak uhvaćen: {ex.Message}");
                Console.WriteLine("Resursi su automatski zatvoreni zahvaljujući Dispose implementaciji.");
            }
        }
    }
}
