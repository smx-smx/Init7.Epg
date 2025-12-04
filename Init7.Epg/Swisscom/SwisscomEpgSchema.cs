using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Init7.Epg.Swisscom.Schema;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public class NodeDomainAttribute(string domain) : Attribute
{
    public string Domain { get; private set; } = domain;
}

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public class NodeKindAttribute(string kind) : Attribute
{
    public string Kind { get; private set; } = kind;
}

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
public class NodeRoleAttribute(string role) : Attribute
{
    public string Role { get; private set; } = role;
}

public class Channel
{
    [JsonPropertyName("Identifier")]
    public required string Identifier { get; set; }

    [JsonPropertyName("Title")]
    public string? Title { get; set; }

    [JsonPropertyName("Position")]
    public int Position { get; set; }

    [JsonPropertyName("Positions")]
    public List<ChannelPosition>? Positions { get; set; }

    [JsonPropertyName("ChargeType")]
    public string? ChargeType { get; set; }

    [JsonPropertyName("State")]
    public string? State { get; set; }

    [JsonPropertyName("Visibility")]
    public string? Visibility { get; set; }

    [JsonPropertyName("HasMarkers")]
    public bool HasMarkers { get; set; }

    [JsonPropertyName("HasAlternativeAdvertising")]
    public bool HasAlternativeAdvertising { get; set; }

    [JsonPropertyName("Description")]
    public string? Description { get; set; }

    [JsonPropertyName("Groups")]
    public List<string>? Groups { get; set; }

    [JsonPropertyName("Bouquets")]
    public List<string>? Bouquets { get; set; }

    [JsonPropertyName("Languages")]
    public List<string>? Languages { get; set; }

    [JsonPropertyName("Services")]
    public ChannelServices? Services { get; set; }

    [JsonPropertyName("ChannelShapes")]
    public List<ChannelShape>? ChannelShapes { get; set; }

    [JsonPropertyName("Images")]
    public List<ChannelImage>? Images { get; set; }
}

public class ChannelPosition
{
    [JsonPropertyName("Type")]
    public string? Type { get; set; }

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
    public string? State { get; set; }

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
    public string? Identifier { get; set; }

    [JsonPropertyName("Shape")]
    public string? Shape { get; set; }

    [JsonPropertyName("HDRType")]
    public string? HdrType { get; set; }

    [JsonPropertyName("IsEncrypted")]
    public bool? IsEncrypted { get; set; }

    [JsonPropertyName("IsOTTEncrypted")]
    public bool? IsOttEncrypted { get; set; }

    [JsonPropertyName("Services")]
    public ChannelServices? Services { get; set; }

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
    public string? Identifier { get; set; }
    [JsonPropertyName("Title")]
    public string? Title { get; set; }
}


public class CatalogResponse
{
    [JsonPropertyName("Status")]
    public required ResponseStatus Status { get; set; }
    [JsonPropertyName("DataSource")]
    public DataSourceInfo? DataSource { get; set; }
    [JsonPropertyName("Results")]
    public required List<SeriesNode> Results { get; set; }
}

public class ResponseHeader
{
    [JsonPropertyName("Status")]
    public required ResponseStatus Status { get; set; }

    [JsonPropertyName("DataSource")]
    public DataSourceInfo? DataSource { get; set; }
}

public class TvResponse : ResponseHeader
{
    [JsonPropertyName("Request")]
    public required RequestDetails Request { get; set; }
    [JsonPropertyName("Nodes")]
    public required NodeCollection Nodes { get; set; }
}

public class SeriesResponse : ResponseHeader
{
    [JsonPropertyName("Results")]
    public List<SeriesNode>? Results { get; set; }
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

public class Helpers
{
    public static IEnumerable<NodeKey> GetNodeKeys<T>() where T : Node
    {
        return GetNodeKeys(typeof(T));
    }

