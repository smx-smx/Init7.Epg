using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Init7.Epg
{
    class CommonConverters
    {
        public static TOut[]? ConvertSingleNullable<TIn, TOut>(TIn? value, Func<TIn, TOut> converter)
        {
            if (value == null) return null;
            var res = converter(value);
            return [res];
        }

        public static TOut[]? ConvertSingleNullable<TIn, TOut>(bool cond, TIn? value, Func<TIn, TOut> converter)
        {
            if (!cond || value == null) return null;
            var res = converter(value);
            return [res];
        }

        public static TOut? ConvertNullable<TIn, TOut>(
            TIn? value,
            Func<TIn, TOut> converter) where TOut : class
        {
            if (value == null) return null;
            return converter(value);
        }

        public static TOut? ConvertNullable<TIn, TOut>(
            bool cond, TIn? value,
            Func<TIn, TOut> converter) where TOut : class
        {
            if (!cond || value == null) return null;
            return converter(value);
        }

        public static string ConvertDateTimeXmlTv(DateTimeOffset dt)
        {
            return $"{dt:yyyyMMddHHmmss zz}{dt.Offset:mm}";
        }
    }
}
