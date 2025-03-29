using Init7.Epg;
using Init7.Epg.Init7;
using Init7.Epg.Schema;
using Init7.Epg.Teleboy;
using m3uParser;
using m3uParser.Model;
using System.Data;
using System.Diagnostics;

public class Program
{

    private EpgBuilder _epgOut;

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
        _epgOut.BuildToFile(outFilePath);
    }

    public Program()
    {
        _epgOut = new EpgBuilder();
    }

    public static async Task Main(string[] args)
    {
        Trace.Listeners.Add(new ConsoleTraceListener());

        var outFilePath = args.ElementAtOrDefault(0) ?? "output.xml";
        await new Program().Run(outFilePath);
    }
}

