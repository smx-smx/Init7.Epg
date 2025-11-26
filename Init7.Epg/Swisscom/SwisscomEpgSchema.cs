using Init7.Epg.Swisscom.Channels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Init7.Epg.Swisscom.Channels
{
    public class Channel
    {
        [JsonPropertyName("Identifier")]
        public required string Identifier { get; set; }

        [JsonPropertyName("Title")]
        public required string Title { get; set; }

        [JsonPropertyName("Position")]
        public int Position { get; set; }

        [JsonPropertyName("Positions")]
        public required List<ChannelPosition> Positions { get; set; }

        [JsonPropertyName("ChargeType")]
        public required string ChargeType { get; set; }

        [JsonPropertyName("State")]
        public required string State { get; set; }

        [JsonPropertyName("Visibility")]
        public required string Visibility { get; set; }

        [JsonPropertyName("HasMarkers")]
        public bool HasMarkers { get; set; }

        [JsonPropertyName("HasAlternativeAdvertising")]
        public bool HasAlternativeAdvertising { get; set; }

        [JsonPropertyName("Description")]
        public required string Description { get; set; }

        [JsonPropertyName("Groups")]
        public required List<string> Groups { get; set; }

        [JsonPropertyName("Bouquets")]
        public required List<string> Bouquets { get; set; }

        [JsonPropertyName("Languages")]
        public required List<string> Languages { get; set; }

        [JsonPropertyName("Services")]
        public required ChannelServices Services { get; set; }

        [JsonPropertyName("ChannelShapes")]
        public required List<ChannelShape> ChannelShapes { get; set; }

        [JsonPropertyName("Images")]
        public required List<ChannelImage> Images { get; set; }
    }

    public class ChannelPosition
    {
        [JsonPropertyName("Type")]
        public required string Type { get; set; }

        [JsonPropertyName("Position")]
        public int Position { get; set; }

        [JsonPropertyName("Start")]
        public DateTimeOffset? Start { get; set; }

        [JsonPropertyName("End")]
        public DateTimeOffset? End { get; set; }
    }

    public class ChannelServices
    {
        // These individual services are nullable because App Channels (e.g. Netflix) 
        // do not have IPTV.LiveTV, and Standard Channels do not have AppChannel.

        [JsonPropertyName("IPTV.LiveTV")]
        public ServiceDetail? IptvLiveTv { get; set; }

        [JsonPropertyName("IPTV.LiveTV_PIP")]
        public ServiceDetail? IptvLiveTvPip { get; set; }

        [JsonPropertyName("IPTV.NPVR")]
        public ServiceDetail? IptvNpvr { get; set; }

        [JsonPropertyName("IPTV.Timeshift")]
        public ServiceDetail? IptvTimeshift { get; set; }

        [JsonPropertyName("IPTV.ReplayTV")]
        public ServiceDetail? IptvReplayTv { get; set; }

        [JsonPropertyName("OTT.NPVR")]
        public ServiceDetail? OttNpvr { get; set; }

        [JsonPropertyName("OTT.Timeshift")]
        public ServiceDetail? OttTimeshift { get; set; }

        [JsonPropertyName("OTT.ReplayTV")]
        public ServiceDetail? OttReplayTv { get; set; }

        [JsonPropertyName("OfflineRecording")]
        public ServiceDetail? OfflineRecording { get; set; }

        [JsonPropertyName("HbbTV")]
        public ServiceDetail? HbbTv { get; set; }

        [JsonPropertyName("AdTrigger")]
        public ServiceDetail? AdTrigger { get; set; }

        [JsonPropertyName("AdFree")]
        public ServiceDetail? AdFree { get; set; }

        [JsonPropertyName("IPTV.AppChannel")]
        public ServiceDetail? IptvAppChannel { get; set; }
    }

    public class ServiceDetail
    {
        [JsonPropertyName("State")]
        public required string State { get; set; }

        [JsonPropertyName("Start")]
        public DateTimeOffset? Start { get; set; }

        [JsonPropertyName("End")]
        public DateTimeOffset? End { get; set; }

        [JsonPropertyName("Duration")]
        public string? Duration { get; set; }

        [JsonPropertyName("Address")]
        public string? Address { get; set; }
    }

    public class ChannelShape
    {
        [JsonPropertyName("Identifier")]
        public required string Identifier { get; set; }

        [JsonPropertyName("Shape")]
        public required string Shape { get; set; }

        [JsonPropertyName("HDRType")]
        public required string HdrType { get; set; }

        [JsonPropertyName("IsEncrypted")]
        public bool? IsEncrypted { get; set; }

        [JsonPropertyName("IsOTTEncrypted")]
        public bool? IsOttEncrypted { get; set; }

        [JsonPropertyName("Services")]
        public required ChannelServices Services { get; set; }

        // Properties is nullable because "APPChannel" shapes in the JSON do not contain it.
        [JsonPropertyName("Properties")]
        public ShapeProperties? Properties { get; set; }
    }

    public class ShapeProperties
    {
        [JsonPropertyName("LinkedOttStreamChannel")]
        public string? LinkedOttStreamChannel { get; set; }

        [JsonPropertyName("LinkedSportChannel")]
        public string? LinkedSportChannel { get; set; }
    }

    public class SportItem
    {
        [JsonPropertyName("Identifier")]
        public required string Identifier { get; set; }
        [JsonPropertyName("Title")]
        public required string Title { get; set; }
    }

    public class ChannelImage
    {
        [JsonPropertyName("Role")]
        public required string Role { get; set; }

        [JsonPropertyName("ContentPath")]
        public required string ContentPath { get; set; }
    }
}

