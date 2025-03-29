using Init7.Epg.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Init7.Epg.Init7
{
    public class Init7DateTimeOffsetConverter : JsonConverter<DateTimeOffset>
    {
        public override DateTimeOffset Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var str = reader.GetString();
            if (str == null)
            {
                throw new InvalidDataException();
            }
            return DateTimeOffset.Parse(str);
        }

        public override void Write(Utf8JsonWriter writer, DateTimeOffset value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(CommonConverters.ConvertDateTimeXmlTv(value));
        }
    }

    public class Init7NullableDateTimeOffsetConverter : JsonConverter<DateTimeOffset?>
    {
        public override DateTimeOffset? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var str = reader.GetString();
            if (str == null) return null;
            return DateTimeOffset.Parse(str);
        }

        public override void Write(Utf8JsonWriter writer, DateTimeOffset? value, JsonSerializerOptions options)
        {
            if (value.HasValue)
            {
                writer.WriteStringValue(CommonConverters.ConvertDateTimeXmlTv(value.Value));

            } else
            {
                writer.WriteNullValue();
            }
        }
    }

    public class Init7Converters
    {
        public static credits ConvertCredits(ICollection<EpgResultCredit> credits)
        {
            var ecb = new EpgCreditBuilder();
            foreach (var itm in credits)
            {
                switch (itm.Position)
                {
                    case "actor":
                        ecb.AddActor(new actor
                        {
                            Text = [itm.Name]
                        });
                        break;
                    case "producer":
                        ecb.AddProducer(new producer
                        {
                            Text = [itm.Name]
                        });
                        break;
                    case "director":
                        ecb.AddDirector(new director
                        {
                            Text = [itm.Name]
                        });
                        break;
                }
            }
            return ecb.Build();
        }
    }
}
