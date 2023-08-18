using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimeAxis
{
    public class Track : Row
    {
        public List<Segment> Segments = new List<Segment>();

        public bool IsShow { get; set; } = true;

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

        public float FontSize { get; set; } = 13;

        public Color FontColor { get; set; } = Color.Black;

        public FontStyle FontStyle { get; set; } = FontStyle.Regular;
    }
}
