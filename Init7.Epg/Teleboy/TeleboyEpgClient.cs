using System.Globalization;
using System.Net.Http.Json;
using System.Web;

namespace Init7.Epg.Teleboy
{
    public class TeleboyEpgClient : IDisposable
    {
        private readonly HttpClient _httpClient;
        private bool disposedValue;

        public TeleboyEpgClient()
        {
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Add("X-Teleboy-ApiKey", "e899f715940a209148f834702fc7f340b6b0496b62120b3ed9c9b3ec4d7dca00");
        }

        public async Task<TeleboyGenreApiResponse?> GetGenres()
        {
            var uriBuilder = new UriBuilder(Constants.TELEBOYGENRESAPIURL);
            var resp = await _httpClient.GetAsync(uriBuilder.Uri).ConfigureAwait(false);
            var body = await resp.Content.ReadFromJsonAsync(
                typeof(TeleboyGenreApiResponse),
                SerializationModeOptionsContext.Default).ConfigureAwait(false) as TeleboyGenreApiResponse;
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
                para["skip"] = offset.ToString(CultureInfo.InvariantCulture);
            }
            para["limit"] = limit.ToString(CultureInfo.InvariantCulture);

            var uriBuilder = new UriBuilder(Constants.TELEBOYBROADCASTSAPIURL)
            {
                Query = para.ToString()
            };

            Console.WriteLine(uriBuilder.Uri.ToString());
            var resp = await _httpClient.GetAsync(uriBuilder.Uri).ConfigureAwait(false);
            var body = await resp.Content.ReadFromJsonAsync(
                typeof(TeleboyEpgResponse),
                SerializationModeOptionsContext.Default).ConfigureAwait(false) as TeleboyEpgResponse;
            return body;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _httpClient.Dispose();
                }

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