    public static IEnumerable<NodeKey> GetNodeKeys(Type t)
    {
        var domain = t.GetCustomAttribute<NodeDomainAttribute>()?.Domain;
        var kind = t.GetCustomAttribute<NodeKindAttribute>()?.Kind;
        if (string.IsNullOrEmpty(kind))
        {
            throw new InvalidOperationException($"Missing Kind attribute for type {t.FullName}");
        }

        yield return new NodeKey
        {
            Kind = kind,
            Domain = domain
        };

        foreach (var role in t.GetCustomAttributes<NodeRoleAttribute>())
        {
            yield return new NodeKey
            {
                Kind = kind,
                Domain = domain,
                Role = role.Role
            };
        }

    }
}

public class NodeCollection
{
    [JsonPropertyName("Count")]
    public int Count { get; set; }

    [JsonPropertyName("TotalItemCount")]
    public int TotalItemCount { get; set; }

    [JsonPropertyName("Items")]
    public List<Node>? Items { get; set; }

    public IEnumerable<T> GetNodes<T>() where T : Node
    {
        var key = Helpers.GetNodeKeys<T>().First();

        return from itm in Items
               where (itm.Domain == key.Domain && itm.Kind == key.Kind)
               select itm as T;
    }
}

public class Node
{
    [JsonPropertyName("Domain")]
    public string? Domain { get; set; }

    [JsonPropertyName("Identifier")]
    public string? Identifier { get; set; }

    [JsonPropertyName("Kind")]
    public string? Kind { get; set; }
    [JsonPropertyName("Role")]
    public string? Role { get; set; }
}

[NodeDomain("Universal")]
[NodeKind("Genre")]
public class UniversalGenre : Node
{
    [JsonPropertyName("TargetIdentifier")]
    public string? TargetIdentifier { get; set; }
}

[NodeDomain("Universal")]
[NodeKind("Series")]
public class SeriesNode : Node
{
    // variant 2 in standalone API
    [JsonPropertyName("Title")]
    public string? Title { get; set; }
    [JsonPropertyName("Country")]
    public string? Country { get; set; }
    [JsonPropertyName("Keywords")]
    public List<string>? Keywords { get; set; }
    [JsonPropertyName("Languages")]
    public List<string>? Languages { get; set; }
    [JsonPropertyName("Summary")]
    public string? Summary { get; set; }
    [JsonPropertyName("ExternalGenres")]
    public List<string>? ExternalGenres { get; set; }
    [JsonPropertyName("Genres")]
    public List<string>? Genres { get; set; }
    [JsonPropertyName("Images")]
    public List<Image>? Images { get; set; }
    [JsonPropertyName("Availabilities")]
    public List<Availability>? Availabilities { get; set; }
    [JsonPropertyName("Children")]
    public List<SeasonNode>? Children { get; set; }
    [JsonPropertyName("AvailabilitiesSummary")]
    public Dictionary<string, List<string>>? AvailabilitiesSummary { get; set; }
    [JsonPropertyName("AvailabilitiesProperties")]
    public AvailabilitiesProperties? AvailabilitiesProperties { get; set; }
    [JsonPropertyName("OnDemandReferences")]
    public List<OnDemandReference>? OnDemandReferences { get; set; }
    [JsonPropertyName("AgeRestriction")]
    public int? AgeRestriction { get; set; }
    [JsonPropertyName("Rating")]
    public int? Rating { get; set; }
    [JsonPropertyName("Descriptors")]
    public List<string>? Descriptors { get; set; }
    [JsonPropertyName("IMDB")]
    public string? IMDB { get; set; }

    // variant 1 in TV Guide

