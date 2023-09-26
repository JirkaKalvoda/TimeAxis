using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimeAxis
{
    public class Segment : ICloneable
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

        /// <summary>
        /// 显示文本
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// 惟一名字
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 惟一号码
        /// </summary>
        public int Id { get; set; }

        public string Font { get; set; } = "Calibri";

        public float FontSize { get; set; } = 8;

        public Color FontColor { get; set; } = Color.Black;

        public FontStyle FontStyle { get; set; } = FontStyle.Bold;

        public object Clone()
        {
            Segment output = new Segment();
            output.Start = this.Start;
            output.Stop = this.Stop;
            output.DefaultColor = this.DefaultColor;
            output.BorderColor = this.BorderColor;
            output.HighLightColor = this.HighLightColor;
            output.BorderWidth = this.BorderWidth;
            output.IsSelected = this.IsSelected;
            output.Text = this.Text;
            output.Name = this.Name;
            output.Id = this.Id;
            output.Font = this.Font;
            output.FontColor = this.FontColor;
            output.FontSize = this.FontSize;
            output.FontStyle = this.FontStyle;
            return output;
        }
    }
}