namespace Init7.Epg.Swisscom.Catalog
{
    public class TvResponse
    {
        [JsonPropertyName("Status")]
        public required ResponseStatus Status { get; set; }

        [JsonPropertyName("Request")]
        public required RequestDetails Request { get; set; }

        [JsonPropertyName("DataSource")]
        public DataSourceInfo? DataSource { get; set; }

        [JsonPropertyName("Nodes")]
        public required NodeCollection Nodes { get; set; }
    }

    public class ResponseStatus
    {
        [JsonPropertyName("Version")]
        public string? Version { get; set; }

        [JsonPropertyName("Status")]
        public string? Status { get; set; }

        [JsonPropertyName("ProcessingTime")]
        public string? ProcessingTime { get; set; }

        [JsonPropertyName("ExecutionTime")]
        public DateTimeOffset? ExecutionTime { get; set; }
    }

    public class RequestDetails
    {
        [JsonPropertyName("Domain")]
        public string? Domain { get; set; }

        [JsonPropertyName("Resource")]
        public string? Resource { get; set; }

        [JsonPropertyName("Action")]
        public string? Action { get; set; }

        [JsonPropertyName("Parameters")]
        public string? Parameters { get; set; }

        [JsonPropertyName("Identifiers")]
        public List<string>? Identifiers { get; set; }

        [JsonPropertyName("Start")]
        public DateTimeOffset? Start { get; set; }

        [JsonPropertyName("End")]
        public DateTimeOffset? End { get; set; }

        [JsonPropertyName("DataLevel")]
        public string? DataLevel { get; set; }
    }

    public class DataSourceInfo
    {
        [JsonPropertyName("Snapshot")]
        public string? Snapshot { get; set; }

        [JsonPropertyName("DbCreationTime")]
        public DateTimeOffset? DbCreationTime { get; set; }

        [JsonPropertyName("IncrementCreationTime")]
        public DateTimeOffset? IncrementCreationTime { get; set; }
    }

    public class NodeCollection
    {
        [JsonPropertyName("Count")]
        public int Count { get; set; }

        [JsonPropertyName("TotalItemCount")]
        public int TotalItemCount { get; set; }

        [JsonPropertyName("Items")]
        public required List<TvNode> Items { get; set; }
    }

    /// <summary>
    /// Represents a polymorphic item in the graph (Channel, Broadcast, Image, Series, Participant).
    /// </summary>
    public class TvNode
    {
        [JsonPropertyName("Domain")]
        public string? Domain { get; set; }

        [JsonPropertyName("Identifier")]
        public required string Identifier { get; set; }

        [JsonPropertyName("Kind")]
        public required string Kind { get; set; }

        // Broadcast specific
        [JsonPropertyName("RootIdentifier")]
        public string? RootIdentifier { get; set; }

        [JsonPropertyName("Channel")]
        public string? Channel { get; set; }

        // Image specific
        [JsonPropertyName("Role")]
        public string? Role { get; set; }

        [JsonPropertyName("Shape")]
        public string? Shape { get; set; }

        [JsonPropertyName("ContentPath")]
        public string? ContentPath { get; set; }

        [JsonPropertyName("HasTitle")]
        public bool? HasTitle { get; set; }

        // Participant/Relation specific
        [JsonPropertyName("Sex")]
        public string? Sex { get; set; }

        // Complex Objects
        [JsonPropertyName("Content")]
        public ContentInfo? Content { get; set; }

        [JsonPropertyName("Nodes")]
        public NodeCollection? Nodes { get; set; }

        [JsonPropertyName("Series")]
        public SeriesInfo? Series { get; set; }

        [JsonPropertyName("TechnicalAttributes")]
        public TechnicalAttributes? TechnicalAttributes { get; set; }

        [JsonPropertyName("Version")]
        public VersionInfo? Version { get; set; }

        [JsonPropertyName("Availabilities")]
        public List<Availability>? Availabilities { get; set; }

