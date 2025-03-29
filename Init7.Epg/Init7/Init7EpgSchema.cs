using Init7.Epg.Init7;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Diagnostics.CodeAnalysis;
using System.Dynamic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Init7.Epg
{
    public class EpgResultTimeSlot
    {
        [JsonPropertyName("lower")]
        [JsonConverter(typeof(Init7DateTimeOffsetConverter))]
        public required DateTimeOffset LowerTimeIso { get; set; }
        [JsonPropertyName("upper")]
        [JsonConverter(typeof(Init7DateTimeOffsetConverter))]
        public required DateTimeOffset UpperTimeIso { get; set; }
        [JsonPropertyName("bounds")]
        public required string Bounds { get; set; }
    }

    public class EpgResultChannel
    {
        [JsonPropertyName("pk")]
        public required string PrimaryKey { get; set; }
        [JsonPropertyName("name")]
        public required string Name { get; set; }
        [JsonPropertyName("hd")]
        public required bool IsHighDefinition { get; set; }
        [JsonPropertyName("src")]
        public required string SourceUri { get; set; }
        [JsonPropertyName("canonical_name")]
        public required string CanonicalName { get; set; }
        [JsonPropertyName("logo")]
        public required Uri LogoUri { get; set; }
        [JsonPropertyName("visible")]
        public required bool IsVisible { get; set; }
        [JsonPropertyName("ordernum")]
        public required long OrderingIndex { get; set; }
        [JsonPropertyName("langordernum")]
        public required long LanguageOrderingIndex { get; set; }
        [JsonPropertyName("country")]
        public required string Country { get; set; }
        [JsonPropertyName("language")]
        public required string Language { get; set; }
        [JsonPropertyName("has_replay")]
        public required bool HasReplay { get; set; }
        [JsonPropertyName("hls_src")]
        public required string HlsUri { get; set; }
        [JsonPropertyName("has_hls")]
        public required bool HasHls { get; set; }
        [JsonPropertyName("changed")]
        [JsonConverter(typeof(Init7DateTimeOffsetConverter))]
        public required DateTimeOffset ChangedTimeIso { get; set; }


        public string GetChannelId()
        {
            return CanonicalName;
        }
    }

    public class EpgResultCredit
    {
        [JsonPropertyName("position")]
        public required string Position { get; set; }
        [JsonPropertyName("name")]
        public required string Name { get; set; }
    }

    public class EpgResultItem
    {
        [JsonPropertyName("pk")]
        public required string PrimaryKey { get; set; }
        [JsonPropertyName("timeslot")]
        public required EpgResultTimeSlot TimeSlot { get; set; }
        [JsonPropertyName("channel")]
        public required EpgResultChannel Channel { get; set; }
        [JsonPropertyName("title")]
        public required string? Title { get; set; }
        [JsonPropertyName("sub_title")]
        public required string? SubTitle { get; set; }
        [JsonPropertyName("desc")]
        public required string? Description { get; set; }
        [JsonPropertyName("categories")]
        public required IList<string> Categories { get; set; }
        [JsonPropertyName("country")]
        public required string? Country { get; set; }
        [JsonPropertyName("date")]
        public required long? Year { get; set; }
        [JsonPropertyName("icons")]
        public required IList<string> Icons { get; set; }
        [JsonPropertyName("credits")]
        public required IList<EpgResultCredit> Credits { get; set; }
        [JsonPropertyName("rating_system")]
        public required string? RatingSystem { get; set; }
        [JsonPropertyName("rating")]
        public required string? Rating { get; set; }
        [JsonPropertyName("episode_num_system")]
        public required string? EpisodeNumberingSystem { get; set; }
        [JsonPropertyName("episode_num")]
        public required string? EpisodeNumber { get; set; }
        [JsonPropertyName("premiere")]
        public required bool IsPremiere { get; set; }
        [JsonPropertyName("subtitles")]
        public required bool Subtitles { get; set; }
        [JsonPropertyName("star_rating")]
        public required string? StarRating { get; set; }
    }

    public class EpgResultList
    {
        [JsonPropertyName("count")]
        public required long Count { get; set; }
        [JsonPropertyName("next")]
        public required Uri? NextUri { get; set; }
        [JsonPropertyName("previous")]
        public required Uri? PreviousUri { get; set; }
        [JsonPropertyName("results")]
        public required IList<EpgResultItem> Items { get; set; }
    }
}