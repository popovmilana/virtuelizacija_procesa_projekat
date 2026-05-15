using Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common2.Events
{
    public class SampleEventArgs : EventArgs
    {
        public SensorSample Sample { get; }
        public SampleEventArgs(SensorSample sample) { Sample = sample; }
    }
}
