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
    public enum EpgBoundType
    {
        LowerIn_UpperOut
    }

    public class EpgResultTimeSlot_high
    {
        public DateTime LowerTimeIso { get; set; }
        public DateTime UpperTimeIso { get; set; }
        public EpgBoundType Bounds { get; set; }

        public EpgResultTimeSlot_high(EpgResultTimeSlot_low low)
        {
            LowerTimeIso = DateTime.Parse(low.LowerTimeIso);
            UpperTimeIso = DateTime.Parse(low.UpperTimeIso);
            Bounds = low.Bounds switch
            {
                "[)" => EpgBoundType.LowerIn_UpperOut,
                _ => throw new NotImplementedException(low.Bounds)
            };
        }
    }

    public class EpgResultTimeSlot_low
    {
        [JsonPropertyName("lower")]
        public required string LowerTimeIso { get; set; }
        [JsonPropertyName("upper")]
        public required string UpperTimeIso { get; set; }
        [JsonPropertyName("bounds")]
        public required string Bounds { get; set; }
    }

    public partial class EpgResultChannel_high
    {
        [GeneratedRegex(@"\(hd|schweiz\)|[\. ]")]
        private static partial Regex ChannelNameTrim();
    }

    public partial class EpgResultChannel_high
    {
        public string PrimaryKey { get; set; }
        public string Name { get; set; }
        public bool IsHighDefinition { get; set; }
        public Uri SourceUri { get; set; }
        public string CanonicalName { get; set; }
        public Uri LogoUri { get; set; }
        public bool IsVisible { get; set; }
        public long OrderingIndex { get; set; }
        public long LanguageOrderingIndex { get; set; }
        public string Country { get; set; }
        public string Language { get; set; }
        public bool HasReplay { get; set; }
        public Uri HlsUri { get; set; }
        public bool HasHls { get; set; }
        public DateTime ChangedTimeIso { get; set; }

        public EpgResultChannel_high(EpgResultChannel_low low)
        {
            PrimaryKey = low.PrimaryKey;
            Name = low.Name;
            IsHighDefinition = low.IsHighDefinition;
            SourceUri = new Uri(low.SourceUri);
            CanonicalName = low.CanonicalName;
            LogoUri = new Uri(low.LogoUri);
            IsVisible = low.IsVisible;
            OrderingIndex = low.OrderingIndex;
            LanguageOrderingIndex = low.LanguageOrderingIndex;
            Country = low.Country;
            Language = low.Language;
            HasReplay = low.HasReplay;
            HlsUri = new Uri(low.HlsUri);
            HasHls = low.HasHls;
            ChangedTimeIso = DateTime.Parse(low.ChangedTimeIso);
        }

        public string GetChannelId()
        {
            //return Name;
            return CanonicalName;
            //return "_" + CanonicalName.Split('.').First();
        }

    }

    public class EpgResultChannel_low
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
        public required string LogoUri { get; set; }
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
        public required string ChangedTimeIso { get; set; }
    }

    public class EpgResultCredit
    {
        [JsonPropertyName("position")]
        public required string Position { get; set; }
        [JsonPropertyName("name")]
        public required string Name { get; set; }
    }

    public class EpgResultItem_high
    {
        public string PrimaryKey { get; set; }
        public EpgResultTimeSlot_high TimeSlot { get; set; }
        public EpgResultChannel_high Channel { get; set; }
        public string? Title { get; set; }
        public string? SubTitle { get; set; }
        public string? Description { get; set; }
        public IList<string> Categories { get; set; }
        public string? Country { get; set; }
        public long? Year { get; set; }
        public IList<string> Icons { get; set; }
        public IList<EpgResultCredit> Credits { get; set; }
        public string? RatingSystem { get; set; }
        public string? Rating { get; set; }
        public string? EpisodeNumberingSystem { get; set; }
        public string? EpisodeNumber { get; set; }
        public bool IsPremiere { get; set; }
        public bool Subtitles { get; set; }
        public string? StarRating { get; set; }

        public EpgResultItem_high(EpgResultItem_low low)
        {
            PrimaryKey = low.PrimaryKey;
            TimeSlot = new EpgResultTimeSlot_high(low.TimeSlot);
            Channel = new EpgResultChannel_high(low.Channel);
            Title = low.Title;
            SubTitle = low.SubTitle;
            Description = low.Description;
            Categories = low.Categories;
            Country = low.Country;
            Year = low.Year;
            Icons = low.Icons;
            Credits = low.Credits;
            RatingSystem = low.RatingSystem;
            Rating = low.Rating;
            EpisodeNumberingSystem = low.EpisodeNumberingSystem;
            EpisodeNumber = low.EpisodeNumber;
            IsPremiere = low.IsPremiere;
            Subtitles = low.Subtitles;
            StarRating = low.StarRating;
        }
    }

    public class EpgResultItem_low
    {
        [JsonPropertyName("pk")]
        public required string PrimaryKey { get; set; }
        [JsonPropertyName("timeslot")]
        public required EpgResultTimeSlot_low TimeSlot { get; set; }
        [JsonPropertyName("channel")]
        public required EpgResultChannel_low Channel { get; set; }
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

    public class EpgResultList_high
    {
        public long Count { get; set; }
        public Uri? NextUri { get; set; }
        public Uri? PreviousUri { get; set; }
        public IList<EpgResultItem_high> Items { get; set; }

        public EpgResultList_high(EpgResultList_low low)
        {
            Count = low.Count;
            NextUri = low.NextUri == null ? null : new Uri(low.NextUri);
            PreviousUri = low.PreviousUri == null ? null : new Uri(low.PreviousUri);
            Items = low.Items.Select(itm => new EpgResultItem_high(itm)).ToList();
        }
    }

    public class EpgResultList_low
    {
        [JsonPropertyName("count")]
        public required long Count { get; set; }
        [JsonPropertyName("next")]
        public required string? NextUri { get; set; }
        [JsonPropertyName("previous")]
        public required string? PreviousUri { get; set; }
        [JsonPropertyName("results")]
        public required IList<EpgResultItem_low> Items { get; set; }
    }
}