using Common;
using Common.Faults;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class SensorService : ISensorService
    {
        private string sessionDir;
        private SessionFiles sessionCSVFiles;
        private bool transferStarted = false;

        private double avgLightLevel = 0;
        private double avgRelativeHumidity = 0;
        private double avgAirQuality = 0;
        private int sampleCount = 0;

        private readonly double L_threshold = double.Parse(ConfigurationManager.AppSettings["L_threshold"] ?? "50");
        private readonly double RH_threshold = double.Parse(ConfigurationManager.AppSettings["RH_threshold"] ?? "10");
        private readonly double AQ_threshold = double.Parse(ConfigurationManager.AppSettings["AQ_threshold"] ?? "100");
       
        //prethodne vrednosti da bi deltu racunali
        private double lastLightLevel = 0;
        private double lastRelativeHumidity = 0;
        private double lastAirQuality = 0;


        private readonly SensorEvents events = new SensorEvents();
        public SensorService()
        {
            //pretplate na dogadjaje
            events.TransferStarted += () =>
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("[INFO] Prenos je zapocet...");
                Console.ResetColor();
            };
            events.SampleReceived += (sample) =>
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine($"[SAMPLE] {sample.DateTime}: V={sample.Volume:F2} LL={sample.LightLevel:F2} RH={sample.RelativeHumidity:F2} AQ={sample.AirQuality:F2}");
                Console.ResetColor();
            };
            events.TransferCompleted += () =>
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("[INFO] Prenos je zavrsen.");
                Console.ResetColor();
            };
            events.WarningRaised += (message, sample) =>
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"[UPOZORENJE] {message} | {sample.DateTime}");
                Console.ResetColor();
            };
            events.LightSpike += (message, sample, deltaL) =>
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"[LIGHT SPIKE] {message} | Delta: {deltaL:F2} | {sample.DateTime}");
                Console.ResetColor();
            };
            events.RHSpike += (message, sample, deltaRH) =>
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"[RH SPIKE] {message} | Delta: {deltaRH:F2} | {sample.DateTime}");
                Console.ResetColor();
            };
            events.AQSpike += (message, sample, deltaAQ) =>
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"[AQ SPIKE] {message} | Delta: {deltaAQ:F2} | {sample.DateTime}");
                Console.ResetColor();
            };
            events.OutOfBandWarning += (message, sample, avg) =>
            {
                Console.ForegroundColor = ConsoleColor.Magenta;
                Console.WriteLine($"[OUT-OF-BAND] {message} | Prosek: {avg:F2} | {sample.DateTime}");
                Console.ResetColor();
            };
        }

        public string StartSession(SessionMeta meta)
        {
            if (meta == null)
                throw new FaultException<DataFormatFault>(new DataFormatFault("SessionMeta ne sme biti null."));


            if (string.IsNullOrWhiteSpace(meta.SessionId))
                throw new FaultException("SessionId je obavezan.");

            string osnovnaPutanja = ConfigurationManager.AppSettings["SessionsPath"]
                ?? AppDomain.CurrentDomain.BaseDirectory;

            sessionDir = Path.Combine(osnovnaPutanja, meta.SessionId);
            Directory.CreateDirectory(sessionDir);

            sessionCSVFiles = new SessionFiles(
                Path.Combine(sessionDir, "measurements_session.csv"),
                Path.Combine(sessionDir, "rejects.csv"));

            transferStarted = false;
            avgLightLevel = 0;
            avgRelativeHumidity = 0;
            avgAirQuality = 0;
            sampleCount = 0;

            events.RaiseTransferStarted();
            return "Sesija zapoceta!";
        }

        public void PushSample(SensorSample sample)
        {
            //validacija uzorka
            if (sample == null)
            {
                throw new FaultException<DataFormatFault>(
                    new DataFormatFault("Uzorak senzora (SensorSample) ne sme biti prazan."));
            }

            if (sessionCSVFiles == null)
                throw new FaultException<ValidationFault>(
                     new ValidationFault("StartSession nije pozvan pre PushSample."));

            string line = $"{sample.DateTime},{sample.Volume},{sample.LightLevel},{sample.RelativeHumidity},{sample.AirQuality}";
            if (sample.DateTime == default(DateTime))
            {
                sessionCSVFiles.RejectsWriter.WriteLine(line + ", nevalidan datum");
                sessionCSVFiles.RejectsWriter.Flush();
                throw new FaultException<ValidationFault>(
                    new ValidationFault("DateTime je obavezan i ne sme biti default vrednost."));
            }

            if(double.IsNaN(sample.LightLevel) || double.IsInfinity(sample.LightLevel))
            {
                sessionCSVFiles.RejectsWriter.WriteLine(line + ", nevalidan LightLevel");
                sessionCSVFiles.RejectsWriter.Flush();
                throw new FaultException<DataFormatFault>(
                    new DataFormatFault("LightLevel mora biti validan broj."));
            }

            if(sample.LightLevel<0)
            {
                sessionCSVFiles.RejectsWriter.WriteLine(line + ", negativan LightLevel");
                sessionCSVFiles.RejectsWriter.Flush();
                throw new FaultException<ValidationFault>(
                   new ValidationFault("LightLevel ne sme biti negativan."));
            }

            if(double.IsNaN(sample.RelativeHumidity) || double.IsInfinity(sample.RelativeHumidity))
            {
                sessionCSVFiles.RejectsWriter.WriteLine(line + ", nevalidan RelativeHumidity");
                sessionCSVFiles.RejectsWriter.Flush();
                throw new FaultException<DataFormatFault>(
                    new DataFormatFault("RelativeHumidity mora biti validan broj."));
            }

            if(sample.RelativeHumidity<=0)
            {
                sessionCSVFiles.RejectsWriter.WriteLine(line + ", RelativeHumidity <= 0");
                sessionCSVFiles.RejectsWriter.Flush();
                throw new FaultException<ValidationFault>(
                    new ValidationFault("RelativeHumidity mora biti veci od nule."));
            }

            if(double.IsNaN(sample.AirQuality) || double.IsInfinity(sample.AirQuality))
            {
                sessionCSVFiles.RejectsWriter.WriteLine(line + ", nevalidan AirQuality");
                sessionCSVFiles.RejectsWriter.Flush();
                throw new FaultException<DataFormatFault>(
                    new DataFormatFault("AirQuality mora biti validan broj."));
            }
                if (sample.AirQuality<0)
            {
                sessionCSVFiles.RejectsWriter.WriteLine(line + ", negativan AirQuality");
                sessionCSVFiles.RejectsWriter.Flush();
                throw new FaultException<ValidationFault>(
                    new ValidationFault("AirQuality ne sme biti negativan."));
            }

            if(double.IsNaN(sample.Volume) || double.IsInfinity(sample.Volume))
            {
                sessionCSVFiles.RejectsWriter.WriteLine(line + ", nevalidan Volume");
                sessionCSVFiles.RejectsWriter.Flush();
                throw new FaultException<DataFormatFault>(
                    new DataFormatFault("Volume mora biti validan broj."));
            }

            if (sample.Volume < 0)
            {
                sessionCSVFiles.RejectsWriter.WriteLine(line + ", negativan Volume");
                sessionCSVFiles.RejectsWriter.Flush();
                throw new FaultException<ValidationFault>(
                    new ValidationFault("Volume ne sme biti negativan."));
            }

            try
            {
                if (!transferStarted)
                    transferStarted = true;

                List<string> warnings = new List<string>();
                if (sampleCount > 0)
                {
                    if (sample.LightLevel < avgLightLevel * 0.75)
                        warnings.Add($"LightLevel odstupa više od -25% od proseka. \n LL={sample.LightLevel:F2} (prosek={avgLightLevel:F2})");
                    if (sample.LightLevel > avgLightLevel * 1.25)
                        warnings.Add($"LightLevel odstupa više od +25% od proseka.  \n LL={sample.LightLevel:F2} (prosek={avgLightLevel:F2}) ");

                    if (sample.RelativeHumidity < avgRelativeHumidity * 0.75)
                        warnings.Add($"RelativeHumidity odstupa više od -25% od proseka.  \n RH={sample.RelativeHumidity:F2} (prosek={avgRelativeHumidity:F2}) ");
                    if (sample.RelativeHumidity > avgRelativeHumidity * 1.25)
                        warnings.Add($"RelativeHumidity odstupa više od +25% od proseka.  \n RH={sample.RelativeHumidity:F2} (prosek={avgRelativeHumidity:F2}) ");

                    if (sample.AirQuality < avgAirQuality * 0.75)
                        warnings.Add($"AirQuality odstupa više od -25% od proseka. \n AQ={sample.AirQuality:F2} (prosek={avgAirQuality:F2}) ");
                    if (sample.AirQuality > avgAirQuality * 1.25)
                        warnings.Add($"AirQuality odstupa više od +25% od proseka.  \n AQ={sample.AirQuality:F2} (prosek={avgAirQuality:F2})\" ");

                }


                //detekcija nagle promene svetla
                if (sampleCount > 0)
                {
                    double deltaL = sample.LightLevel - lastLightLevel;

                    if (Math.Abs(deltaL) > L_threshold)
                    {
                        string message;
                        if (deltaL > 0)
                            message = "Nagla promena svetla: " + deltaL + ", iznad ocekivanog.";
                        else
                            message = "Nagla promena svetla: " + deltaL + ", ispod ocekivanog.";

                        events.RaiseLightSpike(message, sample, deltaL);
                    }

                    //odstupanje +/- 25% od proseka
                    if (sample.LightLevel < avgLightLevel * 0.75)
                        events.RaiseOutOfBandWarning("LightLevel ispod očekivane vrednosti", sample, avgLightLevel);
                    else if (sample.LightLevel > avgLightLevel * 1.25)
                        events.RaiseOutOfBandWarning("LightLevel iznad očekivane vrednosti", sample, avgLightLevel);

                }

                //detekcija nagle promene RelativeHumidity
                if (sampleCount > 0)
                {
                    double deltaRH = sample.RelativeHumidity - lastRelativeHumidity;
                    if (Math.Abs(deltaRH) > RH_threshold)
                    {
                        string message;
                        if (deltaRH > 0)
                            message = "Nagla promena RH: " + deltaRH + ", iznad ocekivanog.";
                        else
                            message = "Nagla promena RH: " + deltaRH + ", ispod ocekivanog.";

                        events.RaiseRHSpike(message, sample, deltaRH);
                    }

                    if (sample.RelativeHumidity < avgRelativeHumidity * 0.75)
                        events.RaiseOutOfBandWarning("RelativeHumidity ispod ocekivane vrednosti", sample, avgRelativeHumidity);
                    else if (sample.RelativeHumidity > avgRelativeHumidity * 1.25)
                        events.RaiseOutOfBandWarning("RelativeHumidity iznad ocekivane vrednosti", sample, avgRelativeHumidity);
                }

                //detekcija nagle promene AirQuality
                if (sampleCount > 0)
                {
                    double deltaAQ = sample.AirQuality - lastAirQuality;
                    if (Math.Abs(deltaAQ) > AQ_threshold)
                    {
                        string message;
                        if (deltaAQ > 0)
                            message = "Nagla promena AQ: " + deltaAQ + ", iznad ocekivanog.";
                        else
                            message = "Nagla promena AQ: " + deltaAQ + ", ispod ocekivanog.";

                        events.RaiseAQSpike(message, sample, deltaAQ);
                    }

                    if (sample.AirQuality < avgAirQuality * 0.75)
                        events.RaiseOutOfBandWarning("AirQuality ispod ocekivane vrednosti", sample, avgAirQuality);
                    else if (sample.AirQuality > avgAirQuality * 1.25)
                        events.RaiseOutOfBandWarning("AirQuality iznad ocekivane vrednosti", sample, avgAirQuality);
                }


                //azuriranje proseka i prethodnih vrednosti
                sampleCount++;
                avgLightLevel = (avgLightLevel * (sampleCount - 1) + sample.LightLevel) / sampleCount;
                avgRelativeHumidity = (avgRelativeHumidity * (sampleCount - 1) + sample.RelativeHumidity) / sampleCount;
                avgAirQuality = (avgAirQuality * (sampleCount - 1) + sample.AirQuality) / sampleCount;

                lastLightLevel = sample.LightLevel;
                lastRelativeHumidity = sample.RelativeHumidity;
                lastAirQuality = sample.AirQuality;


                //upis u fajl 
                sessionCSVFiles.MeasurementsWriter.WriteLine(line);
                sessionCSVFiles.MeasurementsWriter.Flush();

                Console.ForegroundColor = ConsoleColor.DarkCyan;
                Console.WriteLine($"[Server] Prenos u toku... primljen uzorak {sampleCount}/{sampleCount}");
                Console.ResetColor();
                events.RaiseSampleReceived(sample);
                foreach (var warning in warnings)
                    events.RaiseWarning(warning, sample);

                Console.WriteLine("-----------------------------------------------------------");
            }
            catch (Exception ex)
            {
                sessionCSVFiles.RejectsWriter.WriteLine(line + ", izuzetak: " + ex.Message);
                sessionCSVFiles.RejectsWriter.Flush();
                sessionCSVFiles.Dispose();
                throw;
            }
        }

        public string EndSession()
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("[Server] Završen prenos.");
            Console.ResetColor();
            
            if (transferStarted)
                events.RaiseTransferCompleted();

            sessionCSVFiles?.Dispose();
            return "Sesija zavrsena!";
        }
    }
}
