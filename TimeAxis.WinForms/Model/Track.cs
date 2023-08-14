using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimeAxis
{
    public class Track : Row
    {
        public List<Segment> Segments = new List<Segment>();

        public bool IsShow { get; set; } = true;
    }
}
