using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimeAxis
{
    public class MarkLine
    {
        public Color Color { get; set; } = Color.LawnGreen;

        public DateTime Time { get; set; }

        public int Width { get; set; } = 2;
    }
}