        [JsonPropertyName("Relations")]
        public List<Relation>? Relations { get; set; }

        [JsonPropertyName("ExternalGenres")]
        public List<string>? ExternalGenres { get; set; }

        [JsonPropertyName("Descriptors")]
        public List<string>? Descriptors { get; set; }

        [JsonPropertyName("LegalProductTypeExcludes")]
        public List<string>? LegalProductTypeExcludes { get; set; }

        [JsonPropertyName("OnDemand")]
        public OnDemandInfo? OnDemand { get; set; }

        [JsonPropertyName("OnDemandReferences")]
        public List<OnDemandReference>? OnDemandReferences { get; set; }
    }

    public class ContentInfo
    {
        [JsonPropertyName("Description")]
        public DescriptionInfo? Description { get; set; }

        [JsonPropertyName("Nodes")]
        public NodeCollection? Nodes { get; set; }
    }

    public class DescriptionInfo
    {
        [JsonPropertyName("Title")]
        public string? Title { get; set; }

        [JsonPropertyName("Language")]
        public string? Language { get; set; }

        [JsonPropertyName("Summary")]
        public string? Summary { get; set; }

        [JsonPropertyName("ShortSummary")]
        public string? ShortSummary { get; set; }

        [JsonPropertyName("Country")]
        public string? Country { get; set; }

        [JsonPropertyName("ReleaseDate")]
        public DateTimeOffset? ReleaseDate { get; set; }

        [JsonPropertyName("AgeRestrictionRating")]
        public string? AgeRestrictionRating { get; set; }

        [JsonPropertyName("AgeRestrictionSystem")]
        public string? AgeRestrictionSystem { get; set; }

        [JsonPropertyName("Duration")]
        public TimeSpan? Duration { get; set; }

        [JsonPropertyName("Subtitle")]
        public string? Subtitle { get; set; }

        // Participant specific fields
        [JsonPropertyName("FirstName")]
        public string? FirstName { get; set; }

        [JsonPropertyName("LastName")]
        public string? LastName { get; set; }

        [JsonPropertyName("Fullname")]
        public string? Fullname { get; set; }
    }

    public class SeriesInfo
    {
        [JsonPropertyName("Identifier")]
        public string? Identifier { get; set; }

        [JsonPropertyName("GlobalSeriesIdentifier")]
        public string? GlobalSeriesIdentifier { get; set; }

        [JsonPropertyName("Season")]
        public int? Season { get; set; }

        [JsonPropertyName("Episode")]
        public int? Episode { get; set; }

        [JsonPropertyName("Part")]
        public int? Part { get; set; }

        [JsonPropertyName("PartsCount")]
        public int? PartsCount { get; set; }
    }

    public class TechnicalAttributes
    {
        [JsonPropertyName("Stereo")]
        public bool? Stereo { get; set; }

        [JsonPropertyName("Subtitles")]
        public bool? Subtitles { get; set; }

        [JsonPropertyName("NarratedAudio")]
        public bool? NarratedAudio { get; set; }
        [JsonPropertyName("DolbySurround")]
        public bool? DolbySurround { get; set; }
    }

    public class VersionInfo
    {
        [JsonPropertyName("Date")]
        public DateTimeOffset? Date { get; set; }

        [JsonPropertyName("Hash")]
        public string? Hash { get; set; }
    }

    public class Availability
    {
        [JsonPropertyName("AvailabilityStart")]
        public DateTimeOffset? AvailabilityStart { get; set; }

        [JsonPropertyName("AvailabilityEnd")]
        public DateTimeOffset? AvailabilityEnd { get; set; }
    }

    public class Relation
    {
        [JsonPropertyName("Domain")]
        public string? Domain { get; set; }

        [JsonPropertyName("Kind")]
        public string? Kind { get; set; }

        [JsonPropertyName("Role")]
        public string? Role { get; set; }

        [JsonPropertyName("TargetIdentifier")]
        public string? TargetIdentifier { get; set; }

        [JsonPropertyName("TargetNode")]
        public TvNode? TargetNode { get; set; }

        // Specifically for Actor relations
        [JsonPropertyName("Title")]
        public string? Title { get; set; }
    }

    public class OnDemandInfo
    {
        [JsonPropertyName("Identifier")]
        public string? Identifier { get; set; }

        [JsonPropertyName("AvailabilitiesSummary")]
        public Dictionary<string, List<string>>? AvailabilitiesSummary { get; set; }
    }

    public class OnDemandReference
    {
        [JsonPropertyName("Identifier")]
        public string? Identifier { get; set; }

        [JsonPropertyName("Languages")]
        public List<string>? Languages { get; set; }

        [JsonPropertyName("AvailabilitiesSummary")]
        public Dictionary<string, List<string>>? AvailabilitiesSummary { get; set; }
    }
}