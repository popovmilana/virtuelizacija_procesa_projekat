using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace Common
{
    public delegate void TransferStartedEventHandler();
    public delegate void TransferCompletedEventHandler();
    public delegate void SampleReceivedEventHandler(SensorSample sample);
    public delegate void WarningRaisedEventHandler(string message, SensorSample sample);

    public delegate void LightSpikeEventHandler(string message, SensorSample sample, double deltaL);
    public delegate void OutOfBandWarningEventHandler(string message, SensorSample sample, double avg);
    
    public delegate void RHSpikeEventHandler(string message, SensorSample sample, double deltaRH);
    public delegate void AQSpikeEventHandler(string message, SensorSample sample, double deltaAQ);
    public class SensorEvents
    {
        public event TransferStartedEventHandler TransferStarted;
        public event TransferCompletedEventHandler TransferCompleted;
        public event SampleReceivedEventHandler SampleReceived;
        public event WarningRaisedEventHandler WarningRaised;
        public event LightSpikeEventHandler LightSpike;
        public event OutOfBandWarningEventHandler OutOfBandWarning;
        public event RHSpikeEventHandler RHSpike;
        public event AQSpikeEventHandler AQSpike;
        public void RaiseTransferStarted()
        {
            if (TransferStarted != null)
                TransferStarted();
        }
        public void RaiseTransferCompleted()
        {
            if (TransferCompleted != null)
                TransferCompleted();
        }
        public void RaiseSampleReceived(SensorSample sample)
        {
            if (SampleReceived != null)
                SampleReceived(sample);
        }
        public void RaiseWarning(string message, SensorSample sample)
        {
            if (WarningRaised != null)
                WarningRaised(message, sample);
        }
        public void RaiseLightSpike(string message, SensorSample sample, double deltaL)
        {
            if (LightSpike != null)
                LightSpike(message, sample, deltaL);
        }
        public void RaiseOutOfBandWarning(string message, SensorSample sample, double avg)
        {
            if (OutOfBandWarning != null)
                OutOfBandWarning(message, sample, avg);
        }
        public void RaiseRHSpike(string message, SensorSample sample, double deltaRH)
        {
            if (RHSpike != null)
                RHSpike(message, sample, deltaRH);
        }
        public void RaiseAQSpike(string message, SensorSample sample, double deltaAQ)
        {
            if (AQSpike != null)
                AQSpike(message, sample, deltaAQ);
        }
    }
}
