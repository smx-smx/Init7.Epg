using Init7.Epg.Init7;
using Init7.Epg.Schema;
using Init7.Epg.Swisscom.Schema;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Init7.Epg.Teleboy
{
    public class TeleboyEpgProviderConfig
    {
        public TimeSpan TimeSpanBackwards { get; set; } = TimeSpan.FromHours(6);
        public TimeSpan TimeSpanForward { get; set; } = TimeSpan.FromDays(2);
        /// <summary>
        /// Only add data for existing channels (from Init7) in the EPG map
        /// </summary>
        public bool StandaloneMode { get; set; } = false;
        public Dictionary<string, string>? ChannelMappings { get; set; }
    }

    public class TeleboyEpgProvider : IEpgProvider, IDisposable
    {
        private readonly TeleboyEpgProviderConfig _config;
        private readonly TeleboyEpgClient _client;
        private TeleboyGenreApiResponse? _genres;
        private IDictionary<int, GenreItem> _genreMap;

        private readonly HashSet<string> _channelWarnings = new HashSet<string>();

        /// <summary>
        /// mapping for specific channel names that are called differently
        /// </summary>
        private static readonly Dictionary<string, string> _teleboyToInit7 = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase)
        {
        };

        private readonly HashSet<string> _teleboyChannels = new HashSet<string>();

        void HandleChunk_Teleboy(
            TeleboyEpgResponse epgIn,
            EpgBuilder epgOut)
        {
            var dtNow = DateTimeOffset.Now;
            var dtMin = dtNow;
            var dtMax = dtNow;

            if (!epgIn.Success || epgIn.Data == null) return;
            foreach (var itm in epgIn.Data.Items)
            {
                if (!itm.Begin.HasValue) continue;

                var station = itm.Station;
                if (station == null) continue;

                {
                    var label = station.Label;
                    if(label != null)
                    {
                        _teleboyChannels.Add(label);
                    }
                }

                var id = station.GetChannelId();
                var chan_out = new channel
                {
                    displayname = Constants.DISPLAY_LANGS.Select(lang => new displayname
                    {
                        lang = lang,
                        Value = id
                    }).ToArray(),
                    icon = TeleboyConverters.ConvertLogos(itm.Station?.Logos ?? new StationLogos()),
                    id = id,
                };
                if (_config.StandaloneMode)
                {
                    // if we're not in append only mode, we add any channel we see
                    epgOut.TryAddChannel(chan_out);
                } else
                {
                    // check if we have this channel from Init7
                    if (epgOut.TryGetChannel(id, out var existing))
                    {
                        // copy the channel ID from the existing entry
                        // (in case the casing is different)
                        chan_out.id = existing.id;
                    }
                    // check if it's called differently
                    else if (_teleboyToInit7.TryGetValue(id, out var mapValue))
                    {
                        id = mapValue;
                        chan_out.id = mapValue;
                    } else
                    {
                        if (!_channelWarnings.Contains(id))
                        {
                            _channelWarnings.Add(id);
                            Console.WriteLine($"Couldn't find channel \"{id}\", skipping");
                        }
                        continue;
                    }
                }

                var prg = new programme
                {
                    title = CommonConverters.ConvertSingleNullable(itm.Title, value => new title
                    {
                        Value = value
                    }),
                    subtitle = CommonConverters.ConvertSingleNullable(itm.Subtitle, value => new subtitle
                    {
                        Value = value
                    }),
                    category = itm.GenreId?.Let(id =>
                    {
                        if (_genreMap.TryGetValue(id, out var genre)) return genre;
                        return null;
                    })?.Let(TeleboyConverters.ConvertGenre),
                    channel = chan_out.id,
                    start = CommonConverters.ConvertDateTimeXmlTv(itm.Begin.Value),
                    stop = itm.End.HasValue ? CommonConverters.ConvertDateTimeXmlTv(itm.End.Value) : null,
                    length = itm.End.HasValue ? new length
                    {
                        units = lengthUnits.seconds,
                        Value = (itm.End.Value - itm.Begin.Value).TotalSeconds.ToString()
                    } : null,
                    desc = CommonConverters.ConvertSingleNullable(itm.ShortDescription, value => new desc
                    {
                        Value = value
                    })
                };
                if(!epgOut.TryAddProgramme(
                    itm.Begin.Value,
                    itm.End,
                    prg))
                {
                    Console.Error.WriteLine($"Failed to add program \"{prg.title?.FirstOrDefault()?.Value ?? string.Empty}\" to channel {chan_out.id}. " +
                            $"Start Time: {itm.Begin.Value}");
                }
            }
        }


        async Task<TeleboyEpgResponse?> FetchTeleboyEpg(
            DateTimeOffset from, DateTimeOffset to,
            int offset, int limit,
            EpgBuilder epgOut
        )
        {
            var epgIn = await _client.GetEpg(from, to, offset: offset, limit);
            if (epgIn == null || !epgIn.Success)
            {
                Console.Error.WriteLine("Failed to fetch Teleboy EPG");
                return null;
            }

            HandleChunk_Teleboy(epgIn, epgOut);
            return epgIn;
        }

        async Task GetTeleboyEpg(EpgBuilder epgOut, DateTimeOffset from, DateTimeOffset to)
        {
            var offset = 0;
            const int limit = 2000;

            var epgIn = await FetchTeleboyEpg(from, to, offset, limit, epgOut);
            while (epgIn != null
                && epgIn.Success
                && epgIn.Data != null
                && epgIn.Data.Items.Count > 0
                && offset < epgIn.Data.Total)
            {
                offset += epgIn.Data.Items.Count;
                epgIn = await FetchTeleboyEpg(from, to, offset, limit, epgOut);
            }
        }

        public TeleboyEpgProvider(TeleboyEpgProviderConfig config)
        {
            _config = config;
            _client = new TeleboyEpgClient();
            _genreMap = new Dictionary<int, GenreItem>();
            if (config.ChannelMappings != null)
            {
                _teleboyToInit7.Clear();
                foreach (var mapping in config.ChannelMappings)
                {
                    _teleboyToInit7.Add(mapping.Key, mapping.Value);
                }
            }
        }

        public async Task Initialize()
        {
            _genres = await _client.GetGenres();
            if (_genres != null && _genres.Success)
            {
                _genreMap = _genres.Data.Items.ToDictionary(
                    itm => itm.Id,
                    itm => itm
                );
            }
        }

        private async Task DumpChannelListAsync(string filePath)
        {
            using var fh = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.Read);
            fh.SetLength(0);

            using var writer = new StreamWriter(fh, new UTF8Encoding(false));

            foreach (var ch in _teleboyChannels.Order())
            {
                await writer.WriteLineAsync(ch);
            }
        }

        public async Task FillEpg(EpgBuilder epgOut)
        {
            var referenceTime = DateTimeOffset.Now;
            var boundHigh = referenceTime.Add(_config.TimeSpanForward);
            var boundLow = referenceTime.Subtract(_config.TimeSpanBackwards);
            await GetTeleboyEpg(epgOut, boundLow, boundHigh);
            await DumpChannelListAsync("channels_teleboy.txt");

        }

        public void Dispose()
        {
            _client.Dispose();
        }
    }
}
