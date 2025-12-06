using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Init7.Epg
{
    internal static class Utils
    {
        public static IEnumerable<(T, T)> Pairs<T>(IEnumerable<T> list)
        {
            return list.SelectMany((x, i) => list.Skip(i + 1).Select(y => (x, y)));
        }
    }
}
