using Init7.Epg;
using Init7.Epg.Teleboy;
using System.Text.Json.Serialization;

[JsonSerializable(typeof(EpgResultList))]
[JsonSerializable(typeof(TeleboyEpgResponse))]
[JsonSerializable(typeof(TeleboyGenreApiResponse))]
public partial class SerializationModeOptionsContext : JsonSerializerContext
{
}

