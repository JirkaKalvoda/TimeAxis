using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TimeAxis.Model;

namespace TimeAxis
{
    public partial class TimeAxisMain : UserControl
    {
        #region 公开

        #region 可配置项

        public Color BackColorLeft { get; set; } = Color.White;

        public Color BackColorRight { get; set; } = SystemColors.ControlDarkDark;

        public int BorderWidth { get; set; } = 1;

        public Color BorderColor { get; set; } = Color.DarkGray;

        public int ScrollWidth { get; set; } = 15;

        public SplitLine SplitLine { get; set; } = new SplitLine();

        public MarkLine MarkLine { get; set; } = new MarkLine();

        public Ruler Ruler { get; set; } = new Ruler();

        public List<Track> Tracks { get; set; } = new List<Track>();

        public Tick UpperTick { get; set; } = new Tick();

        public Tick LowerTick { get; set; } = new Tick();

        public ContextMenuStrip RulerHeaderMenu { get; set; } = new ContextMenuStrip();

        public ContextMenuStrip RulerMenu { get; set; } = new ContextMenuStrip();
        
        public ContextMenuStrip TrackHeaderMenu { get; set; } = new ContextMenuStrip();
        
        public ContextMenuStrip TrackBlankMenu { get; set; } = new ContextMenuStrip();
        
        public ContextMenuStrip SegmentMenu { get; set; } = new ContextMenuStrip();


        #endregion

        public TimeAxisMain()
        {
            InitializeComponent();
            SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint | ControlStyles.ResizeRedraw, true);
            Ruler.Start = DateTime.Today;
            Ruler.Stop = DateTime.Today.AddDays(1);
            MarkLine.Time = Ruler.DisplayStart;
            LowerTick.IsUpperAlign = false;
            UpperTick.TimeToX = UpperTimeToXPosition;
            LowerTick.TimeToX = LowerTimeToXPosition;
            // 滚动条不会被清除
            this.Controls.Add(vScrollBar);
            vScrollBar.Width = ScrollWidth;
            vScrollBar.SmallChange = 5;
            vScrollBar.ValueChanged += VerticalScrollBar_ValueChanged;

            InitRightClickMenu();
        }

        #endregion

        private const int leftPadding = 15;
        
        private Keys keyState = Keys.None;

        private MouseState mouseState = MouseState.None;

        /// <summary>
        /// 纵向偏移量，纵向滚动条在最上时第1行显示完整该值=0，纵向滚动条往下拉时前几行被标尺等挡住整该值<![CDATA[<]]>0，运算取加法
        /// </summary>
        private int verticalOffset = 0;

        /// <summary>
        /// 鼠标距离多远时改变指针形状
        /// </summary>
        private float threshold = 5;

        /// <summary>
        /// 鼠标悬停的横线对应的行
        /// </summary>
        private Row hoverRow = null;

        /// <summary>
        /// 鼠标悬停的横线对应的行的上边Y坐标
        /// </summary>
        private int aboveHeight = 0;

        /// <summary>
        /// 鼠标悬停在上标尺方框里时距离左边的时间长度
        /// </summary>
        private double mouseToDisplayStart = 0;

        /// <summary>
        /// 鼠标悬停在上标尺方框里时距离右边的时间长度
        /// </summary>
        private double mouseToDisplayStop = 0;


        /// <summary>
        /// 上标尺的游标位置（下标尺的游标位置由时间决定，并且为了保证精度，不能再引用上游标位置）
        /// </summary>
        internal int MarkLinePosition
        {
            get
            {
                return UpperTimeToXPosition(MarkLine.Time);
            }
            set
            {
                MarkLine.Time = UpperXPositionToTime(value);
            }
        }

        private VScrollBar vScrollBar = new VScrollBar();
        private ToolStripMenuItem tsmi_ResetScale;
        private ToolStripMenuItem tsmi_ResetScale2;
        private ToolStripMenuItem tsmi_SetTime;
        private ToolStripMenuItem tsmi_SetTime2;
        private ToolStripMenuItem tsmi_SetTime3;
        private ToolStripMenuItem tsmi_ShowAllTrack;
        private ToolStripMenuItem tsmi_SegmentFill;
        
        #region 从分割条到垂直滚动条左边之间的区域，X坐标和时间互相转化

