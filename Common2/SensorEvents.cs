using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common2.Events;
namespace Common
{
    public delegate void TransferEventHandler(object sender, EventArgs e);
    public delegate void SampleEventHandler(object sender, SampleEventArgs e);
    public delegate void WarningEventHandler(object sender, WarningEventArgs e);

    public delegate void LightSpikeEventHandler(object sender, LightSpikeEventArgs e);
    public delegate void OutOfBandEventHandler(object sender,OutOfBandEventArgs e);
    
    public delegate void RHSpikeEventHandler(object sender, RHSpikeEventArgs e);
    public delegate void AQSpikeEventHandler(object sender, AQSpikeEventArgs e);
    public class SensorEvents
    {
        public event TransferEventHandler OnTransferStarted;
        public event TransferEventHandler OnTransferCompleted;
        public event SampleEventHandler OnSampleReceived;
        public event WarningEventHandler OnWarningRaised;
        public event LightSpikeEventHandler LightSpike;
        public event OutOfBandEventHandler OutOfBandWarning;
        public event RHSpikeEventHandler RHSpike;
        public event AQSpikeEventHandler AQSpike;
        public void RaiseTransferStarted()
        {
            if (OnTransferStarted != null)
                OnTransferStarted(this, EventArgs.Empty);
        }
        public void RaiseTransferCompleted()
        {
            if (OnTransferCompleted != null)
                OnTransferCompleted(this, EventArgs.Empty);
        }
        public void RaiseSampleReceived(SensorSample sample)
        {
            if (OnSampleReceived != null)
                OnSampleReceived(this, new SampleEventArgs(sample));
        }
        public void RaiseWarning(string message, SensorSample sample)
        {
            if (OnWarningRaised != null)
                OnWarningRaised(this, new WarningEventArgs(message, sample));
        }
        public void RaiseLightSpike(string message, SensorSample sample, double deltaL)
        {
            if (LightSpike != null)
                LightSpike(this, new LightSpikeEventArgs(message, sample, deltaL));
        }
        public void RaiseOutOfBandWarning(string message, SensorSample sample, double avg)
        {
            if (OutOfBandWarning != null)
                OutOfBandWarning(this, new OutOfBandEventArgs(message, sample, avg));
        }
        public void RaiseRHSpike(string message, SensorSample sample, double deltaRH)
        {
            if (RHSpike != null)
                RHSpike(this, new RHSpikeEventArgs(message, sample, deltaRH));
        }
        public void RaiseAQSpike(string message, SensorSample sample, double deltaAQ)
        {
            if (AQSpike != null)
                AQSpike(this, new AQSpikeEventArgs(message, sample, deltaAQ));
        }
    }
}
