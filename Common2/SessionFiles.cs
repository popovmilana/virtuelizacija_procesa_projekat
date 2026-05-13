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

        private bool disposed = false;

        public SessionFiles(string putanjaMerenja, string putanjaOdbacenih)
        {
            MeasurementsFilePath = putanjaMerenja;
            RejectsFilePath = putanjaOdbacenih;

            MeasurementsWriter = new StreamWriter(
                File.Open(MeasurementsFilePath, FileMode.Create, FileAccess.Write));
            RejectsWriter = new StreamWriter(
                File.Open(RejectsFilePath, FileMode.Create, FileAccess.Write));

            MeasurementsWriter.WriteLine("DateTime,Volume,LightLevel,RelativeHumidity,AirQuality");
            MeasurementsWriter.Flush();

            RejectsWriter.WriteLine("DateTime,Volume,LightLevel,RelativeHumidity,AirQuality,Razlog");
            RejectsWriter.Flush();
        }

        ~SessionFiles()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    MeasurementsWriter?.Dispose();
                    MeasurementsWriter = null;

                    RejectsWriter?.Dispose();
                    RejectsWriter = null;

                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    Console.WriteLine("[DISPOSE] SessionFiles resursi su oslobodjeni.");
                    Console.ResetColor();
                }
                disposed = true;
            }
        }
    }
}