    [JsonPropertyName("Content")]
    public ContentInfo? Content { get; set; }
    [JsonPropertyName("Relations")]
    public List<Node>? Relations { get; set; }
}

public class SeasonNode : Node
{
    [JsonPropertyName("Series")]
    public SeriesInfo? Series { get; set; }
    [JsonPropertyName("Children")]
    public List<Node>? Children { get; set; }
}

[NodeDomain("TV")]
[NodeKind("Genre")]
public class TvGenre : Node
{
    [JsonPropertyName("Content")]
    public ContentInfo? Content { get; set; }
    [JsonPropertyName("Relations")]
    public List<Node>? Relations { get; set; }
}

[NodeDomain("VOD")]
[NodeKind("Episode")]
public class VodEpisodeNode : Node
{
    [JsonPropertyName("RootIdentifier")]
    public string? RootIdentifier { get; set; }
    [JsonPropertyName("Title")]
    public string? Title { get; set; }
    [JsonPropertyName("Languages")]
    public List<string>? Languages { get; set; }
    [JsonPropertyName("Series")]
    public SeriesInfo? Series { get; set; }
    [JsonPropertyName("Images")]
    public List<Image>? Images { get; set; }
    [JsonPropertyName("AvailabilitiesSummary")]
    public Dictionary<string, List<string>>? AvailabilitiesSummary { get; set; }
    [JsonPropertyName("AvailabilitiesProperties")]
    public AvailabilitiesProperties? AvailabilitiesProperties { get; set; }
}

[NodeDomain("TV")]
[NodeKind("Episode")]
public class TvEpisodeNode : Node
{
    [JsonPropertyName("Channel")]
    public int? Channel { get; set; }
    [JsonPropertyName("RootIdentifier")]
    public string? RootIdentifier { get; set; }
    [JsonPropertyName("ProgramIdentifier")]
    public string? ProgramIdentifier { get; set; }
    [JsonPropertyName("Title")]
    public string? Title { get; set; }
    [JsonPropertyName("Start")]
    public DateTimeOffset? Start { get; set; }
    [JsonPropertyName("End")]
    public DateTimeOffset? End { get; set; }
    [JsonPropertyName("Languages")]
    public List<string>? Languages { get; set; }
    [JsonPropertyName("Series")]
    public SeriesInfo? Series { get; set; }
    [JsonPropertyName("Images")]
    public List<Image>? Images { get; set; }
}

public class VodNode : Node
{
    [JsonPropertyName("RootIdentifier")]
    public string? RootIdentifier { get; set; }
    [JsonPropertyName("Title")]
    public string? Title { get; set; }
    [JsonPropertyName("Languages")]
    public List<string>? Languages { get; set; }
    [JsonPropertyName("Series")]
    public SeriesInfo? Series { get; set; }
    [JsonPropertyName("Images")]
    public List<Image>? Images { get; set; }
    [JsonPropertyName("AvailabilitiesSummary")]
    public Dictionary<string, List<string>>? AvailabilitiesSummary { get; set; }
}

[NodeDomain("TV")]
[NodeKind("Container")]
[NodeRole("Store")]
public class TvContainer : Node
{
    [JsonPropertyName("Content")]
    public ContentInfo? Content { get; set; }
    [JsonPropertyName("Relations")]
    public List<NodeRelation>? Relations { get; set; }
}

[NodeDomain("TV")]
[NodeKind("Channel")]
public class TvChannel : Node
{
    [JsonPropertyName("Content")]
    public ContentInfo? Content { get; set; }
}

public class SportType : Node
{
    [JsonPropertyName("Title")]
    public string? Title { get; set; }
}

public class SportTexts
{

}

public class SportCompetition : Node
{
    [JsonPropertyName("ShapeAgnosticIdentifier")]
    public string? ShapeAgnosticIdentifier { get; set; }
    [JsonPropertyName("Sport")]
    public SportType? Sport { get; set; }
    [JsonPropertyName("Title")]
    public string? Title { get; set; }
    // $FIXME: Unknown
    [JsonPropertyName("Texts")]
    public SportTexts? Texts { get; set; }
}

public class SportParticipant
{
    [JsonPropertyName("Identifier")]
    public string? Identifier { get; set; }
    [JsonPropertyName("GracenoteIdentifier")]
    public string? GracenoteIdentifier { get; set; }
    [JsonPropertyName("Title")]
    public string? Title { get; set; }
    [JsonPropertyName("ShortTitle")]
    public string? ShortTitle { get; set; }
}

public class SportEvent
{
    [JsonPropertyName("Identifier")]
    public string? Identifier { get; set; }
    [JsonPropertyName("Teams")]
    public List<int>? Teams { get; set; }
    [JsonPropertyName("Venue")]
    public int? Venue { get; set; }
    [JsonPropertyName("Date")]
    public DateTimeOffset? Date { get; set; }
    [JsonPropertyName("ScheduledStartTime")]
    public DateTimeOffset? ScheduledStartTime { get; set; }
    [JsonPropertyName("RemoteId")]
    public string? RemoteId { get; set; }
    [JsonPropertyName("HasLiveStatistics")]
    public bool? HasLiveStatistics { get; set; }
    [JsonPropertyName("GracenoteIdentifier")]
    public string? GracenoteIdentifier { get; set; }
    [JsonPropertyName("RoundName")]
    public string? RoundName { get; set; }
    [JsonPropertyName("TeamVenueDate")]
    public string? TeamVenueDate { get; set; }
    [JsonPropertyName("RootIdentifier")]
    public string? RootIdentifier { get; set; }
    [JsonPropertyName("Title")]
    public string? Title { get; set; }
}

public class UniversalSports
{
    [JsonPropertyName("SportType")]
    public SportType? SportType { get; set; }
    [JsonPropertyName("Competition")]
    public SportCompetition? Competition { get; set; }
    [JsonPropertyName("Event")]
    public SportEvent? Event { get; set; }
    [JsonPropertyName("Participants")]
    public List<SportParticipant>? Participants { get; set; }
}



[NodeDomain("TV")]
[NodeKind("Broadcast")]
public class TvBroadcast : Node
{
    [JsonPropertyName("Channel")]
    public string? Channel { get; set; }
    [JsonPropertyName("Content")]
    public ContentInfo? Content { get; set; }

