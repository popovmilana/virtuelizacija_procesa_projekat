using Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common2.Events
{
    public class LightSpikeEventArgs : EventArgs
    {
        public string Message { get; }
        public SensorSample Sample { get; }
        public double DeltaL { get; }
        public LightSpikeEventArgs(string message, SensorSample sample, double deltaL)
        { Message = message; Sample = sample; DeltaL = deltaL; }
    }
}