        /// <summary>
        /// 下标尺和轨道的时间转成X坐标
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        private int LowerTimeToXPosition(DateTime time)
        {
            return (int) ((time - Ruler.DisplayStart).TotalSeconds /
                   (Ruler.DisplayStop - Ruler.DisplayStart).TotalSeconds *
                   (this.Width - vScrollBar.Width - SplitLine.Position - SplitLine.Width)) + SplitLine.Position + SplitLine.Width;
        }

        /// <summary>
        /// 下标尺和轨道的X坐标转成时间
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        private DateTime LowerXPositionToTime(int x)
        {
            return Ruler.DisplayStart.AddSeconds(1d * (x - SplitLine.Position - SplitLine.Width) / 
                   (this.Width - vScrollBar.Width - SplitLine.Position - SplitLine.Width) *
                   (Ruler.DisplayStop - Ruler.DisplayStart).TotalSeconds);
        }

        /// <summary>
        /// 上标尺的时间转成X坐标
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        private int UpperTimeToXPosition(DateTime time)
        {
            return (int) ((time - Ruler.Start).TotalSeconds /
                   (Ruler.Stop - Ruler.Start).TotalSeconds *
                   (this.Width - vScrollBar.Width - SplitLine.Position - SplitLine.Width)) + SplitLine.Position + SplitLine.Width;
        }

        /// <summary>
        /// 上标尺的X坐标转成时间
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        private DateTime UpperXPositionToTime(int x)
        {
            return Ruler.Start.AddSeconds(1d * (x - SplitLine.Position - SplitLine.Width) /
                   (this.Width - vScrollBar.Width - SplitLine.Position - SplitLine.Width) *
                   (Ruler.Stop - Ruler.Start).TotalSeconds);
        }

        #endregion

        #region 画图方法


