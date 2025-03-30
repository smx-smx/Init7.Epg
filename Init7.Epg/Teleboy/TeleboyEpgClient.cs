using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Web;

namespace Init7.Epg.Teleboy
{
    public class TeleboyEpgClient : IDisposable
    {
        private readonly HttpClient _httpClient;

        public TeleboyEpgClient()
        {
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Add("X-Teleboy-ApiKey", "e899f715940a209148f834702fc7f340b6b0496b62120b3ed9c9b3ec4d7dca00");
        }

        public async Task<TeleboyGenreApiResponse?> GetGenres()
        {
            var uriBuilder = new UriBuilder("https://tv.api.teleboy.ch/epg/genres");
            var resp = await _httpClient.GetAsync(uriBuilder.Uri);
            var body = await resp.Content.ReadFromJsonAsync(
                typeof(TeleboyGenreApiResponse),
                SerializationModeOptionsContext.Default) as TeleboyGenreApiResponse;
            return body;
        }

        public async Task<TeleboyEpgResponse?> GetEpg(
            DateTimeOffset? begin = default,
            DateTimeOffset? end = default,
            int offset = 0,
            int limit = 0)
        {
            var para = HttpUtility.ParseQueryString("");
            if (begin.HasValue)
            {
                para["begin"] = TeleboyConverters.ConvertDateTimeApiParameter(begin.Value);
            }
            if (end.HasValue)
            {
                para["end"] = TeleboyConverters.ConvertDateTimeApiParameter(end.Value);
            }
            para["expand"] = "station,logos,flags,primary_image";
            if (offset > 0)
            {
                para["skip"] = offset.ToString();
            }
            para["limit"] = limit.ToString();

            var uriBuilder = new UriBuilder("https://tv.api.teleboy.ch/epg/broadcasts");
            uriBuilder.Query = para.ToString();

            Console.WriteLine(uriBuilder.Uri.ToString());
            var resp = await _httpClient.GetAsync(uriBuilder.Uri);
            var body = await resp.Content.ReadFromJsonAsync(
                typeof(TeleboyEpgResponse),
                SerializationModeOptionsContext.Default) as TeleboyEpgResponse;
            return body;
        }

        public void Dispose()
        {
            _httpClient.Dispose();
        }
    }
}
