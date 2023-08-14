using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimeAxis
{
    public class Segment
    {
        public DateTime Start { get; set; }
        
        public DateTime Stop { get; set; }

        public Color Color
        {
            get { return IsSelected ? HighLightColor : DefaultColor; }
        }

        public Color DefaultColor { get; set; } = Color.BurlyWood;

        public Color BorderColor { get; set; } = Color.DeepSkyBlue;

        public Color HighLightColor { get; set; } = Color.LightSkyBlue;

        public int BorderWidth { get; set; } = 2;

        public bool IsSelected { get; set; } = false;
    }
}
