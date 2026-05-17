using Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common2.Events
{
    public class OutOfBandEventArgs : EventArgs
    {
        public string Message { get; }
        public SensorSample Sample { get; }
        public double Average { get; }
        public OutOfBandEventArgs(string message, SensorSample sample, double avg)
        { Message = message; Sample = sample; Average = avg; }

    }
}
