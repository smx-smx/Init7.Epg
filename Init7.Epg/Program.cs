using Init7.Epg;
using Init7.Epg.Init7;
using Init7.Epg.Teleboy;
using System.Data;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO.Compression;
using System.Reflection;
using System.Text.Json;

public class Program
{

    private EpgBuilder _epgOut;

    async Task Run(string configPath, string outFilePath)
    {
        ConfigurationSchema? schema = null;
        if(File.Exists(configPath))
        {
            Console.WriteLine($"Using config file: {configPath}");
            using var configFile = new FileStream(configPath, FileMode.Open, FileAccess.Read);
            schema = JsonSerializer.Deserialize(configFile, SerializationModeOptionsContext.Default.ConfigurationSchema);
        }

        var providers = new List<IEpgProvider>() {
            new Init7EpgProvider(),
            new TeleboyEpgProvider(new TeleboyEpgProviderConfig
            {
                TimeSpanBackwards = TimeSpan.FromHours(6),
                TimeSpanForward = TimeSpan.FromDays(2),
                AppendOnlyMode = true,
                ChannelMappings = schema?.TeleboyMappings
            })
        };
        foreach (var prov in providers)
        {
            Console.WriteLine($"Initializing: {prov.GetType()}");
            await prov.Initialize();
            Console.WriteLine($"FillEpg: {prov.GetType()}");
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

    public static async Task Main(string[] args)
    {
        var configPath = Path.Combine(AppContext.BaseDirectory, "config.json"); ;
        var outFilePath = args.ElementAtOrDefault(0) ?? "output.xml.gz";
        await new Program().Run(configPath, outFilePath);
    }
}

