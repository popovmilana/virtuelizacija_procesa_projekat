using Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common2.Events
{
    public class AQSpikeEventArgs : EventArgs
    {
        public string Message { get; }
        public SensorSample Sample { get; }
        public double DeltaAQ { get; }
        public AQSpikeEventArgs(string message, SensorSample sample, double deltaAQ)
        { Message = message; Sample = sample; DeltaAQ = deltaAQ; }
    }
}
