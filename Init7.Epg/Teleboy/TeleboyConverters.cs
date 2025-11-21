using Init7.Epg.Schema;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Init7.Epg.Teleboy
{
    public class TeleboyDateTimeOffsetConverter : JsonConverter<DateTimeOffset?>
    {
        public override DateTimeOffset? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var str = reader.GetString();
            if (str == null) return null;
            return DateTimeOffset.Parse(str, CultureInfo.InvariantCulture);
        }

        public override void Write(Utf8JsonWriter writer, DateTimeOffset? value, JsonSerializerOptions options)
        {
            ArgumentNullException.ThrowIfNull(writer);
            if (value.HasValue)
            {
                writer.WriteStringValue(TeleboyConverters.ConvertDateTimeApiParameter(value.Value));
            } else
            {
                writer.WriteNullValue();
            }
        }
    }

    public static class TeleboyConverters
    {
        public static string ConvertDateTimeApiParameter(DateTimeOffset dt)
        {
            return $"{dt:yyyy-MM-ddTHH:mm:sszz}{dt.Offset:mm}";
        }

        public static category[] ConvertGenre(GenreItem genre)
        {
            ArgumentNullException.ThrowIfNull(genre);
            var res = new List<category>();
            if (!string.IsNullOrEmpty(genre.NameDe))
            {
                res.Add(new category
                {
                    lang = "de",
                    Value = genre.NameDe
                });
            }
            if (!string.IsNullOrEmpty(genre.NameFr))
            {
                res.Add(new category
                {
                    lang = "fr",
                    Value = genre.NameFr
                });
            }
            if (!string.IsNullOrEmpty(genre.NameIt))
            {
                res.Add(new category
                {
                    lang = "it",
                    Value = genre.NameIt
                });
            }
            if (!string.IsNullOrEmpty(genre.NameEn))
            {
                res.Add(new category
                {
                    lang = "en",
                    Value = genre.NameEn
                });
            }
            return [.. res];
        }

        public static icon[] ConvertLogos(StationLogos logos)
        {
            ArgumentNullException.ThrowIfNull(logos);
            var res = new List<icon>();
            do
            {
                if (logos.Path == null) break;
                if (logos.Sizes == null) break;

                var tpl = logos.Path
                    .Replace("[size]", "{0}", StringComparison.OrdinalIgnoreCase)
                    .Replace("[type]", "{1}", StringComparison.OrdinalIgnoreCase);

                foreach (var type in logos.Types.Values)
                {
                    foreach (var size in logos.Sizes.Values)
                    {
                        res.Add(new icon
                        {
                            height = size.ToString(CultureInfo.InvariantCulture),
                            width = size.ToString(CultureInfo.InvariantCulture),
                            src = string.Format(CultureInfo.InvariantCulture, tpl, size, type)
                        });
                    }
                }
            } while (false);
            return [.. res];
        }
    }
}
