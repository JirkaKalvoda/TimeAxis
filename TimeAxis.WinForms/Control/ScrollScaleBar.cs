using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TimeAxis
{
    /// <summary>
    /// 带放大功能的滚动条
    /// <br>该控件缩放时假定总数据范围不变、修改显示范围，实际也可能修改总数据范围，所以目前是在使用处触发重绘滚动条</br>
    /// </summary>
    public partial class ScrollScaleBar : UserControl
    {
        public bool IsVertical { get; set; } = true;

        /// <summary>
        /// 长边像素
        /// </summary>
        public int Length
        {
            get
            {
                return IsVertical ? Height : Width;
            }
            set
            {
                if (IsVertical)
                {
                    Height = value;
                }
                else
                {
                    Width = value;
                }
            }
        }

        /// <summary>
        /// 短边像素和按钮边长
        /// </summary>
        public int Thickness
        {
            get
            {
                return IsVertical ? Width : Height;
            }
            set
            {
                if (IsVertical)
                {
                    Width = value;
                }
                else
                {
                    Height = value;
                }
            }
        }

        /// <summary>
        /// 总数据最小值
        /// </summary>
        public double Start { get; set; } = 1;

        /// <summary>
        /// 总数据最大值
        /// </summary>
        public double Stop { get; set; } = 2;

        /// <summary>
        /// 显示数据最小值
        /// </summary>
        public double DisplayStart { get; set; } = 1;

        /// <summary>
        /// 显示数据最大值
        /// </summary>
        public double DisplayStop { get; set; } = 2;

        /// <summary>
        /// 显示数据范围相对于上1帧的变化情况，注意不能在get里动态算值
        /// </summary>
        public double DisplayChangeRate { get; private set; }
        
        /// <summary>
        /// 显示数据范围相对于总数据范围的放大倍数
        /// </summary>
        public double Scale
        {
            get
            {
                return (Stop - Start) / (DisplayStop - DisplayStart);
            }
        }

        /// <summary>
        /// 最大放大倍数，防止计算和画图溢出
        /// </summary>
        public double ScaleMax { get; set; } = 4;

        public override Color BackColor { get; set; } = Color.DimGray;

        public override Color ForeColor { get; set; } = Color.DarkGray;
        
        /// <summary>
        /// 圆环颜色
        /// </summary>
        public Color LightColor { get; set; } = Color.LightGray;
        
        /// <summary>
        /// 点箭头移动多少像素
        /// </summary>
        public int SmallChange { get; set; } = 5;

        /// <summary>
        /// 点击箭头或拖动滚动条触发事件
        /// </summary>
        public event EventHandler<EventArgs> BarMoved;

        /// <summary>
        /// 拖动滚动条最小端圆环触发事件
        /// </summary>
        public event EventHandler<EventArgs> RingMinDragged;

        /// <summary>
        /// 拖动滚动条最大端圆环触发事件
        /// </summary>
        public event EventHandler<EventArgs> RingMaxDragged;

        private MouseState mouseState;

        private double mouseToDisplayStart;

        private double mouseToDisplayStop;


        public ScrollScaleBar()
        {
            InitializeComponent();
            SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint | ControlStyles.ResizeRedraw, true);
            this.Thickness = 14;
        }

        //public override void Refresh()
        //{
        //    Invalidate();
        //}

        private double PositionToValue(float p)
        {
            return Start + 1d * (p - Thickness * 2) / (Length - Thickness * 4) * (Stop - Start);
        }

        private float ValueToPosition(double value)
        {
            return (float) ((value - Start) / (Stop - Start) * (Length - Thickness * 4) + Thickness * 2);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            RefreshObjects(e.Graphics);
        }

        private void RefreshObjects(Graphics graphics)
        {
            graphics.Clear(BackColor);
            float start = ValueToPosition(DisplayStart);
            float stop = ValueToPosition(DisplayStop);
            using (SolidBrush brushFore = new SolidBrush(ForeColor))
            using (SolidBrush brushLight = new SolidBrush(LightColor))
            {
                if (IsVertical)
                {
                    graphics.FillPolygon(brushFore, new PointF[]
                    {
                        new PointF(Thickness / 2f, Thickness * 0.2f),
                        new PointF(Thickness * 0.2f, Thickness * 0.8f),
                        new PointF(Thickness * 0.8f, Thickness * 0.8f), 
                    });
                    graphics.FillPolygon(brushFore, new PointF[]
                    {
                        new PointF(Thickness / 2f, Length - Thickness * 0.2f),
                        new PointF(Thickness * 0.2f, Length - Thickness * 0.8f),
                        new PointF(Thickness * 0.8f, Length - Thickness * 0.8f),
                    });
                    // 滚动条和圆环之间的区域填色
                    graphics.FillRectangle(brushFore, 0, start, Thickness, stop - start);
                    graphics.FillRectangle(brushFore, 0, start - Thickness / 2f, Thickness, Thickness / 2f);
                    graphics.FillRectangle(brushFore, 0, stop, Thickness, Thickness / 2f);

                    graphics.FillEllipse(brushLight, 0, start - Thickness, Thickness, Thickness);
                    graphics.FillEllipse(brushLight, 0, stop, Thickness, Thickness);
                    graphics.FillEllipse(brushFore, Thickness * 0.2f, start - Thickness * 0.8f, Thickness * 0.6f, Thickness * 0.6f);
                    graphics.FillEllipse(brushFore, Thickness * 0.2f, stop + Thickness * 0.2f, Thickness * 0.6f, Thickness * 0.6f);
                }
                else
                {
                    graphics.FillPolygon(brushFore, new PointF[]
                    {
                        new PointF(Thickness * 0.2f, Thickness / 2f),
                        new PointF(Thickness * 0.8f, Thickness * 0.2f),
                        new PointF(Thickness * 0.8f, Thickness * 0.8f),
                    });
                    graphics.FillPolygon(brushFore, new PointF[]
                    {
                        new PointF(Length - Thickness * 0.2f, Thickness / 2f),
                        new PointF(Length - Thickness * 0.8f, Thickness * 0.2f),
                        new PointF(Length - Thickness * 0.8f, Thickness * 0.8f),
                    });
                    // 滚动条和圆环之间的区域填色
                    graphics.FillRectangle(brushFore, start, 0, stop - start, Thickness);
                    graphics.FillRectangle(brushFore, start - Thickness / 2f, 0, Thickness / 2f, Thickness);
                    graphics.FillRectangle(brushFore, stop, 0, Thickness / 2f, Thickness);

                    graphics.FillEllipse(brushLight, start - Thickness, 0, Thickness, Thickness);
                    graphics.FillEllipse(brushLight, stop, 0, Thickness, Thickness);
                    graphics.FillEllipse(brushFore, start - Thickness * 0.8f, Thickness * 0.2f, Thickness * 0.6f, Thickness * 0.6f);
                    graphics.FillEllipse(brushFore, stop + Thickness * 0.2f, Thickness * 0.2f, Thickness * 0.6f, Thickness * 0.6f);
                }
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            // 避免鼠标拖太快，刷新跟不上，图形不跟随鼠标，所以只在无按键且移动时改变状态
            if (e.Button == MouseButtons.None)
            {
                // 判断箭头
                if (IsMouseAtArrow(e.X, e.Y)) { }
                // 判断圆环
                else if (IsMouseAtRing(e.X, e.Y)) { }
                // 判断滚动条
                else if (IsMouseInBar(e.X, e.Y, out mouseToDisplayStart, out mouseToDisplayStop)) { }
                else
                {
                    Cursor = Cursors.Arrow;
                    mouseState = MouseState.None;
                }
            }
            else if ((e.Button & MouseButtons.Left) > 0)
            {
                switch (mouseState)
                {
                    case MouseState.ScrollBar:
                        DragBar(e.X, e.Y);
                        break;

                    case MouseState.ScrollRingMin:
                        DragRingMin(e.X, e.Y);
                        break;

                    case MouseState.ScrollRingMax:
                        DragRingMax(e.X, e.Y);
                        break;

                    default:
                        break;
                }

                switch (mouseState)
                {
                    case MouseState.ScrollBar:
                        BarMoved?.Invoke(this, null);
                        break;

                    case MouseState.ScrollRingMin:
                        RingMinDragged ?.Invoke(this, null);
                        break;

                    case MouseState.ScrollRingMax:
                        RingMaxDragged?.Invoke(this, null);
                        break;

                    default:
                        break;
                }
            }
        }

        protected override void OnMouseClick(MouseEventArgs e)
        {
            base.OnMouseClick(e);
            if ((e.Button & MouseButtons.Left) > 0)
            {
                switch (mouseState)
                {
                    case MouseState.ScrollArrowMin:
                        ClickArrowMin();
                        break;

                    case MouseState.ScrollArrowMax:
                        ClickArrowMax();
                        break;

                    default:
                        break;
                }

                switch (mouseState)
                {
                    case MouseState.ScrollArrowMin:
                    case MouseState.ScrollArrowMax:
                        BarMoved?.Invoke(this, null);
                        break;

                    default:
                        break;
                }
            }
        }

        private bool IsMouseAtArrow(int x, int y)
        {
            int p = IsVertical ? y : x;
            if (p <= Thickness)
            {
                mouseState = MouseState.ScrollArrowMin;
                return true;
            }
            else if (p >= Length - Thickness)
            {
                mouseState = MouseState.ScrollArrowMax;
                return true;
            }
            return false;
        }

        private bool IsMouseAtRing(int x, int y)
        {
            int p = IsVertical ? y : x;
            float start = ValueToPosition(DisplayStart);
            float stop = ValueToPosition(DisplayStop);
            if (p <= start && p >= start - Thickness)
            {
                mouseState = MouseState.ScrollRingMin;
                Cursor = IsVertical ? Cursors.SizeNS : Cursors.SizeWE;
                return true;
            }
            else if (p >= stop && p <= stop + Thickness)
            {
                mouseState = MouseState.ScrollRingMax;
                Cursor = IsVertical ? Cursors.SizeNS : Cursors.SizeWE;
                return true;
            }
            Cursor = Cursors.Arrow;
            return false;
        }

        private bool IsMouseInBar(int x, int y, out double mouseToDisplayStart_, out double mouseToDisplayStop_)
        {
            int p = IsVertical ? y : x;
            float start = ValueToPosition(DisplayStart);
            float stop = ValueToPosition(DisplayStop);
            mouseToDisplayStart_ = double.NaN;
            mouseToDisplayStop_ = double.NaN;
            if (p <= stop && p >= start)
            {
                mouseState = MouseState.ScrollBar;
                double v = PositionToValue(p);
                mouseToDisplayStart_ = v - DisplayStart;
                mouseToDisplayStop_ = DisplayStop - v;
                return true;
            }
            return false;
        }

        private void ClickArrowMin()
        {
            DisplayChangeRate = 1;
            double temp = DisplayStop - DisplayStart;
            DisplayStart = Math.Max(Start, PositionToValue(ValueToPosition(DisplayStart) - SmallChange));
            DisplayStop = DisplayStart + temp;
        }

        private void ClickArrowMax()
        {
            DisplayChangeRate = 1;
            double temp = DisplayStop - DisplayStart;
            DisplayStop = Math.Min(Stop, PositionToValue(ValueToPosition(DisplayStop) + SmallChange));
            DisplayStart = DisplayStop - temp;
        }

        private void DragBar(int x, int y)
        {
            DisplayChangeRate = 1;
            int p = IsVertical ? y : x;
            double v = PositionToValue(p);
            double start = v - mouseToDisplayStart;
            double stop = v + mouseToDisplayStop;
            if (start < Start || stop > Stop)
            {
                return;
            }
            DisplayStart = start;
            DisplayStop = stop;
        }

        private void DragRingMin(int x, int y)
        {
            int p = IsVertical ? y : x;
            double d1 = DisplayStop - DisplayStart;
            DisplayStart = Math.Max(Start, PositionToValue(p + Thickness));
            // 拖动过大导致的倍数过大或差值变负
            if (Scale >= ScaleMax || Scale <= 0)
            {
                DisplayStart = DisplayStop - (Stop - Start) / ScaleMax;
            }
            double d2 = DisplayStop - DisplayStart;
            DisplayChangeRate = d2 / d1;
            //System.Diagnostics.Debug.WriteLine(string.Format("{0} {1} {2} {3} {4} {5} {6}",
            //    DisplayChangeRate.ToString().PadLeft(20),
            //    DisplayStart.ToString().PadLeft(20),
            //    DisplayStop.ToString().PadLeft(20),
            //    Start.ToString().PadLeft(20),
            //    Stop.ToString().PadLeft(20),
            //    p,
            //    Scale));
        }

        private void DragRingMax(int x, int y)
        {
            int p = IsVertical ? y : x;
            double d1 = DisplayStop - DisplayStart;
            DisplayStop = Math.Min(Stop, PositionToValue(p - Thickness));
            // 拖动过大导致的倍数过大或差值变负
            if (Scale >= ScaleMax || Scale <= 0)
            {
                DisplayStop = DisplayStart + (Stop - Start) / ScaleMax;
            }
            double d2 = DisplayStop - DisplayStart;
            DisplayChangeRate = d2 / d1;
            //System.Diagnostics.Debug.WriteLine(string.Format("{0} {1} {2} {3} {4} {5} {6}",
            //    DisplayChangeRate.ToString().PadLeft(20),
            //    DisplayStart.ToString().PadLeft(20),
            //    DisplayStop.ToString().PadLeft(20),
            //    Start.ToString().PadLeft(20),
            //    Stop.ToString().PadLeft(20),
            //    p,
            //    Scale));
        }

    }
}
