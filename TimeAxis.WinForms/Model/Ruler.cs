﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimeAxis
{
    /// <summary>
    /// 标尺类，分上下2个尺，上排是真正的起止时刻，下排是显示部分的起止时刻
    /// </summary>
    public class Ruler : Row
    {
        /// <summary>
        /// 放大倍数，目前>=1
        /// </summary>
        public double Scale { get; set; } = 1;

        public DateTime Start { get; set; }

        public DateTime Stop { get; set; }

        public DateTime DisplayStart { get; set; }
        
        public DateTime DisplayStop { get; set; }

        public string Font { get; set; } = "Calibri";

        public float FontSize { get; set; } = 13;

        public Color FontColor { get; set; } = Color.Black;

        public FontStyle FontStyle { get; set; } = FontStyle.Bold;

        public Color BackColor { get; set; } = Color.DeepSkyBlue;

        public int UpperHeight
        {
            get
            {
                return Height / 2;
            }
        }

        public Ruler()
        {
            Start = DateTime.Today;
            Stop = DateTime.Today.AddDays(1);
            DisplayStart = Start;
            DisplayStop = Stop;
            Height = 60;
        }
    }
}