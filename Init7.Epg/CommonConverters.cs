namespace Init7.Epg
{
    public static class CommonConverters
    {
        public static TOut[]? ConvertSingleNullable<TIn, TOut>(TIn? value, Func<TIn, TOut> converter)
        {
            ArgumentNullException.ThrowIfNull(converter);

            if (value is null) return null;
            var res = converter(value);
            return [res];
        }

        public static string ConvertDateTimeXmlTv(DateTimeOffset dt)
        {
            return $"{dt:yyyyMMddHHmmss zz}{dt.Offset:mm}";
        }
    }
}
