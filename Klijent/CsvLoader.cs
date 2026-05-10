using Common;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.IO;

namespace Klijent
{
    public class CsvLoader
    {
        public List<SensorSample> LoadCsv(out List<string> invalidRows, int maxRows = 130)
        {
            invalidRows = new List<string>();
            List<SensorSample> samples = new List<SensorSample>();
            string csvPath = ConfigurationManager.AppSettings["CsvPath"];

            if (!File.Exists(csvPath))
                throw new FileNotFoundException($"CSV fajl nije pronađen: {csvPath}");

            using (var reader = new StreamReader(csvPath))
            {
                reader.ReadLine();
                int lineCount = 0;

                while (!reader.EndOfStream && lineCount < maxRows)
                {
                    string line = reader.ReadLine();
                    lineCount++;
                    var fields = line.Split(',');

                    try
                    {
                        if (fields.Length < 8)
                            throw new Exception("Nedovoljan broj kolona");

                        SensorSample sample = new SensorSample
                        {
                            DateTime = DateTime.Parse(fields[0], CultureInfo.InvariantCulture),
                            Volume = double.Parse(fields[1], CultureInfo.InvariantCulture),
                            LightLevel = double.Parse(fields[2], CultureInfo.InvariantCulture),
                            RelativeHumidity = double.Parse(fields[6], CultureInfo.InvariantCulture),
                            AirQuality = double.Parse(fields[7], CultureInfo.InvariantCulture)
                        };
                        samples.Add(sample);
                    }
                    catch
                    {
                        invalidRows.Add(line);
                    }
                }
            }

            if (invalidRows.Count > 0)
            {
                string logPath = ConfigurationManager.AppSettings["LogPath"];
                using (StreamWriter logWriter = new StreamWriter(logPath, true))
                {
                    foreach (var invalidRow in invalidRows)
                        logWriter.WriteLine(invalidRow);
                }
            }

            Console.WriteLine($"Uspešno učitanih redova: {samples.Count}");
            Console.WriteLine($"Nevalidnih redova: {invalidRows.Count}");
            return samples;
        }
    }
}