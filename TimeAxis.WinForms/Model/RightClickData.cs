using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimeAxis.Model
{
    /// <summary>
    /// 右键菜单里需要传出的数据类
    /// </summary>
    public class RightClickData : ICloneable
    {
        public DateTime? MarkTime { get; set; }

        public Track Track { get; set; }

        public Segment Segment { get; set; }

        internal Point Position { get; set; }

        public object Clone()
        {
            RightClickData output = new RightClickData();
            output.MarkTime = this.MarkTime;
            output.Position = this.Position;
            output.Track = this.Track.Clone() as Track;
            output.Segment = this.Segment.Clone() as Segment;
            return output;
        }
    }
}
