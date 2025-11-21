using System.Globalization;
using System.Net.Http.Json;
using System.Web;

namespace Init7.Epg.Init7
{
    public class Init7EpgClient : IDisposable
    {
        private readonly HttpClient _httpClient;
        private bool disposedValue;

        public Init7EpgClient()
        {
            _httpClient = new HttpClient();
        }

        public async Task<EpgResultList?> GetEpg(long? offset = null, long? limit = null)
        {
            var para = HttpUtility.ParseQueryString("");
            if (offset != null)
            {
                para["offset"] = offset.Value.ToString(CultureInfo.InvariantCulture);
            }
            if (limit != null)
            {
                para["limit"] = limit.Value.ToString(CultureInfo.InvariantCulture);
            }

            var uriBuilder = new UriBuilder(Constants.INIT7APIURL)
            {
                Query = para.ToString()
            };

            var resp = await _httpClient.GetAsync(uriBuilder.Uri).ConfigureAwait(false);
            Console.WriteLine(uriBuilder.Uri.ToString());

            if (await resp.Content.ReadFromJsonAsync(
                typeof(EpgResultList),
                SerializationModeOptionsContext.Default).ConfigureAwait(false) is not EpgResultList body)
            {
                throw new InvalidOperationException("EPG fetch failed");
            }
            return body;
        }

        public async Task<EpgResultList?> GetNext(EpgResultList curr)
        {
            ArgumentNullException.ThrowIfNull(curr);

            if (curr.NextUri == null)
            {
                throw new InvalidOperationException("Cannot fetch next EPG data. End of Data");
            }
            var qs = HttpUtility.ParseQueryString(curr.NextUri.Query);
            return await GetEpg(
                qs["offset"]?.Let(Convert.ToInt64),
                qs["limit"]?.Let(Convert.ToInt64)
            ).ConfigureAwait(false);
        }

        public async Task<EpgResultList?> GetPrevious(EpgResultList curr)
        {
            ArgumentNullException.ThrowIfNull(curr);

            if (curr.PreviousUri == null)
            {
                throw new InvalidOperationException("Cannot fetch previous EPG data. End of Data");
            }
            var qs = HttpUtility.ParseQueryString(curr.PreviousUri.Query);
            return await GetEpg(
                qs["offset"]?.Let(Convert.ToInt64),
                qs["limit"]?.Let(Convert.ToInt64)
            ).ConfigureAwait(false);
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