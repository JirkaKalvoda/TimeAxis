using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimeAxis
{
    /// <summary>
    /// 滚动条触发事件
    /// </summary>
    public class ScrollValueChangedEventArgs : EventArgs
    {
        //public double Start { get; set; }
        
        //public double Stop { get; set; }
        
        public double DisplayStart { get; set; }
        
        public double DisplayStop { get; set; }
    }
}
