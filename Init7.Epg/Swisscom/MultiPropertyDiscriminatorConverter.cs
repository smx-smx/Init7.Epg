using Init7.Epg.Swisscom.Schema;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Init7.Epg.Swisscom
{
    public class NodeKey
    {
        public string? Domain { get; set; }
        public required string Kind { get; set; }
        public string? Role { get; set; }

        public override bool Equals(object? obj)
        {
            return obj is NodeKey other &&
               Domain == other?.Domain
               && Kind == other?.Kind
               && Role == other?.Role;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(
                Domain?.GetHashCode() ?? 0,
                Kind?.GetHashCode() ?? 0,
                Role?.GetHashCode() ?? 0);
        }

        public override string ToString()
        {
            return $"NodeKey(Domain={Domain},Kind={Kind},Role={Role})";
        }
    }

    public class MultiPropertyDiscriminatorConverter : JsonConverter<Node>
    {
        private static readonly ImmutableDictionary<NodeKey, Type> TYPE_MAP = BuildTypeMap();

        private static ImmutableDictionary<NodeKey, Type> BuildTypeMap()
        {
            var x = typeof(NodeKey).Assembly.GetTypes()
                .Where(x => typeof(Node).IsAssignableFrom(x)
                    && x.GetCustomAttribute<NodeKindAttribute>() != null
                )
                .Select(x => Helpers.GetNodeKeys(x).Select(k => KeyValuePair.Create(k, x)))
                .SelectMany(x => x)
                .ToImmutableDictionary();
            return x;
        }

        public override Node? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            using (JsonDocument doc = JsonDocument.ParseValue(ref reader))
            {
                var root = doc.RootElement;

                string? domainText = null;
                string? kindText = null;
                string? roleText = null;

                var hasDomain = root.TryGetProperty("Domain", out var domain)
                    && (domainText = domain.GetString()) != null;

                if(!root.TryGetProperty("Kind", out var kind)
                    || (kindText = kind.GetString()) == null)
                {
                    throw new JsonException("Cannot deserialize Node: missing 'Kind' property");
                }
                var hasRole = root.TryGetProperty("Role", out var role)
                    && (roleText = role.GetString()) != null;

                if(!TYPE_MAP.TryGetValue(new NodeKey
                {
                    Kind = kindText,
                    Domain = domainText,
                    Role = roleText
                }, out var type)
                    && !TYPE_MAP.TryGetValue(new NodeKey
                    {
                        Kind = kindText,
                        Domain = domainText
                    }, out type)
                    && !TYPE_MAP.TryGetValue(new NodeKey
                    {
                        Kind = kindText,
                    }, out type))
                {
                    throw new JsonException($"Cannot deserialize Node: unknown key <{domainText},{kindText},{roleText}>\n" +
                        $"Raw Text: {root.GetRawText()}");
                }

                return JsonSerializer.Deserialize(root, type, options) as Node;
            }
        }

        public override void Write(Utf8JsonWriter writer, Node value, JsonSerializerOptions options)
        {
            JsonSerializer.Serialize(writer, value, value.GetType(), options);
        }
    }
}
