using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimeAxis.Model
{
    public class DateTimeChangedEventArgs : EventArgs
    {
        public DateTime Time { get; }

        public DateTimeChangedEventArgs(DateTime time)
        {
            Time = time;
        }
    }
}
