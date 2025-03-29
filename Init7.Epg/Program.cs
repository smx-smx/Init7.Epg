using Init7.Epg;
using Init7.Epg.Init7;
using Init7.Epg.Teleboy;
using System.Data;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO.Compression;

public class Program
{

    private EpgBuilder _epgOut;

    [RequiresUnreferencedCode("XmlSerializer")]
    async Task Run(string outFilePath)
    {
        var providers = new List<IEpgProvider>() {
            new Init7EpgProvider(),
            new TeleboyEpgProvider(new TeleboyEpgProviderConfig
            {
                TimeSpanBackwards = TimeSpan.FromHours(6),
                TimeSpanForward = TimeSpan.FromDays(2),
                AppendOnlyMode = true
            })
        };
        foreach (var prov in providers)
        {
            await prov.Initialize();
            await prov.FillEpg(_epgOut);
        }

        var fileStream = new FileStream(
            outFilePath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.ReadWrite);
        fileStream.SetLength(0);

        Stream stream = fileStream;

        var isCompressed = outFilePath.EndsWith(".gz", StringComparison.InvariantCultureIgnoreCase);
        if (isCompressed)
        {
            var gzipStream = new GZipStream(fileStream, CompressionLevel.Optimal);
            stream = gzipStream;
        }

        _epgOut.BuildToStream(stream);
        await stream.FlushAsync();
        await stream.DisposeAsync();

        if (isCompressed)
        {
            await fileStream.DisposeAsync();
        }

        foreach (var prov in providers)
        {
            if (prov is IDisposable disp)
            {
                disp.Dispose();
            }
        }
    }

    public Program()
    {
        _epgOut = new EpgBuilder();
    }

    [RequiresUnreferencedCode("XmlSerializer")]
    public static async Task Main(string[] args)
    {
        Trace.Listeners.Add(new ConsoleTraceListener());

        var outFilePath = args.ElementAtOrDefault(0) ?? "output.xml.gz";
        await new Program().Run(outFilePath);
    }
}

