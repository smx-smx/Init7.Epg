using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Init7.Epg;

public class RequestLoggingHandler<T> : DelegatingHandler
{
    private static readonly string TAG = typeof(T).Name;

    public RequestLoggingHandler() : base(new HttpClientHandler())
    {
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        Console.WriteLine($"[{TAG}] >> {request.RequestUri}");
        var response = await base.SendAsync(request, cancellationToken);
        return response;
    }
}
