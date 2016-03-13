using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IoTActivityMonitor
{

    public static class ScaleExtension
    {
        public static IEnumerable<int> Scale(this IEnumerable<int> source, int max)
        {
            int sourceMax = source.Max();

            if(sourceMax > max-1)
            {
                float scale = (float)max / (float)sourceMax;
                return source.Select(i => (int)Math.Ceiling(i * scale));
            }
            else
            {
                return source;
            }
        }
    }

}