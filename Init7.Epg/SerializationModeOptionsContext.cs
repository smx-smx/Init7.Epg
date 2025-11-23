using Init7.Epg;
using Init7.Epg.Teleboy;
using System.Text.Json.Serialization;

[JsonSerializable(typeof(EpgResultList))]
[JsonSerializable(typeof(TeleboyEpgResponse))]
[JsonSerializable(typeof(TeleboyGenreApiResponse))]
[JsonSerializable(typeof(ConfigurationSchema))]
public partial class SerializationModeOptionsContext : JsonSerializerContext
{
}

