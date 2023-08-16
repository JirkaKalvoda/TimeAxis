using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimeAxis.Model
{
    /// <summary>
    /// 标尺里的刻度类
    /// </summary>
    public class Tick
    {
        public DateTime Start { get; set; }

        public DateTime Stop { get; set; }

        /// <summary>
        /// 最小X坐标
        /// </summary>
        public int XStart { get; set; }
        
        /// <summary>
        /// 最大X坐标
        /// </summary>
        public int XStop { get; set; }

        /// <summary>
        /// 上标尺/下标尺
        /// </summary>
        public bool IsUpperAlign { get; set; } = true;

        public int LongStickLength { get; set; } = 10;

        public int ShortStickLength { get; set; } = 5;

        /// <summary>
        /// 每个最小间隔横向至少要多少像素
        /// </summary>
        public int Resolution { get; set; } = 4;

        /// <summary>
        /// 时间转化为X坐标的委托
        /// </summary>
        public Func<DateTime, int> TimeToX { get; set; }

        /// <summary>
        /// 每个最小间隔代表多少秒，取值是从第0个起往后连乘
        /// </summary>
        public static double[] Unit = {10, 6, 10, 6, 2, 3, 4, 2, 5};

        /// <summary>
        /// 获取最近的整的时间，整数或整10等
        /// </summary>
        /// <param name="input"></param>
        /// <param name="unit">累积单位长度</param>
        /// <returns></returns>
        public static DateTime GetNearestIntTime(DateTime input, double unit)
        {
            double duration = input.TimeOfDay.TotalSeconds;
            if (duration % unit == 0)
            {
                return input;
            }
            else
            {
                return input.Date.AddSeconds(unit * Math.Ceiling(duration / unit));
            }
        }
    }
}