    [JsonPropertyName("Availabilities")]
    public List<Availability>? Availabilities { get; set; }
    [JsonPropertyName("Nodes")]
    public NodeCollection? Nodes { get; set; }
    [JsonPropertyName("Relations")]
    public List<NodeRelation>? Relations { get; set; }
    [JsonPropertyName("Series")]
    public SeriesInfo? Series { get; set; }
    [JsonPropertyName("TechnicalAttributes")]
    public TechnicalAttributes? TechnicalAttributes { get; set; }
    [JsonPropertyName("RootIdentifier")]
    public string? RootIdentifier { get; set; }
    [JsonPropertyName("Version")]
    public VersionInfo? Version { get; set; }
    [JsonPropertyName("LiveTransmission")]
    public bool? LiveTransmission { get; set; }
    [JsonPropertyName("LegalProductTypeExcludes")]
    public List<string>? LegalProductTypeExcludes { get; set; }
    [JsonPropertyName("IMDB")]
    public string? IMDB { get; set; }
    [JsonPropertyName("OnDemand")]
    public OnDemandInfo? OnDemand { get; set; }
    [JsonPropertyName("OnDemandReferences")]
    public List<OnDemandReference>? OnDemandReferences { get; set; }
    [JsonPropertyName("UniversalSports")]
    public UniversalSports? UniversalSports { get; set; }

