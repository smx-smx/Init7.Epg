using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace Init7.Epg.Teleboy
{
    public class TeleboyEpgResponse
    {
        [JsonPropertyName("success")]
        public bool Success { get; set; }

        [JsonPropertyName("data")]
        public EpgData? Data { get; set; }
    }

    public class EpgData
    {
        [JsonPropertyName("total")]
        public int Total { get; set; }

        [JsonPropertyName("items")]
        public List<ProgramItem> Items { get; set; } = [];
    }

    // --- Detailed Station Information ---

    public class LogoTypes
    {
        [JsonPropertyName("light")]
        public string? Light { get; set; }

        [JsonPropertyName("dark")]
        public string? Dark { get; set; }
    }

    public class LogoSizes
    {
        [JsonPropertyName("S")]
        public int S { get; set; }

        [JsonPropertyName("M")]
        public int M { get; set; }

        [JsonPropertyName("L")]
        public int L { get; set; }

        [JsonPropertyName("XL")]
        public int XL { get; set; }
    }

    public class StationLogos
    {
        [JsonPropertyName("types")]
        public Dictionary<string, string> Types { get; set; } = [];

        [JsonPropertyName("sizes")]
        public Dictionary<string, int> Sizes { get; set; } = [];

        [JsonPropertyName("path")]
        public string? Path { get; set; } // URL template
    }

    public class StationFeatures
    {
        [JsonPropertyName("stream")]
        public bool Stream { get; set; }

        [JsonPropertyName("dolby")]
        public bool Dolby { get; set; }

        [JsonPropertyName("dual_audio")]
        public bool DualAudio { get; set; }

        [JsonPropertyName("captions")]
        public bool Captions { get; set; }
    }

    public class Mediapuls
    {
        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [JsonPropertyName("name")]
        public string? Name { get; set; }
    }

    public partial class Station
    {
        [JsonPropertyName("logos")]
        public StationLogos? Logos { get; set; }

        [JsonPropertyName("station_group_id")]
        public int StationGroupId { get; set; }

        [JsonPropertyName("has_stream")]
        public bool HasStream { get; set; }

        [JsonPropertyName("features")]
        public StationFeatures? Features { get; set; }

        [JsonPropertyName("mediapuls")]
        public Mediapuls? Mediapuls { get; set; } // Optional

        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("country")]
        public string? Country { get; set; }

        [JsonPropertyName("name")]
        public string? Name { get; set; } // Detailed station name

        [JsonPropertyName("label")]
        public string? Label { get; set; } // Same as station_label in ProgramItem?

        [JsonPropertyName("slug")]
        public string? Slug { get; set; }

        [JsonPropertyName("language")]
        public string? Language { get; set; }

        [JsonPropertyName("quality")]
        public string? Quality { get; set; } // e.g., "sd", "hd", "fullhd"

        [JsonPropertyName("is_cpvr")]
        public bool IsCpvr { get; set; } // Cloud PVR?

        [JsonPropertyName("is_downloadable")]
        public bool IsDownloadable { get; set; }

        [JsonPropertyName("premium_type")] // Added for MySports example
        public string? PremiumType { get; set; }


        public string GetChannelId()
        {
            var labelConverted = Label?.Let(it => LabelReplace().Replace(it, ""));
            var countryConverted = Country switch
            {
                "gb" => "uk",
                _ => Country
            };
            return $"{labelConverted}.{countryConverted}";
        }

        [GeneratedRegex(@"[ \!]")]
        private static partial Regex LabelReplace();
    }

    // --- Program Image Information ---

    public class PrimaryImage
    {
        [JsonPropertyName("base_path")]
        public string? BasePath { get; set; }

        [JsonPropertyName("hash")]
        public string? Hash { get; set; }

        [JsonPropertyName("source")]
        public string? Source { get; set; } // Optional source info

        [JsonPropertyName("title")]
        public string? Title { get; set; } // Optional image title

        [JsonPropertyName("copyright")]
        public string? Copyright { get; set; } // Optional copyright info

        [JsonPropertyName("type")]
        public string? Type { get; set; } // e.g., "epg", "live"
    }

    // --- Program Flags ---

    public class Flags
    {
        [JsonPropertyName("is_seriable")]
        public bool IsSeriable { get; set; }

        [JsonPropertyName("is_tip")]
        public bool IsTip { get; set; }

        [JsonPropertyName("has_trailer")]
        public bool HasTrailer { get; set; }

        [JsonPropertyName("has_alternative_stream")]
        public bool HasAlternativeStream { get; set; }

        [JsonPropertyName("is_watchlistable")]
        public bool IsWatchlistable { get; set; }

        [JsonPropertyName("is_playable")]
        public bool IsPlayable { get; set; }

        [JsonPropertyName("is_playing")]
        public bool IsPlaying { get; set; }

        [JsonPropertyName("is_recordable")]
        public bool IsRecordable { get; set; }

        [JsonPropertyName("is_replayable")]
        public bool IsReplayable { get; set; }

        [JsonPropertyName("is_replay_range")]
        public bool IsReplayRange { get; set; }
    }


    // --- Updated Program Item ---

    public class ProgramItem
    {
        [JsonPropertyName("is_dual_audio")]
        public bool IsDualAudio { get; set; }

        [JsonPropertyName("has_caption")]
        public bool HasCaption { get; set; }

        [JsonPropertyName("has_dolby")]
        public bool HasDolby { get; set; }

        [JsonPropertyName("subtitle")]
        public string? Subtitle { get; set; }

        [JsonPropertyName("station_id")]
        public int StationId { get; set; } // Still present, redundant with Station.Id

        [JsonPropertyName("station_label")]
        public string? StationLabel { get; set; } // Still present, redundant with Station.Label

        [JsonPropertyName("slug")]
        public string? Slug { get; set; }

        [JsonPropertyName("type")]
        public string? Type { get; set; }

        [JsonPropertyName("primary_image")] // New object
        public PrimaryImage? PrimaryImage { get; set; }

        [JsonPropertyName("flags")] // New object
        public Flags? Flags { get; set; }

        [JsonPropertyName("id")]
        public long Id { get; set; }

        [JsonPropertyName("begin")]
        [JsonConverter(typeof(TeleboyDateTimeOffsetConverter))]
        public DateTimeOffset? Begin { get; set; }

        [JsonPropertyName("end")]
        [JsonConverter(typeof(TeleboyDateTimeOffsetConverter))]
        public DateTimeOffset? End { get; set; }

        [JsonPropertyName("title")]
        public string? Title { get; set; }

        [JsonPropertyName("station")] // New detailed station object
        public Station? Station { get; set; }

        [JsonPropertyName("duration")]
        public int Duration { get; set; }

        [JsonPropertyName("is_audio_description")]
        public bool IsAudioDescription { get; set; }

        [JsonPropertyName("new")]
        public bool New { get; set; }

        [JsonPropertyName("short_description")]
        public string? ShortDescription { get; set; }

        [JsonPropertyName("genre_id")]
        public int? GenreId { get; set; }

        [JsonPropertyName("original_title")]
        public string? OriginalTitle { get; set; }

        [JsonPropertyName("country")]
        public string? Country { get; set; }

        [JsonPropertyName("year")]
        public int? Year { get; set; }

        [JsonPropertyName("serie_episode_title")]
        public string? SerieEpisodeTitle { get; set; }

        [JsonPropertyName("serie_episode")]
        public int? SerieEpisode { get; set; }

        [JsonPropertyName("serie_season")]
        public int? SerieSeason { get; set; }

        [JsonPropertyName("serie_episodes")]
        public int? SerieEpisodes { get; set; }

        [JsonPropertyName("serie_group_id")] // New optional field
        public int? SerieGroupId { get; set; }

        [JsonPropertyName("age")]
        public int? Age { get; set; }

        [JsonPropertyName("headline")]
        public string? Headline { get; set; }

        [JsonPropertyName("imdb_id")]
        public string? ImdbId { get; set; }

        [JsonPropertyName("imdb_rating")]
        public double? ImdbRating { get; set; }

        [JsonPropertyName("imdb_vote_count")]
        public int? ImdbVoteCount { get; set; }
    }

    // Root object representing the entire JSON response
    public class TeleboyGenreApiResponse
    {
        [JsonPropertyName("success")]
        public bool Success { get; set; }

        [JsonPropertyName("data")]
        public required GenreData Data { get; set; }
    }

    // Represents the nested "data" object
    public class GenreData
    {
        [JsonPropertyName("total")]
        public int Total { get; set; }

        [JsonPropertyName("items")]
        public List<GenreItem> Items { get; set; } = [];
    }

    // Represents an item within the "items" array (a main genre)
    public class GenreItem
    {
        // Note: This list might be null if a genre item doesn't have sub_genres
        [JsonPropertyName("sub_genres")]
        public List<SubGenre> SubGenres { get; set; } = [];

        [JsonPropertyName("name")]
        public required string Name { get; set; }

        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("slug")]
        public required string Slug { get; set; }

        [JsonPropertyName("name_de")]
        public required string NameDe { get; set; }

        [JsonPropertyName("name_en")]
        public required string NameEn { get; set; }

        [JsonPropertyName("name_fr")]
        public required string NameFr { get; set; }

        [JsonPropertyName("name_it")]
        public required string NameIt { get; set; }

        [JsonPropertyName("description")]
        public required string Description { get; set; }

        [JsonPropertyName("sort")]
        public int Sort { get; set; }
    }

    // Represents an object within the "sub_genres" array
    public class SubGenre
    {
        [JsonPropertyName("name")]
        public required string Name { get; set; }

        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("slug")]
        public required string Slug { get; set; }

        [JsonPropertyName("name_de")]
        public required string NameDe { get; set; }

        [JsonPropertyName("name_en")]
        public required string NameEn { get; set; }

        [JsonPropertyName("name_fr")]
        public required string NameFr { get; set; }

        [JsonPropertyName("name_it")]
        public required string NameIt { get; set; }

        [JsonPropertyName("description")]
        public required string Description { get; set; }

        [JsonPropertyName("sort")]
        public int Sort { get; set; }
    }
}
