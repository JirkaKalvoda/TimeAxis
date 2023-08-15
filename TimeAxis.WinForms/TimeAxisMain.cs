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

        
        #endregion
        
        public TimeAxisMain()
        {
            InitializeComponent();
            SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint | ControlStyles.ResizeRedraw, true);
            Ruler.Start = DateTime.Today;
            Ruler.Stop = DateTime.Today.AddDays(1);
            MarkLine.Time = Ruler.DisplayStart;
            // 滚动条不会被清除
            this.Controls.Add(vScrollBar);
            vScrollBar.Width = ScrollWidth;
            vScrollBar.SmallChange = 5;
            vScrollBar.ValueChanged += VerticalScrollBar_ValueChanged;
        }

        #endregion
        
        private VScrollBar vScrollBar = new VScrollBar();

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
        /// 鼠标悬停的横线对应的行的上边纵坐标
        /// </summary>
        private int aboveHeight = 0;

        /// <summary>
        /// 上标尺的游标位置（下标尺的游标位置可以通过时间换算）
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

        #region 从分割条到垂直滚动条左边之间的区域，横坐标和时间互相转化

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
                    font, brush, 15, (Ruler.Height - Ruler.UpperHeight - font.Height) / 2 + Ruler.UpperHeight);
            }
            // todo
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
                        for (int column = 0; column < Tracks[row].Segments.Count; ++column)
                        {
                            int x1 = Math.Max(LowerTimeToXPosition(Tracks[row].Segments[column].Start), SplitLine.Position + SplitLine.Width);
                            int x2 = LowerTimeToXPosition(Tracks[row].Segments[column].Stop);
                            int y1 = Math.Max(lineHeight - Tracks[row].Height, Ruler.Height);
                            int y2 = lineHeight;
                            using (Pen segPen = new Pen(Tracks[row].Segments[column].BorderColor, Tracks[row].Segments[column].BorderWidth))
                            using (Brush brush = new SolidBrush(Tracks[row].Segments[column].Color))
                            {
                                graphics.FillRectangle(brush, x1, y1, x2 - x1, y2 - y1);
                                graphics.DrawRectangle(segPen, x1, y1, x2 - x1, y2 - y1);
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
                    new PointF(x1, Ruler.UpperHeight - 7),
                    new PointF(x1 - 6, Ruler.UpperHeight), 
                    new PointF(x1 + 5, Ruler.UpperHeight), 
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

        private bool IsMouseAtRowLine(int y, out Row hoverRow, out int aboveHeight)
        {
            List<Row> rows = new List<Row>();
            rows.Add(Ruler);
            rows.AddRange(Tracks);
            int sumHeight = 0;
            aboveHeight = 0;
            hoverRow = null;
            for (int r = 0; r < rows.Count; ++r)
            {
                sumHeight += rows[r].Height;
                if (r == 1)
                {
                    sumHeight -= verticalOffset;
                }
                aboveHeight = sumHeight - rows[r].Height;
                if (Math.Abs(y - sumHeight) <= threshold)
                {
                    if (r == 0 || sumHeight > Ruler.Height)
                    {
                        mouseState = MouseState.RowLine;
                        hoverRow = rows[r];
                        Cursor = Cursors.SizeNS;
                        return true;
                    }
                }
            }
            return false;
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
            if (y <= Ruler.UpperHeight)
            {
                MarkLinePosition = x;
            }
            else
            {
                MarkLinePosition = UpperTimeToXPosition(LowerXPositionToTime(x));
            }
            MarkLinePosition = Math.Max(MarkLinePosition, SplitLine.Position + SplitLine.Width);
            MarkLinePosition = Math.Min(MarkLinePosition, this.Width - vScrollBar.Width);
        }

        private void DragRowLine(int y, Row hoverRow, int aboveHeight)
        {
            hoverRow.Height = Math.Max(y - aboveHeight, 10);
        }

        private void ClickSegment(int x, int y)
        {
            int height = Ruler.Height + verticalOffset;
            for (int row = 0; row < Tracks.Count; ++row)
            {
                height += Tracks[row].Height;
                // 如果前几行被完全挡住了就不响应点击
                if (y >= Math.Max(height - Tracks[row].Height, Ruler.Height) && y <= height)
                {
                    for (int column = 0; column < Tracks[row].Segments.Count; ++column)
                    {
                        int x1 = LowerTimeToXPosition(Tracks[row].Segments[column].Start);
                        int x2 = LowerTimeToXPosition(Tracks[row].Segments[column].Stop);
                        if (x >= x1 && x <= x2)
                        {
                            Tracks[row].Segments[column].IsSelected = true;
                        }
                        else
                        {
                            Tracks[row].Segments[column].IsSelected = false;
                        }
                    }
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
            // 防止到原始比例出现误差
            if (Math.Abs(Ruler.Scale - 1) < 0.01)
            {
                Ruler.Scale = 1;
                Ruler.DisplayStart = Ruler.Start;
            }
            DateTime mouseTime = LowerXPositionToTime(x);
            Ruler.DisplayStart = mouseTime.AddSeconds(-(mouseTime - Ruler.Start).TotalSeconds / Ruler.Scale);
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

                    default:
                        break;
                }

                switch (mouseState)
                {
                    case MouseState.MarkLine:
                    case MouseState.SplitLine:
                    case MouseState.RowLine:
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
            if ((e.Button | MouseButtons.Left | MouseButtons.Right) > 0 && e.X >= SplitLine.Position + SplitLine.Width)
            {
                ClickSegment(e.X, e.Y);
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
