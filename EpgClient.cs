using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading.Tasks;
using System.Web;

namespace Init7.Epg
{
    public class EpgClient : IDisposable
    {
        const string API_0 = "https://api.tv.init7.net/api/epg/";
        private readonly HttpClient _httpClient;

        public EpgClient()
        {
            _httpClient = new HttpClient();
        }

        public async Task<EpgResultList_high> GetEpg(long? offset = null, long? limit = null)
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

            var uriBuilder = new UriBuilder(new Uri(API_0));
            uriBuilder.Query = para.ToString();

            var resp = await _httpClient.GetAsync(uriBuilder.Uri);
            var body = await resp.Content.ReadFromJsonAsync<EpgResultList_low>(new JsonSerializerOptions
            {
                TypeInfoResolver = SerializationModeOptionsContext.Default
            });
            if (body is null)
            {
                throw new InvalidOperationException("EPG fetch failed");
            }
            return new EpgResultList_high(body);
        }

        public async Task<EpgResultList_high> GetNext(EpgResultList_high curr)
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

        public async Task<EpgResultList_high> GetPrevious(EpgResultList_high curr)
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