    public IEnumerable<NodeRelation<T>> GetRelations<T>() where T : Node
    {
        var key = Helpers.GetNodeKeys<T>().First();
        return from node in Relations
               where node.Domain == key.Domain
                && node.Kind == key.Kind
                && node.TargetNode is not null
                && node.TargetNode.Domain == key.Domain
                && node.TargetNode.Kind == key.Kind
               select new NodeRelation<T>(node);
    }

}

[NodeDomain("Universal")]
[NodeKind("Participant")]
public class UniversalParticipant : Node
{
    [JsonPropertyName("Content")]
    public ContentInfo? Content { get; set; }
    [JsonPropertyName("Sex")]
    public string? Sex { get; set; }
    [JsonPropertyName("Title")]
    public string? Title { get; set; }
}

public class ChannelImage
{
    [JsonPropertyName("Role")]
    public string? Role { get; set; }
    [JsonPropertyName("ContentPath")]
    public string? ContentPath { get; set; }
}

[NodeKind("Image")]
public class Image : Node
{
    [JsonPropertyName("Shape")]
    public string? Shape { get; set; }
    [JsonPropertyName("ContentPath")]
    public string? ContentPath { get; set; }
    [JsonPropertyName("HasTitle")]
    public bool? HasTitle { get; set; }
    [JsonPropertyName("Languages")]
    public List<string>? Languages { get; set; }
}

public class ContentInfo
{
    [JsonPropertyName("Description")]
    public DescriptionInfo? Description { get; set; }

    [JsonPropertyName("Nodes")]
    public NodeCollection? Nodes { get; set; }
    public SeriesInfo? Series { get; set; }
    [JsonPropertyName("TechnicalAttributes")]
    public TechnicalAttributes? TechnicalAttributes { get; set; }
    [JsonPropertyName("ExternalGenres")]
    public List<string>? ExternalGenres { get; set; }
    [JsonPropertyName("Descriptors")]
    public List<string>? Descriptors { get; set; }
}

public class DescriptionInfo
{
    [JsonPropertyName("Title")]
    public string? Title { get; set; }

    [JsonPropertyName("Language")]
    public string? Language { get; set; }
    [JsonPropertyName("OriginalLanguage")]
    public string? OriginalLanguage { get; set; }

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
    [JsonPropertyName("Rating")]
    public string? Rating { get; set; }
}

public class SeriesInfo
{
    [JsonPropertyName("Identifier")]
    public string? Identifier { get; set; }
    [JsonPropertyName("Episode")]
    public int? Episode { get; set; }
    [JsonPropertyName("Season")]
    public int? Season { get; set; }

    [JsonPropertyName("GlobalSeriesIdentifier")]
    public string? GlobalSeriesIdentifier { get; set; }

    [JsonPropertyName("Part")]
    public int? Part { get; set; }

    [JsonPropertyName("PartsCount")]
    public int? PartsCount { get; set; }
    [JsonPropertyName("SeasonIdentifier")]
    public long? SeasonIdentifier { get; set; }
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

public class AvailabilitiesProperties
{
    [JsonPropertyName("MostRecentEpisodeEnd")]
    public Dictionary<string, DateTimeOffset>? MostRecentEpisodeEnd { get; set; }
    [JsonPropertyName("MostRecentEpisodeIdentifier")]
    public Dictionary<string, string>? MostRecentEpisodeIdentifier { get; set; }
    [JsonPropertyName("TVodAvailabilityStart")]
    public DateTimeOffset? TVodAvailabilityStart { get; set; }
}

public class NodeRelation : Node
{
    [JsonPropertyName("TargetNode")]
    public Node? TargetNode { get; set; }
    [JsonPropertyName("TargetIdentifier")]
    public string? TargetIdentifier { get; set; }
    [JsonPropertyName("Title")]
    public string? Title { get; set; }
}

public class NodeRelation<T> where T : Node
{
    public NodeRelation(NodeRelation node)
    {
        Relation = node;
        if (node.TargetNode is not T target)
        {
            throw new InvalidDataException($"Cannot cast TragetNode to {typeof(T).FullName}");
        }
        TargetNode = target;
    }

    public NodeRelation Relation { get; private set; }
    public T TargetNode { get; private set; }
}

[NodeDomain("TV")]
[NodeKind("Reference")]
[NodeRole("GenreId")]
public class TvReference : Node
{
    public string? TargetIdentifier { get; set; }
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
