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
            while (true)
            {
                Console.WriteLine("=====================================");
                Console.WriteLine("   Kancelarijski senzorski sistem");
                Console.WriteLine("=====================================");
                Console.WriteLine("1. Pokreni prenos podataka");
                Console.WriteLine("2. Testiraj Dispose pattern (simuliraj prekid)");
                Console.WriteLine("0. Izlaz");
                Console.Write("Izbor: ");
                string izbor = Console.ReadLine();

                switch (izbor)
                {
                    case "1":
                        StartSessionTest();
                        break;
                    case "2":
                        TestirajDispose();
                        break;
                    case "0":
                        Console.WriteLine("Izlaz iz aplikacije...");
                        return;
                    default:
                        Console.WriteLine("Nepoznata opcija, pokusaj ponovo.");
                        break;
                }

                Console.WriteLine("\nPritisni Enter za povratak u meni...");
                Console.ReadLine();
                Console.Clear();
            }
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
                    if (proksi == null)
                    {
                        Console.WriteLine("Greska pri kreiranju proksija.");
                        return;
                    }

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
                        var samples = ucitavac.LoadCsv(out List<string> invalidRows, 130);

                        int brojac = 0;
                        foreach (var sample in samples)
                        {
                            if (brojac == 10)
                            {
                                Console.WriteLine("Simulacija: gubitak konekcije usred prenosa...");
                                throw new Exception("Simulirani prekid veze");
                            }
                            servis.PushSample(sample);
                            Console.WriteLine($"[Klijent] Prenos u toku... Sample {brojac + 1}/{samples.Count} ");
                            Thread.Sleep(100);
                            brojac++;
                        }
                        Console.WriteLine("[Klijent] Završen prenos.");
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

        private static void StartSessionTest()
        {
            using (ChannelFactory<ISensorService> factory = new ChannelFactory<ISensorService>("SensorService"))
            {
                IClientChannel proxy = (IClientChannel)factory.CreateChannel();

                if (proxy == null)
                {
                    Console.WriteLine("Greska pri kreiranju proxy-a.");
                    return;
                }

                using (proxy) 
                {
                    try
                    {
                        ISensorService service = (ISensorService)proxy;
                        var meta = new SessionMeta
                        {
                            SessionId = Guid.NewGuid().ToString(),
                            StartTime = DateTime.Now
                        };
                        Console.WriteLine(service.StartSession(meta));

                        CsvLoader loader = new CsvLoader();
                        var samples = loader.LoadCsv(out List<string> invalidRows, 130);

                        foreach (var sample in samples)
                        {
                            try
                            {
                                Console.WriteLine("prenos u toku . . .");
                                service.PushSample(sample);
                                Thread.Sleep(100);
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"Greska pri slanju sample-a: {ex.Message}");
                            }
                        }

                        Console.WriteLine(service.EndSession());
                        Console.WriteLine("Prenos je zavrsen.");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Izuzetak uhvacen: {ex.Message}");
                    }
                }
            }
        }
        }
}
