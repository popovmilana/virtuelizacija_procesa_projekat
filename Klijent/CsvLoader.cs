using Common;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.IO;

namespace Klijent
{
    internal class CsvLoader
    {
        public List<SensorSample> LoadCsv(out List<string> nevalidniRedovi, int maksRedova = 130)
        {
            nevalidniRedovi = new List<string>();
            List<SensorSample> uzorci = new List<SensorSample>();

            string csvPutanja = ConfigurationManager.AppSettings["CsvPath"];

            if (!File.Exists(csvPutanja))
                throw new FileNotFoundException($"CSV fajl nije pronađen: {csvPutanja}");

            using (var citac = new StreamReader(csvPutanja))
            {
                // Preskoči zaglavlje
                citac.ReadLine();

                int brojLinije = 0;
                while (!citac.EndOfStream && brojLinije < maksRedova)
                {
                    string linija = citac.ReadLine();
                    brojLinije++;

                    var polja = linija.Split(',');

                    try
                    {
                        if (polja.Length < 8)
                            throw new Exception("Nedovoljan broj kolona");

                        SensorSample uzorak = new SensorSample
                        {
                            DateTime = DateTime.Parse(polja[0], CultureInfo.InvariantCulture),
                            LightLevel = double.Parse(polja[2], CultureInfo.InvariantCulture),
                            RelativeHumidity = double.Parse(polja[6], CultureInfo.InvariantCulture),
                            AirQuality = double.Parse(polja[7], CultureInfo.InvariantCulture)
                        };

                        uzorci.Add(uzorak);
                    }
                    catch
                    {
                        nevalidniRedovi.Add(linija);
                    }
                }
            }

            if (nevalidniRedovi.Count > 0)
            {
                string logPutanja = ConfigurationManager.AppSettings["LogPath"];
                using (StreamWriter logPisac = new StreamWriter(logPutanja, true))
                {
                    foreach (var nevalidanRed in nevalidniRedovi)
                        logPisac.WriteLine(nevalidanRed);
                }
            }

            Console.WriteLine($"Uspešno učitanih redova: {uzorci.Count}");
            Console.WriteLine($"Nevalidnih redova: {nevalidniRedovi.Count}");

            return uzorci;
        }
    }
}
