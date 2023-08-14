using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimeAxis
{
    public class Ruler : Row
    {
        public double Scale { get; set; } = 1;

        public DateTime Start { get; set; }

        public DateTime Stop { get; set; }

        public DateTime DisplayStart { get; set; }
        
        public DateTime DisplayStop { get; set; }

        public string Font { get; set; } = "Calibri";

        public float FontSize { get; set; } = 13;

        public Color FontColor { get; set; } = Color.Black;

        public FontStyle FontStyle { get; set; } = FontStyle.Bold;

        public Ruler()
        {
            Start = DateTime.Today;
            Stop = DateTime.Today.AddDays(1);
            DisplayStart = Start;
            DisplayStop = Stop;
            Height = 45;
        }
    }
}
