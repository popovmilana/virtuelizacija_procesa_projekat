using Common;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    public class SensorService : ISensorService
    {
        private string sessionDir;
        private SessionFiles sessionFiles;
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
       // private readonly SensorEvent events = new SensorEvent();
        public SensorService()
        {
            //pretplate na dogadjaje

        }

        public string StartSession(SessionMeta meta)
        {
            if (meta == null)
                throw new FaultException("SessionMeta ne sme biti null.");

            if (string.IsNullOrWhiteSpace(meta.SessionId))
                throw new FaultException("SessionId je obavezan.");

            string osnovnaPutanja = ConfigurationManager.AppSettings["SessionsPath"]
                ?? AppDomain.CurrentDomain.BaseDirectory;

            sessionDir = System.IO.Path.Combine(osnovnaPutanja, meta.SessionId);
            System.IO.Directory.CreateDirectory(sessionDir);

            sessionFiles = new SessionFiles(
                System.IO.Path.Combine(sessionDir, "measurements_session.csv"),
                System.IO.Path.Combine(sessionDir, "rejects.csv"));

            transferStarted = false;
            avgLightLevel = 0;
            avgRelativeHumidity = 0;
            avgAirQuality = 0;
            sampleCount = 0;

            return "Sesija zapoceta!";
        }

        public string PushSample(SensorSample sample)
        {
            return "Sample received";
        }

        public string EndSession()
        {
            sessionFiles?.Dispose();
            return "Sesija zavrsena!";

        }
    }
}
