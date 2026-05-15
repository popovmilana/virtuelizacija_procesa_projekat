using Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common2.Events
{
    public class RHSpikeEventArgs : EventArgs
    {
        public string Message { get; }
        public SensorSample Sample { get; }
        public double DeltaRH { get; }
        public RHSpikeEventArgs(string message, SensorSample sample, double deltaRH)
        { Message = message; Sample = sample; DeltaRH = deltaRH; }
    }
}
