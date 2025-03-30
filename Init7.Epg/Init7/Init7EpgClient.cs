using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading.Tasks;
using System.Web;

namespace Init7.Epg.Init7
{
    public class Init7EpgClient : IDisposable
    {
        const string API_0 = "https://api.tv.init7.net/api/epg/";
        private readonly HttpClient _httpClient;

        public Init7EpgClient()
        {
            _httpClient = new HttpClient();
        }

        public async Task<EpgResultList?> GetEpg(long? offset = null, long? limit = null)
        {
            var para = HttpUtility.ParseQueryString("");
            if (offset != null)
            {
                para["offset"] = offset.ToString();
            }
            if (limit != null)
            {
                para["limit"] = limit.ToString();
            }

            var uriBuilder = new UriBuilder(API_0);
            uriBuilder.Query = para.ToString();

            var resp = await _httpClient.GetAsync(uriBuilder.Uri);
            Console.WriteLine(uriBuilder.Uri.ToString());
            var body = await resp.Content.ReadFromJsonAsync(
                typeof(EpgResultList),
                SerializationModeOptionsContext.Default) as EpgResultList;
            if (body == null)
            {
                throw new InvalidOperationException("EPG fetch failed");
            }
            return body;
        }

        public async Task<EpgResultList?> GetNext(EpgResultList curr)
        {
            if (curr.NextUri == null)
            {
                throw new InvalidOperationException("Cannot fetch next EPG data. End of Data");
            }
            var qs = HttpUtility.ParseQueryString(curr.NextUri.Query);
            return await GetEpg(
                qs["offset"]?.Let(Convert.ToInt64),
                qs["limit"]?.Let(Convert.ToInt64)
            );
        }

        public async Task<EpgResultList?> GetPrevious(EpgResultList curr)
        {
            if (curr.PreviousUri == null)
            {
                throw new InvalidOperationException("Cannot fetch previous EPG data. End of Data");
            }
            var qs = HttpUtility.ParseQueryString(curr.PreviousUri.Query);
            return await GetEpg(
                qs["offset"]?.Let(Convert.ToInt64),
                qs["limit"]?.Let(Convert.ToInt64)
            );
        }

        public void Dispose()
        {
            _httpClient.Dispose();
        }
    }
}