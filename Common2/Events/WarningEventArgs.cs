using Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common2.Events
{
    public class WarningEventArgs : EventArgs
    {
        public string Message { get; }
        public SensorSample Sample { get; }
        public WarningEventArgs(string message, SensorSample sample) 
        { Message = message; Sample = sample; }
    }
}
