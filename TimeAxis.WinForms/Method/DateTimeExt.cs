using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimeAxis
{
    public static class DateTimeExt
    {
        public static DateTime Max(params DateTime[] times)
        {
            if (times.Length == 0)
            {
                return DateTime.MinValue;
            }
            DateTime ret = times[0];
            for (int i = 0; i < times.Length; ++i)
            {
                if (times[i] > ret)
                {
                    ret = times[i];
                }
            }
            return ret;
        }

        public static DateTime Min(params DateTime[] times)
        {
            if (times.Length == 0)
            {
                return DateTime.MinValue;
            }
            DateTime ret = times[0];
            for (int i = 0; i < times.Length; ++i)
            {
                if (times[i] < ret)
                {
                    ret = times[i];
                }
            }
            return ret;
        }
    }
}
