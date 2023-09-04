using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimeAxis
{
    /// <summary>
    /// 鼠标状态枚举
    /// </summary>
    public enum MouseState
    {
        None = 0,

        SplitLine = 1,

        MarkLine = 2,

        RowLine = 3,

        ClickData = 4,

        BoxLeft = 5,

        BoxRight = 6,

        Box = 7,

        EyeHide = 8,

        ScrollArrowMin = 128,

        ScrollArrowMax = 129,

        ScrollRingMin = 130,

        ScrollRingMax = 131,

        ScrollBar = 132,

    }
}
