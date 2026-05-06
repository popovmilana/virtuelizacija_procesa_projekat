using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public class SessionFiles : IDisposable
    {
        public StreamWriter MeasurementsWriter { get; private set; }
        public StreamWriter RejectsWriter { get; private set; }
        public string MeasurementsFilePath { get; private set; }
        public string RejectsFilePath { get; private set; }

        public SessionFiles(string putanjaMerenja, string putanjaOdbacenih)
        {
            MeasurementsFilePath = putanjaMerenja;
            RejectsFilePath = putanjaOdbacenih;

            MeasurementsWriter = new StreamWriter(
                File.Open(MeasurementsFilePath, FileMode.Create, FileAccess.Write));
            RejectsWriter = new StreamWriter(
                File.Open(RejectsFilePath, FileMode.Create, FileAccess.Write));

            MeasurementsWriter.WriteLine("DateTime,LightLevel,RelativeHumidity,AirQuality");
            MeasurementsWriter.Flush();

            RejectsWriter.WriteLine("DateTime,LightLevel,RelativeHumidity,AirQuality,Razlog");
            RejectsWriter.Flush();
        }

        public void Dispose()
        {
            MeasurementsWriter?.Dispose();
            MeasurementsWriter = null;

            RejectsWriter?.Dispose();
            RejectsWriter = null;

            Console.WriteLine("SessionFiles resursi su oslobođeni (Dispose pozvan).");
        }
    }
}
