using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimeAxis
{
    public abstract class Row
    {
        /// <summary>
        /// 行高，因为涉及到整体缩放，为了保证多次缩放后相对比例不变所以用小数，比int精度更高
        /// </summary>
        public float Height { get; set; } = 25;
    }
}
