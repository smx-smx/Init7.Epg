using Init7.Epg;
using Init7.Epg.Teleboy;
using System.Text.Json.Serialization;

[JsonSerializable(typeof(ConfigurationSchema))]
// init7
[JsonSerializable(typeof(EpgResultList))]
// teleboy
[JsonSerializable(typeof(TeleboyEpgResponse))]
[JsonSerializable(typeof(TeleboyGenreApiResponse))]
// swisscom
[JsonSerializable(typeof(List<Init7.Epg.Swisscom.Channels.Channel>))]
[JsonSerializable(typeof(Init7.Epg.Swisscom.Catalog.TvResponse))]
public partial class SerializationModeOptionsContext : JsonSerializerContext
{
}

