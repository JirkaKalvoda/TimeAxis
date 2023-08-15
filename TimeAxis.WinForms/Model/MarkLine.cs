using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimeAxis
{
    /// <summary>
    /// 游标类，同个对象在上标尺和下标尺分别画1条线
    /// </summary>
    public class MarkLine
    {
        public Color Color { get; set; } = Color.IndianRed;

        public DateTime Time { get; set; }

        public int Width { get; set; } = 2;
    }
}
