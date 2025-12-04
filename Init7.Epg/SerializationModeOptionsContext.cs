
using System.Text.Json.Serialization;

[JsonSerializable(typeof(Init7.Epg.ConfigurationSchema))]
// init7
[JsonSerializable(typeof(Init7.Epg.EpgResultList))]
// teleboy
[JsonSerializable(typeof(Init7.Epg.Teleboy.TeleboyEpgResponse))]
[JsonSerializable(typeof(Init7.Epg.Teleboy.TeleboyGenreApiResponse))]
// swisscom
[JsonSerializable(typeof(Init7.Epg.Swisscom.Schema.TvResponse))]
[JsonSerializable(typeof(Init7.Epg.Swisscom.Schema.SeriesResponse))]
[JsonSerializable(typeof(List<Init7.Epg.Swisscom.Schema.Channel>))]
// swisscom nodes
[JsonSerializable(typeof(Init7.Epg.Swisscom.Schema.TvContainer))]
[JsonSerializable(typeof(Init7.Epg.Swisscom.Schema.TvGenre))]
[JsonSerializable(typeof(Init7.Epg.Swisscom.Schema.TvReference))]
[JsonSerializable(typeof(Init7.Epg.Swisscom.Schema.TvChannel))]
[JsonSerializable(typeof(Init7.Epg.Swisscom.Schema.TvBroadcast))]
[JsonSerializable(typeof(Init7.Epg.Swisscom.Schema.Image))]
[JsonSerializable(typeof(Init7.Epg.Swisscom.Schema.UniversalParticipant))]
[JsonSerializable(typeof(Init7.Epg.Swisscom.Schema.SeriesNode))]
[JsonSerializable(typeof(Init7.Epg.Swisscom.Schema.VodEpisodeNode))]
[JsonSerializable(typeof(Init7.Epg.Swisscom.Schema.TvEpisodeNode))]
[JsonSerializable(typeof(Init7.Epg.Swisscom.Schema.UniversalGenre))]
public partial class SerializationModeOptionsContext : JsonSerializerContext
{
}

