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
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("Nepoznata opcija, pokusaj ponovo.");
                        Console.ResetColor();
                        break;
                }

                Console.WriteLine("\nPritisni Enter za povratak u meni...");
                Console.ReadLine();
                Console.Clear();
            }
        }
        private static void TestirajDispose()
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("Testiranje Dispose pattern-a (simulacija prekida veze)...");
            Console.ResetColor();
            try
            {
                using (ChannelFactory<ISensorService> fabrika =
                    new ChannelFactory<ISensorService>("SensorService"))
                {
                    IClientChannel proksi = fabrika.CreateChannel() as IClientChannel;
                    if (proksi == null)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("Greska pri kreiranju proksija.");
                        Console.ResetColor();
                        return;
                    }

                    using (proksi)
                    {
                        ISensorService servis = proksi as ISensorService;

                        CsvLoader ucitavac = new CsvLoader();
                        var samples = ucitavac.LoadCsv(out List<string> invalidRows, 130);

                        var meta = new SessionMeta
                        {
                            SessionId = Guid.NewGuid().ToString(),
                            StartTime = DateTime.Now,
                            Volume = 0,
                            LightLevel = 0,
                            RelativeHumidity = 0,
                            AirQuality = 0
                        };

                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine(servis.StartSession(meta));
                        Console.ResetColor();



                        int brojac = 0;
                        foreach (var sample in samples)
                        {
                            if (brojac == 10)
                            {
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.WriteLine("Simulacija: gubitak konekcije usred prenosa...");
                                Console.ResetColor();
                                //servis.EndSession();
                                throw new Exception("Simulirani prekid veze");
                            }
                            servis.PushSample(sample);
                            Console.ForegroundColor = ConsoleColor.Cyan;
                            Console.WriteLine($"[Klijent] Prenos u toku... Sample {brojac + 1}/{samples.Count} ");
                            Console.ResetColor();
                            Thread.Sleep(100);
                            brojac++;
                        }
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine("[Klijent] Završen prenos.");
                        Console.WriteLine(servis.EndSession());
                        Console.ResetColor();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Izuzetak uhvacen: {ex.Message}");
                Console.ResetColor();
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.WriteLine("Resursi su automatski zatvoreni zahvaljujuci Dispose implementaciji.");
                Console.ResetColor(); 
            }
        }

        private static void StartSessionTest()
        {
            using (ChannelFactory<ISensorService> factory = new ChannelFactory<ISensorService>("SensorService"))
            {
                IClientChannel proxy = (IClientChannel)factory.CreateChannel();

                if (proxy == null)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Greska pri kreiranju proxy-a.");
                    Console.ResetColor();
                    return;
                }

                using (proxy)
                {
                    try
                    {
                        ISensorService service = (ISensorService)proxy;

                        CsvLoader loader = new CsvLoader();
                        var samples = loader.LoadCsv(out List<string> invalidRows, 130);

                        var meta = new SessionMeta
                        {
                            SessionId = Guid.NewGuid().ToString(),
                            StartTime = DateTime.Now,
                            Volume = 0,
                            LightLevel = 0,
                            RelativeHumidity = 0,
                            AirQuality = 0
                        };

                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine(service.StartSession(meta));
                        Console.ResetColor();

                        int brojac = 0;
                        foreach (var sample in samples)
                        {
                            try
                            {
                                Console.ForegroundColor = ConsoleColor.Cyan;
                                Console.WriteLine($"[Klijent] Prenos u toku... Sample {++brojac}/{samples.Count}");
                                Console.ResetColor();
                                service.PushSample(sample);
                                Thread.Sleep(100);
                            }
                            catch (Exception ex)
                            {
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.WriteLine($"Greska pri slanju sample-a: {ex.Message}");
                                Console.ResetColor();
                            }
                        }

                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine(service.EndSession());
                        Console.WriteLine("Prenos je zavrsen.");
                        Console.ResetColor();
                    }
                    catch (Exception ex)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine($"Izuzetak uhvacen: {ex.Message}");
                        Console.ResetColor();
                    }
                }
            }
        }
    }
}
