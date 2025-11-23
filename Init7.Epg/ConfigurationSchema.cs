using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Init7.Epg
{
    public class ConfigurationSchema
    {
        [JsonPropertyName("teleboy_mappings")]
        public Dictionary<string, string> TeleboyMappings { get; set; } = new Dictionary<string, string>();
    }
}
