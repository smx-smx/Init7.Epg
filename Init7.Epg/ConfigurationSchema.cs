using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Init7.Epg
{
    public class EpgProviderConfig
    {
        [JsonPropertyName("enabled")]
        public bool Enabled { get; set; }
        [JsonPropertyName("fetch_back")]
        public TimeSpan? FetchBack { get; set; }
        [JsonPropertyName("fetch_forward")]
        public TimeSpan? FetchForward { get; set; }
        [JsonPropertyName("standalone")]
        public bool StandaloneMode { get; set; }
        [JsonPropertyName("fuzzy_max_delta")]
        public TimeSpan FuzzyMaxDelta { get; set; } = TimeSpan.Zero;
    }

    public class Init7ConfigurationSchema
    {
        [JsonPropertyName("common")]
        public EpgProviderConfig? ProviderConfig { get; set; }
    }

    public class TeleboyConfigurationSchema
    {
        [JsonPropertyName("common")]
        public EpgProviderConfig? ProviderConfig { get; set; }
        [JsonPropertyName("mappings")]
        public Dictionary<string, string> Mappings { get; set; } = new Dictionary<string, string>();
    }

    public class SwisscomConfigurationSchema
    {
        [JsonPropertyName("common")]
        public EpgProviderConfig? ProviderConfig { get; set; }
        [JsonPropertyName("replace")]
        public bool ReplaceEpg { get; set; }
        [JsonPropertyName("language")]
        public string Language { get; set; } = "en";
        [JsonPropertyName("mappings")]
        public Dictionary<string, string> Mappings { get; set; } = new Dictionary<string, string>();
    }

    public class ConfigurationSchema
    {
        [JsonPropertyName("init7")]
        public Init7ConfigurationSchema? Init7 { get; set; }
        [JsonPropertyName("teleboy")]
        public TeleboyConfigurationSchema? Teleboy { get; set; }
        [JsonPropertyName("swisscom")]
        public SwisscomConfigurationSchema? Swisscom { get; set; }
    }
}
