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

        
        //EventArgs

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

        private List<Segment> allSegments = new List<Segment>();

        private List<int> allRowPositions = new List<int>();

        private Row hoverRow = null;
        
        private int aboveHeight = 0;

        internal int MarkLinePosition
        {
            get
            {
                return TimeToXPosition(MarkLine.Time);
            }
            set
            {
                MarkLine.Time = XPositionToTime(value);
            }
        }

        private int TimeToXPosition(DateTime time)
        {
            return (int) ((time - Ruler.DisplayStart).TotalSeconds /
                          (Ruler.DisplayStop - Ruler.DisplayStart).TotalSeconds *
                          (this.Width - vScrollBar.Width - SplitLine.Position - SplitLine.Width)) + SplitLine.Position + SplitLine.Width;
        }

        private DateTime XPositionToTime(int x)
        {
            return Ruler.DisplayStart.AddSeconds(1d * (x - SplitLine.Position - SplitLine.Width) / 
                    (this.Width - vScrollBar.Width - SplitLine.Position - SplitLine.Width) *
                    (Ruler.DisplayStop - Ruler.DisplayStart).TotalSeconds);
        }

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
            using (Pen pen = new Pen(BorderColor, BorderWidth))
            {
                graphics.DrawLine(pen, 0, Ruler.Height, this.Width, Ruler.Height);
            }
            using (Font font = new Font(Ruler.Font, Ruler.FontSize, Ruler.FontStyle))
            using (Brush brush = new SolidBrush(Ruler.FontColor))
            {
                graphics.DrawString(MarkLine.Time.ToString("dd MMM yyyy hh:mm:ss.fff", DateTimeFormatInfo.InvariantInfo),
                    font, brush, 20, (Ruler.Height - font.Height) / 2);
            }
            // todo
        }

        private void DrawTrack(Graphics graphics)
        {
            allSegments.Clear();
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
                            allSegments.Add(Tracks[row].Segments[column]);
                            int x1 = TimeToXPosition(Tracks[row].Segments[column].Start);
                            int x2 = TimeToXPosition(Tracks[row].Segments[column].Stop);
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
                graphics.DrawLine(pen, MarkLinePosition, 0, MarkLinePosition, this.Height);
                PointF[] points = new PointF[]
                {
                    new PointF(MarkLinePosition, 7),
                    new PointF(MarkLinePosition - 6, 0), 
                    new PointF(MarkLinePosition + 5, 0), 
                };
                graphics.DrawPolygon(pen, points);
                graphics.FillPolygon(brush, points);
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

        private bool IsMouseAtMarkLine(int x)
        {
            if (Math.Abs(x - MarkLinePosition) <= threshold)
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

        private void DragMarkLine(int x)
        {
            MarkLinePosition = x;
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
                        int x1 = TimeToXPosition(Tracks[row].Segments[column].Start);
                        int x2 = TimeToXPosition(Tracks[row].Segments[column].Stop);
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

        
        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            // 避免鼠标拖太快，刷新跟不上，图形不跟随鼠标，所以只在无按键且移动时改变状态
            if (e.Button == MouseButtons.None)
            {
                // 判断标线
                if (IsMouseAtMarkLine(e.X))
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
                        DragMarkLine(e.X);
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
                if (e.Delta < 0)
                {
                    vScrollBar.Value = Math.Min(vScrollBar.Value + vScrollBar.SmallChange, vScrollBar.Maximum - vScrollBar.LargeChange + 1);
                }
                else if (e.Delta > 0)
                {
                    vScrollBar.Value = Math.Max(vScrollBar.Value - vScrollBar.SmallChange, vScrollBar.Minimum);
                }
            }
        }

        #endregion


        #region 键盘行为

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            switch (e.KeyCode)
            {
                case Keys.ShiftKey:
                case Keys.ControlKey:
                case Keys.Menu:     // alt
                    keyState = e.KeyCode;
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
