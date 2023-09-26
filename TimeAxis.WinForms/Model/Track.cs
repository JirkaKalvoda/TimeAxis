using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimeAxis
{
    public class Track : Row, ICloneable
    {
        public List<Segment> Segments = new List<Segment>();

        /// <summary>
        /// 是否显示行
        /// </summary>
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

        /// <summary>
        /// 对象ID，如果同个对象会创建多行则启用该属性
        /// </summary>
        public int ObjectId { get; set; }

        public Image Image { get; set; }

        public string Font { get; set; } = "Calibri";

        public float FontSize { get; set; } = 13;

        public Color FontColor { get; set; } = Color.Black;

        public FontStyle FontStyle { get; set; } = FontStyle.Regular;

        public object Clone()
        {
            Track output = new Track();
            output.IsShow = this.IsShow;
            output.Text = this.Text;
            output.Name = this.Name;
            output.Id = this.Id;
            output.ObjectId = this.ObjectId;
            output.Image = this.Image;
            output.Font = this.Font;
            output.FontColor = this.FontColor;
            output.FontSize = this.FontSize;
            output.FontStyle = this.FontStyle;
            foreach (Segment seg in Segments)
            {
                output.Segments.Add(seg.Clone() as Segment);
            }
            return output;
        }
    }
}