        /// <summary>
        /// 用<see cref="Invalidate"/>触发重绘整个页面
        /// </summary>
        /// <param name="e"></param>
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            RefreshPage(e.Graphics);
        }

        /// <summary>
        /// 重绘整个页面
        /// </summary>
        /// <param name="graphics"></param>
        private void RefreshPage(Graphics graphics)
        {
            graphics.Clear(BackColorRight);
            DrawBackColor(graphics);
            DrawRuler(graphics);
            DrawVerticalScrollBar();
            DrawTrack(graphics);
            DrawMarkLine(graphics);
            DrawRulerTick(graphics, UpperTick);
            DrawRulerTick(graphics, LowerTick);
            DrawSplitLine(graphics);
        }

        private void DrawRuler(Graphics graphics)
        {
            using (Brush brush = new SolidBrush(Ruler.BackColor))
            {
                graphics.FillRectangle(brush, SplitLine.Position, 0, this.Width - SplitLine.Position, Ruler.Height);
            }
            using (Pen pen = new Pen(BorderColor, BorderWidth))
            {
                graphics.DrawLine(pen, 0, Ruler.Height, this.Width, Ruler.Height);
                graphics.DrawLine(pen, 0, Ruler.UpperHeight, this.Width, Ruler.UpperHeight);
            }
            using (Font font = new Font(Ruler.Font, Ruler.FontSize, Ruler.FontStyle))
            using (Brush brush = new SolidBrush(Ruler.FontColor))
            {   
                graphics.DrawString(MarkLine.Time.ToString("dd MMM yyyy HH:mm:ss.fff", DateTimeFormatInfo.InvariantInfo),
                    font, brush, leftPadding, (Ruler.Height - Ruler.UpperHeight - font.Height) / 2 + Ruler.UpperHeight);
                graphics.DrawString("Scale: " + Ruler.Scale.ToString("f3"), font, brush, leftPadding, (Ruler.UpperHeight - font.Height) / 2);
            }
            using (Brush brush = new SolidBrush(Ruler.BoxColor))
            using (Pen pen = new Pen(Ruler.BoxBorderColor, Ruler.BoxBorderWidth))
            {
                int x1 = UpperTimeToXPosition(Ruler.DisplayStart);
                int x2 = UpperTimeToXPosition(Ruler.DisplayStop);
                graphics.FillRectangle(brush, x1, 0, x2 - x1, Ruler.UpperHeight);
                graphics.DrawRectangle(pen, x1, 0, x2 - x1, Ruler.UpperHeight);
            }

            UpperTick.Start = Ruler.Start;
            UpperTick.Stop = Ruler.Stop;
            UpperTick.XStart = SplitLine.Width + SplitLine.Position;
            UpperTick.XStop = this.Width - vScrollBar.Width;
            LowerTick.Start = Ruler.DisplayStart;
            LowerTick.Stop = Ruler.DisplayStop;
            LowerTick.XStart = SplitLine.Width + SplitLine.Position;
            LowerTick.XStop = this.Width - vScrollBar.Width;
        }


        private void DrawRulerTick(Graphics graphics, Tick tick)
        {
            double duration = (tick.Stop - tick.Start).TotalSeconds;
            double unit = 1;        // 累积单位长度
            double nextUnit = 1;    // 下个单位长度
            for (int i = 0; i < Tick.Unit.Length; ++i)
            {
                unit *= Tick.Unit[i];
                if ((tick.XStop - tick.XStart) / (duration / unit) < tick.Resolution)
                {
                    continue;
                }
                else
                {
                    if (i < Tick.Unit.Length - 1)
                    {
                        nextUnit = Tick.Unit[i + 1];
                    }
                    break;
                }
            }

            DateTime startDrawTime = Tick.GetNearestIntTime(tick.Start, unit);
            using (Pen pen = new Pen(tick.FontColor, 1))
            using (Brush brush = new SolidBrush(tick.FontColor))
            using (Font font = new Font(tick.Font, tick.FontSize, tick.FontStyle))
            {
                while (true)
                {
                    int x = tick.TimeToX(startDrawTime);
                    int y1;
                    int y2;
                    if (x <= tick.XStop)
                    {
                        y1 = tick.IsUpperAlign ? Ruler.UpperHeight : Ruler.Height;
                        // 判断小刻度还是大刻度
                        y2 = startDrawTime.TimeOfDay.TotalSeconds % (unit * nextUnit) == 0 ? y1 - tick.LongStickLength : y1 - tick.ShortStickLength;
                        graphics.DrawLine(pen, x, y1, x, y2);

                        if (startDrawTime.TimeOfDay.TotalSeconds % (unit * nextUnit) == 0)
                        {
                            graphics.DrawString(startDrawTime.ToString("HH:mm", DateTimeFormatInfo.InvariantInfo), font, brush,
                                x + 1, y2 - font.Height + 3);
                        }
                    }
                    else
                    {
                        break;
                    }
                    startDrawTime = startDrawTime.AddSeconds(unit);
                }
            }
        }



        private void DrawTrack(Graphics graphics)
        {
            if (Ruler.Start >= Ruler.Stop)
            {
                throw new Exception("Start time should be earlier than stop time.");
            }
            using (Pen pen = new Pen(BorderColor, BorderWidth))
            {
                // 前几行有可能被标尺行等挡住，只有下边不被挡住的才绘制
                int lineHeight = Ruler.Height + verticalOffset;
                for (int row = 0; row < Tracks.Count; ++row)
                {
                    lineHeight += Tracks[row].Height;
                    if (lineHeight > Ruler.Height)
                    {
                        graphics.DrawLine(pen, 0, lineHeight, this.Width, lineHeight);
                        int y1 = Math.Max(lineHeight - Tracks[row].Height, Ruler.Height);
                        int y2 = lineHeight;
                        using (Brush brush = new SolidBrush(Tracks[row].FontColor))
                        using (Font font = new Font(Tracks[row].Font, Tracks[row].FontSize, Tracks[row].FontStyle))
                        {
                            graphics.DrawString(Tracks[row].Text, font, brush, leftPadding, (y2 - y1 - font.Height) / 2 + y1);
                        }
                        for (int column = 0; column < Tracks[row].Segments.Count; ++column)
                        {
                            int x1 = Math.Max(LowerTimeToXPosition(Tracks[row].Segments[column].Start), SplitLine.Position + SplitLine.Width);
                            int x2 = LowerTimeToXPosition(Tracks[row].Segments[column].Stop);
                            using (Pen segPen = new Pen(Tracks[row].Segments[column].BorderColor, Tracks[row].Segments[column].BorderWidth))
                            using (Brush brush = new SolidBrush(Tracks[row].Segments[column].Color))
                            {
                                graphics.FillRectangle(brush, x1, y1, x2 - x1, y2 - y1);
                                graphics.DrawRectangle(segPen, x1, y1, x2 - x1, y2 - y1);
                            }
                            using (Brush brush = new SolidBrush(Tracks[row].Segments[column].FontColor))
                            using (Font font = new Font(Tracks[row].Segments[column].Font, Tracks[row].Segments[column].FontSize, Tracks[row].Segments[column].FontStyle))
                            {
                                graphics.DrawString(Tracks[row].Segments[column].Text, font, brush, x1 + 2, (y2 - y1 - font.Height) / 2 + y1);
                            }
                        }
                    }
                }
            }
        }


        private void DrawSplitLine(Graphics graphics)
        {
            using (Pen pen = new Pen(SplitLine.Color, SplitLine.Width))
            {
                graphics.DrawLine(pen, SplitLine.Position, 0, SplitLine.Position, this.Height);
            }
        }

        private void DrawMarkLine(Graphics graphics)
        {
            using (Pen pen = new Pen(MarkLine.Color, MarkLine.Width))
            using (Brush brush = new SolidBrush(MarkLine.Color))
            {
                int x1 = MarkLinePosition;
                graphics.DrawLine(pen, x1, 0, x1, Ruler.UpperHeight);
                PointF[] points = new PointF[]
                {
                    new PointF(x1, 7),
                    new PointF(x1 - 6, 0), 
                    new PointF(x1 + 5, 0), 
                };
                graphics.DrawPolygon(pen, points);
                graphics.FillPolygon(brush, points);

                int x2 = LowerTimeToXPosition(MarkLine.Time);
                if (x2 >= SplitLine.Position + SplitLine.Width)
                {
                    graphics.DrawLine(pen, x2, Ruler.UpperHeight + 1, x2, this.Height);
                    PointF[] points2 = new PointF[]
                    {
                        new PointF(x2, Ruler.UpperHeight + 7),
                        new PointF(x2 - 6, Ruler.UpperHeight + 1),
                        new PointF(x2 + 5, Ruler.UpperHeight + 1),
                    };
                    graphics.DrawPolygon(pen, points2);
                    graphics.FillPolygon(brush, points2);
                }
            }
        }

        private void DrawBackColor(Graphics graphics)
        {
            using (Brush brush = new SolidBrush(BackColorLeft))
            {
                graphics.FillRectangle(brush, 0, 0, SplitLine.Position, this.Height);
            }
        }

        private void DrawVerticalScrollBar()
        {
            vScrollBar.Location = new Point(this.Width - vScrollBar.Width, Ruler.Height);
            vScrollBar.Height = this.Height - Ruler.Height;
            vScrollBar.Minimum = 0;
            int trackHeight = 0;
            for (int row = 0; row < Tracks.Count; ++row)
            {
                trackHeight += Tracks[row].Height;
            }
            if (trackHeight <= this.Height - Ruler.Height)
            {
                verticalOffset = 0;
                vScrollBar.Maximum = this.Height - Ruler.Height;
                vScrollBar.LargeChange = this.Height - Ruler.Height;
            }
            else
            {
                vScrollBar.Maximum = trackHeight;
                vScrollBar.LargeChange = this.Height - Ruler.Height;
            }
        }

        private void VerticalScrollBar_ValueChanged(object sender, EventArgs e)
        {
            verticalOffset = -vScrollBar.Value;
            Invalidate();
        }

        #endregion

        #region 鼠标行为

        #region 判断鼠标位置

        private bool IsMouseAtSplitLine(int x)
        {
            if (Math.Abs(x - SplitLine.Position) <= threshold)
            {
                mouseState = MouseState.SplitLine;
                Cursor = Cursors.SizeWE;
                return true;
            }
            else
            {
                return false;
            }
        }

        private bool IsMouseAtMarkLine(int x, int y)
        {
            if (y <= Ruler.UpperHeight && Math.Abs(x - MarkLinePosition) <= threshold ||
                y > Ruler.UpperHeight && Math.Abs(x - LowerTimeToXPosition(MarkLine.Time)) <= threshold)
            {
                mouseState = MouseState.MarkLine;
                Cursor = Cursors.SizeWE;
                return true;
            }
            else
            {
                return false;
            }
        }

        private bool IsMouseAtRowLine(int y, out Row hoverRow_, out int aboveHeight_)
        {
            List<Row> rows = new List<Row>();
            rows.Add(Ruler);
            rows.AddRange(Tracks);
            int sumHeight = 0;
            aboveHeight_ = 0;
            hoverRow_ = null;
            for (int r = 0; r < rows.Count; ++r)
            {
                sumHeight += rows[r].Height;
                if (r == 1)
                {
                    sumHeight -= verticalOffset;
                }
                aboveHeight_ = sumHeight - rows[r].Height;
                if (Math.Abs(y - sumHeight) <= threshold)
                {
                    if (r == 0 || sumHeight > Ruler.Height)
                    {
                        mouseState = MouseState.RowLine;
                        hoverRow_ = rows[r];
                        Cursor = Cursors.SizeNS;
                        return true;
                    }
                }
            }
            return false;
        }

        private bool IsMouseAtBoxBorder(int x, int y)
        {
            if (y <= Ruler.UpperHeight)
            {
                if (Math.Abs(x - UpperTimeToXPosition(Ruler.DisplayStart)) <= threshold)
                {
                    mouseState = MouseState.BoxLeft;
                    Cursor = Cursors.SizeWE;
                    return true;
                }
                else if (Math.Abs(x - UpperTimeToXPosition(Ruler.DisplayStop)) <= threshold)
                {
                    mouseState = MouseState.BoxRight;
                    Cursor = Cursors.SizeWE;
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }


        private bool IsMouseInBox(int x, int y, out double mouseToBoxLeft_, out double mouseToBoxRight_)
        {
            DateTime hover = UpperXPositionToTime(x);
            double left = (hover - Ruler.DisplayStart).TotalSeconds;
            double right = (Ruler.DisplayStop - hover).TotalSeconds;
            mouseToBoxLeft_ = Math.Abs(left);
            mouseToBoxRight_ = Math.Abs(right);
            if (y <= Ruler.UpperHeight && left > 0 && right > 0)
            {
                mouseState = MouseState.Box;
                Cursor = Cursors.Hand;
                return true;
            }
            else
            {
                return false;
            }
        }

        #endregion


        private void DragSplitLine(int x)
        {
            SplitLine.Position = x;
            SplitLine.Position = Math.Max(SplitLine.Position, 10);
            SplitLine.Position = Math.Min(SplitLine.Position, this.Width - vScrollBar.Width - 10);
        }

        private void DragMarkLine(int x, int y)
        {
            x = Math.Max(x, SplitLine.Position + SplitLine.Width);
            x = Math.Min(x, this.Width - vScrollBar.Width);
            if (y <= Ruler.UpperHeight)
            {
                MarkLinePosition = x;
            }
            else
            {
                MarkLine.Time = LowerXPositionToTime(x);
            }
        }


        private void DragRowLine(int y, Row hoverRow, int aboveHeight)
        {
            hoverRow.Height = Math.Max(y - aboveHeight, 10);
        }


        private void DragBoxLeft(int x)
        {
            Ruler.DisplayStart = UpperXPositionToTime(x);
            Ruler.DisplayStart = DateTimeExt.Min(DateTimeExt.Max(Ruler.DisplayStart, Ruler.Start), Ruler.DisplayStop);
            if ((Ruler.DisplayStop - Ruler.DisplayStart).TotalSeconds < 10)
            {
                Ruler.DisplayStart = Ruler.DisplayStop.AddSeconds(-10);
            }
            Ruler.UpdateScale();
        }

        private void DragBoxRight(int x)
        {
            Ruler.DisplayStop = UpperXPositionToTime(x);
            Ruler.DisplayStop = DateTimeExt.Max(DateTimeExt.Min(Ruler.DisplayStop, Ruler.Stop), Ruler.DisplayStart);
            if ((Ruler.DisplayStop - Ruler.DisplayStart).TotalSeconds < 10)
            {
                Ruler.DisplayStop = Ruler.DisplayStart.AddSeconds(10);
            }
            Ruler.UpdateScale();
        }

        private void DragBox(DateTime x)
        {
            DateTime start = x.AddSeconds(-mouseToDisplayStart);
            DateTime stop = x.AddSeconds(mouseToDisplayStop);
            if (start < Ruler.Start || stop > Ruler.Stop)
            {
                return;
            }
            Ruler.DisplayStart = start;
            Ruler.DisplayStop = stop;
        }

        /// <summary>
        /// 点击轨道区域
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        private Track ClickTrack(int x, int y)
        {
            Track ret = null;
            int height = Ruler.Height + verticalOffset;
            for (int row = 0; row < Tracks.Count; ++row)
            {
                height += Tracks[row].Height;
                // 如果前几行被完全挡住了就不响应点击
                if (y >= Math.Max(height - Tracks[row].Height, Ruler.Height) && y <= height)
                {
                    ret = Tracks[row];
                }
                else
                {
                    for (int column = 0; column < Tracks[row].Segments.Count; ++column)
                    {
                        Tracks[row].Segments[column].IsSelected = false;
                    }
                }
            }
            mouseState = MouseState.ClickData;
            return ret;
        }

        /// <summary>
        /// 点击1条轨道里的段
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="track"></param>
        /// <returns></returns>
        private Segment ClickSegment(int x, int y, Track track)
        {
            Segment ret = null;
            if (track != null)
            {
                for (int column = 0; column < track.Segments.Count; ++column)
                {
                    int x1 = LowerTimeToXPosition(track.Segments[column].Start);
                    int x2 = LowerTimeToXPosition(track.Segments[column].Stop);
                    if (x >= x1 && x <= x2)
                    {
                        track.Segments[column].IsSelected = true;
                        ret = track.Segments[column];
                    }
                    else
                    {
                        track.Segments[column].IsSelected = false;
                    }
                }
            }
            return ret;
        }


        private void VertScroll(int delta)
        {
            if (delta < 0)
            {
                vScrollBar.Value = Math.Min(vScrollBar.Value + vScrollBar.SmallChange, vScrollBar.Maximum - vScrollBar.LargeChange + 1);
            }
            else if (delta > 0)
            {
                vScrollBar.Value = Math.Max(vScrollBar.Value - vScrollBar.SmallChange, vScrollBar.Minimum);
            }
        }

        
        private void HoriScale(int delta, int x)
        {
            if (delta < 0)
            {
                Ruler.Scale += 0.25;
            }
            else if (delta > 0)
            {
                Ruler.Scale = Math.Max(Ruler.Scale - 0.25, 1);
            }
            // 防止回到原始比例出现误差
            if (Math.Abs(Ruler.Scale - 1) < 0.01)
            {
                Ruler.Scale = 1;
                Ruler.DisplayStart = Ruler.Start;
            }
            DateTime mouseTime = LowerXPositionToTime(x);
            Ruler.DisplayStart = mouseTime.AddSeconds((Ruler.Start - mouseTime).TotalSeconds / Ruler.Scale);
            Ruler.DisplayStop = mouseTime.AddSeconds((Ruler.Stop - mouseTime).TotalSeconds / Ruler.Scale);
            Invalidate();
        }

        private void HoriScroll(int delta)
        {
            double deltaSeconds = (Ruler.Stop - Ruler.Start).TotalSeconds / 50;
            double seconds = (Ruler.DisplayStop - Ruler.DisplayStart).TotalSeconds;
            if (delta < 0)
            {
                Ruler.DisplayStop = DateTimeExt.Min(Ruler.DisplayStop.AddSeconds(deltaSeconds), Ruler.Stop);
                Ruler.DisplayStart = Ruler.DisplayStop.AddSeconds(-seconds);
            }
            else if (delta > 0)
            {
                Ruler.DisplayStart = DateTimeExt.Max(Ruler.DisplayStart.AddSeconds(-deltaSeconds), Ruler.Start);
                Ruler.DisplayStop = Ruler.DisplayStart.AddSeconds(seconds);
            }
            Invalidate();
        }
        

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            // 避免鼠标拖太快，刷新跟不上，图形不跟随鼠标，所以只在无按键且移动时改变状态
            if (e.Button == MouseButtons.None)
            {
                // 判断游标
                if (IsMouseAtMarkLine(e.X, e.Y))
                {
                }
                // 判断上标尺方框边界
                else if (IsMouseAtBoxBorder(e.X, e.Y))
                {
                }
                // 判断上标尺方框
                else if (IsMouseInBox(e.X, e.Y, out mouseToDisplayStart, out mouseToDisplayStop))
                {
                }
                // 判断分割线
                else if (IsMouseAtSplitLine(e.X))
                {
                }
                // 判断横线
                else if (IsMouseAtRowLine(e.Y, out hoverRow, out aboveHeight))
                {
                }
                else
                {
                    mouseState = MouseState.None;
                    Cursor = Cursors.Arrow;
                }
            }
            else if ((e.Button & MouseButtons.Left) > 0)
            {
                
                switch (mouseState)
                {
                    case MouseState.MarkLine:
                        DragMarkLine(e.X, e.Y);
                        break;

                    case MouseState.SplitLine:
                        DragSplitLine(e.X);
                        break;

                    case MouseState.RowLine:
                        DragRowLine(e.Y, hoverRow, aboveHeight);
                        break;

                    case MouseState.BoxLeft:
                        DragBoxLeft(e.X);
                        break;

                    case MouseState.BoxRight:
                        DragBoxRight(e.X);
                        break;

                    case MouseState.Box:
                        DragBox(UpperXPositionToTime(e.X));
                        break;

                    default:
                        break;
                }

                switch (mouseState)
                {
                    case MouseState.MarkLine:
                    case MouseState.SplitLine:
                    case MouseState.RowLine:
                    case MouseState.BoxLeft:
                    case MouseState.BoxRight:
                    case MouseState.Box:
                        Invalidate();
                        break;

                    default:
                        break;
                }
            }
        }

        protected override void OnMouseClick(MouseEventArgs e)
        {
            base.OnMouseClick(e);
            RightClickData data = new RightClickData();
            Track track = null;
            Segment segment = null;

            if ((e.Button & (MouseButtons.Left | MouseButtons.Right)) > 0 && e.X >= SplitLine.Position + SplitLine.Width && e.Y > Ruler.Height)
            {
                track = ClickTrack(e.X, e.Y);
                segment = ClickSegment(e.X, e.Y, track);
                if ((e.Button & MouseButtons.Right) > 0)
                {
                    data.MarkTime = LowerXPositionToTime(e.X);
                    data.Track = track;
                    data.Segment = segment;
                    data.Position = e.Location;
                    if (segment != null)
                    {
                        SegmentMenu.Tag = data;
                        SegmentMenu.Show(MousePosition);
                    }
                    else
                    {
                        TrackBlankMenu.Tag = data;
                        TrackBlankMenu.Show(MousePosition);
                    }
                }
            }
            else if ((e.Button & MouseButtons.Right) > 0 && e.X <= SplitLine.Position && e.Y > Ruler.Height)
            {
                track = ClickTrack(e.X, e.Y);
                data.Track = track;
                TrackHeaderMenu.Tag = data;
                TrackHeaderMenu.Show(MousePosition);
            }
            else if ((e.Button & MouseButtons.Right) > 0 && e.X >= SplitLine.Position + SplitLine.Width && e.Y <= Ruler.Height)
            {
                data.MarkTime = LowerXPositionToTime(e.X);
                data.Position = e.Location;
                RulerMenu.Tag = data;
                RulerMenu.Show(MousePosition);
            }
            else if ((e.Button & MouseButtons.Right) > 0 && e.X <= SplitLine.Position && e.Y <= Ruler.Height)
            {
                RulerHeaderMenu.Tag = data;
                RulerHeaderMenu.Show(MousePosition);
            }
            else
            {
                mouseState = MouseState.None;
            }

            switch (mouseState)
            {
                case MouseState.ClickData:
                    Invalidate();
                    break;

                default:
                    break;
            }
        }


        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);
            if (e.Y > Ruler.Height && keyState == Keys.ShiftKey)
            {
                VertScroll(e.Delta);
            }
            else if (e.X >= SplitLine.Width + SplitLine.Position && keyState == Keys.Menu)
            {
                HoriScale(e.Delta, e.X);
            }
            else if (e.Y > Ruler.Height && keyState == Keys.None)
            {
                HoriScroll(e.Delta);
            }
        }

        #region 右键菜单

        private void InitRightClickMenu()
        {
            tsmi_ResetScale = new ToolStripMenuItem("Reset Scale");
            tsmi_ResetScale2 = new ToolStripMenuItem("Reset Scale");
            tsmi_SetTime = new ToolStripMenuItem("Set Time Here");
            tsmi_SetTime2 = new ToolStripMenuItem("Set Time Here");
            tsmi_SetTime3 = new ToolStripMenuItem("Set Time Here");
            tsmi_ShowAllTrack = new ToolStripMenuItem("Restore Hidden Items");
            tsmi_SegmentFill = new ToolStripMenuItem("Fill");
            RulerHeaderMenu.Items.Add(tsmi_ResetScale);
            RulerMenu.Items.Add(tsmi_SetTime);
            RulerMenu.Items.Add(tsmi_ResetScale2);
            TrackHeaderMenu.Items.Add(tsmi_ShowAllTrack);
            TrackBlankMenu.Items.Add(tsmi_SetTime2);
            SegmentMenu.Items.Add(tsmi_SetTime3);
            SegmentMenu.Items.Add(tsmi_SegmentFill);
            RulerHeaderMenu.Opening += ContextMenuStrip_Opening;
            RulerMenu.Opening += ContextMenuStrip_Opening;
            TrackHeaderMenu.Opening += ContextMenuStrip_Opening;
            TrackBlankMenu.Opening += ContextMenuStrip_Opening;
            SegmentMenu.Opening += ContextMenuStrip_Opening;
            tsmi_ResetScale.Click += Tsmi_ResetScale_Click;
            tsmi_ResetScale2.Click += Tsmi_ResetScale_Click;
            tsmi_SetTime.Click += Tsmi_SetTime_Click;
            tsmi_SetTime2.Click += Tsmi_SetTime_Click;
            tsmi_SetTime3.Click += Tsmi_SetTime_Click;
            tsmi_ShowAllTrack.Click += Tsmi_ShowAllTrack_Click;
            tsmi_SegmentFill.Click += Tsmi_SegmentFill_Click;
        }

        private void ContextMenuStrip_Opening(object sender, CancelEventArgs e)
        {
            ContextMenuStrip cms = sender as ContextMenuStrip;
            for (int i = 0; i < cms.Items.Count; ++i)
            {
                cms.Items[i].Tag = cms.Tag;
            }
        }

        private void Tsmi_ResetScale_Click(object sender, EventArgs e)
        {
            Ruler.ResetScale();
            Invalidate();
        }

        private void Tsmi_SetTime_Click(object sender, EventArgs e)
        {
            RightClickData data = (sender as ToolStripMenuItem).Tag as RightClickData;
            MarkLine.Time = data.Position.Y <= Ruler.UpperHeight ? UpperXPositionToTime(data.Position.X) : LowerXPositionToTime(data.Position.X);
            data.MarkTime = MarkLine.Time;
            Invalidate();
        }


        private void Tsmi_ShowAllTrack_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < Tracks.Count; ++i)
            {
                Tracks[i].IsShow = true;
            }
            Invalidate();
        }

        private void Tsmi_SegmentFill_Click(object sender, EventArgs e)
        {
            RightClickData data = (sender as ToolStripMenuItem).Tag as RightClickData;
            Ruler.DisplayStart = data.Segment.Start;
            Ruler.DisplayStop = data.Segment.Stop;
            Invalidate();
        }


        #endregion

        #endregion


        #region 键盘行为

        protected override void OnKeyDown(KeyEventArgs e)
        {
            // 按Alt会让菜单栏获取焦点，所以先标记处理过
            if (e.KeyCode == Keys.Menu)
            {
                e.Handled = true;
            }
            base.OnKeyDown(e);
            switch (e.KeyCode)
            {
                case Keys.ShiftKey:
                case Keys.ControlKey:
                case Keys.Menu:     // alt
                    keyState = e.KeyCode;
                    break;

                default:
                    keyState = Keys.None;
                    break;
            }
        }

        protected override void OnKeyUp(KeyEventArgs e)
        {
            base.OnKeyUp(e);
            keyState = Keys.None;
        }

        #endregion
    }
}
