using Init7.Epg.Init7;
using Init7.Epg.Teleboy;
using System.Data;
using System.IO.Compression;

namespace Init7.Epg
{
    public class Program
    {

        private readonly EpgBuilder _epgOut;

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
                Console.WriteLine($"Initializing: {prov.GetType()}");
                await prov.Initialize().ConfigureAwait(false);
                Console.WriteLine($"FillEpg: {prov.GetType()}");
                await prov.FillEpg(_epgOut).ConfigureAwait(false);
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
            await stream.FlushAsync().ConfigureAwait(false);
            await stream.DisposeAsync().ConfigureAwait(false);

            if (isCompressed)
            {
                await fileStream.DisposeAsync().ConfigureAwait(false);
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
            var outFilePath = args.ElementAtOrDefault(0) ?? "output.xml";
            await new Program().Run(outFilePath).ConfigureAwait(false);
        }
    }
}