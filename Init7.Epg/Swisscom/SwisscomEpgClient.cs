using Init7.Epg.Swisscom.Schema;
using System.Collections.Specialized;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Net.Mime;
using System.Runtime.InteropServices.Marshalling;
using System.Text.Json;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Init7.Epg.Swisscom
{
    public enum SwisscomEpgLanguage
    {
        English,
        French,
        German,
        Italian
    }

    public class SwisscomEpgClient : IDisposable
    {
        private readonly HttpClient _httpClient;
        private static readonly Uri API_CHANNELS = new Uri("https://services.sg101.prd.sctv.ch/portfolio/tv/channels");
        private static readonly Uri API_CATALOG = new Uri("https://services.sg101.prd.sctv.ch/catalog/tv/channels/list");
        private static readonly Uri API_GENRES = new Uri("https://services.sg101.prd.sctv.ch/catalog/tv/genres/detail");
        private static readonly Uri API_SERIES = new Uri("https://services.sg101.prd.sctv.ch/catalog/universal/series");

        public SwisscomEpgLanguage Language { get; set; } = SwisscomEpgLanguage.English;

        public SwisscomEpgClient()
        {
            _httpClient = new HttpClient(new RequestLoggingHandler<SwisscomEpgClient>());
        }

        public string GetLanguageCode()
        {
            return Language switch
            {
                SwisscomEpgLanguage.English => "en",
                SwisscomEpgLanguage.French => "fr",
                SwisscomEpgLanguage.German => "de",
                SwisscomEpgLanguage.Italian => "it",
                _ => throw new ArgumentException("invalid language")
            };
        }

        private HttpRequestMessage BuildRequest_v2(Uri uri, Dictionary<string, string>? para = null)
        {
            var uriBuilder = new UriBuilder(uri);
            if (para != null)
            {
                uriBuilder.Query = (System.Web.HttpUtility.ParseQueryString(string.Empty).Also(it =>
                {
                    foreach (var item in para)
                    {
                        it.Add(item.Key, item.Value);
                    }
                }).ToString() ?? "").Let(it =>
                {
                    if(it.Length > 0)
                    {
                        return $"?{it}";
                    }
                    return it;
                });
            }
            var req = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = uriBuilder.Uri,
            };
            req.Headers.Accept.ParseAdd("application/json");
            req.Headers.AcceptLanguage.ParseAdd(GetLanguageCode());
            req.Headers.Add("Origin", "https://tv.blue.ch");
            req.Headers.Referrer = new Uri("https://tv.blue.ch/");
            return req;
        }

        private HttpRequestMessage BuildRequest(Uri uri, Dictionary<string, string>? para = null)
        {
            var uriBuilder = new UriBuilder(uri);
            if (para != null)
            {
                var paraStr = para.Let(it => string.Join(';', it.Select(kv => $"{kv.Key}={kv.Value}")));
                uriBuilder.Path += $"/({paraStr})";
            }

            var req = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = uriBuilder.Uri,
            };
            req.Headers.Accept.ParseAdd("application/json");
            req.Headers.AcceptLanguage.ParseAdd(GetLanguageCode());
            req.Headers.Add("Origin", "https://tv.blue.ch");
            req.Headers.Referrer = new Uri("https://tv.blue.ch/");
            return req;
        }

        private static string SwisscomDateTime(DateTimeOffset date)
        {
            return $"{date:yyyyMMddHHmm}";
        }

        private static readonly Dictionary<SwisscomEpgLanguage, string> LANGUAGE_GENRE_IDMAP = new()
        {
            { SwisscomEpgLanguage.German, "DeGenres" },
            { SwisscomEpgLanguage.Italian, "ItGenres" },
            { SwisscomEpgLanguage.French, "FrGenres" },
            { SwisscomEpgLanguage.English, "EnGenres" }
        };

        private static async Task<T?> DeserializeResponseAsync<T>(HttpResponseMessage msg)
        {
            var opts = new JsonSerializerOptions
            {
                //UnmappedMemberHandling = System.Text.Json.Serialization.JsonUnmappedMemberHandling.Disallow,
                TypeInfoResolver = SerializationModeOptionsContext.Default
            };
            opts.Converters.Add(new MultiPropertyDiscriminatorConverter());
            return await msg.Content.ReadFromJsonAsync<T>(opts);
        }

        public async Task<SeriesResponse> GetSeriesInfo(string seriesId)
        {
            var uri = new UriBuilder(API_SERIES).Also(it => {
                it.Path += $"/{seriesId}";
            }).Uri;
            var req = BuildRequest_v2(uri, new Dictionary<string, string> {
                { "level", "normal" },
                { "primaryLang", GetLanguageCode() },
                { "secondaryLang", "noContent" }
            });
            var resp = await _httpClient.SendAsync(req);
            var data = await DeserializeResponseAsync<SeriesResponse>(resp);
            if(data == null)
            {
                throw new InvalidOperationException("Request failed");
            }
            return data;
        }

        public async Task<Dictionary<string, DescriptionInfo>?> GetGenres()
        {
            var req = BuildRequest(API_GENRES, new Dictionary<string, string> {
                { "level", "normal" }
            });
            var resp = await _httpClient.SendAsync(req);
            var data = await DeserializeResponseAsync<TvResponse>(resp);
            if (data == null)
            {
                throw new InvalidOperationException("Request failed");
            }

            if(data.Status.Status != "OK")
            {
                throw new InvalidOperationException($"Request failed with status \"{data.Status.Status}\"");
            }
            var items = data.Nodes.Items
                ?.Where(x => x is TvContainer container
                    && container.Identifier != null
                    && container.Content != null)
                ?.Cast<TvContainer>()
                ?.ToDictionary(x => x.Identifier!, x => x.Content!)
                ?? new Dictionary<string, ContentInfo>();

            if(!LANGUAGE_GENRE_IDMAP.TryGetValue(Language, out var itemKey))
            {
                return null;
            }

            if(!items.TryGetValue(itemKey, out var item) || item == null)
            {
                return null;
            }

            var genres = item.Nodes
                ?.Items
                ?.Where(x => x is TvGenre)
                ?.Cast<TvGenre>() ?? Enumerable.Empty<TvGenre>();

            var genreMap = genres.Select(
                x => new
                {
                    GenreId = x.Relations
                        ?.Where(x => x is TvReference)
                        ?.Cast<TvReference>()
                        ?.FirstOrDefault(x => x.Role == "GenreId")
                        ?.TargetIdentifier,
                    Description = x.Content?.Description
                })
                .Where(x => x.GenreId != null && x.Description?.Title != null)
                .ToDictionary(x => x.GenreId!, x => x.Description!);

            return genreMap;
        }

        public async Task<TvResponse> GetEpg(
            DateTimeOffset begin,
            DateTimeOffset end,
            IEnumerable<string> channelIds
        )
        {
            var req = BuildRequest(API_CATALOG, new Dictionary<string, string>
            {
                { "level", "normal" },
                { "start",  SwisscomDateTime(begin) },
                { "end", SwisscomDateTime(end) },
                { "ids",  string.Join(',', channelIds) }
            });
            var resp = await _httpClient.SendAsync(req);
            var data = await DeserializeResponseAsync<TvResponse>(resp);
            if (data == null)
            {
                throw new InvalidOperationException("Request failed");
            }

            return data;
        }

        public async Task<List<Channel>> GetChannels()
        {
            var req = BuildRequest(API_CHANNELS);

            var resp = await _httpClient.SendAsync(req);
            var data = await DeserializeResponseAsync<List<Channel>>(resp);
            if(data == null)
            {
                throw new InvalidOperationException("Request failed");
            }
            return data;
        }

        public void Dispose()
        {
            _httpClient.Dispose();
        }
    }
}