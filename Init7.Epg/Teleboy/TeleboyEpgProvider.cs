using Init7.Epg.Init7;
using Init7.Epg.Schema;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
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
        public bool AppendOnlyMode { get; set; } = false;
    }

    public class TeleboyEpgProvider : IEpgProvider, IDisposable
    {
        private readonly TeleboyEpgProviderConfig _config;
        private readonly TeleboyEpgClient _client;
        private TeleboyGenreApiResponse? _genres;
        private IDictionary<int, GenreItem> _genreMap;

        private HashSet<string> _channelWarnings = new HashSet<string>();

        /// <summary>
        /// mapping for specific channel names that are called differently
        /// </summary>
        private static readonly Dictionary<string, string> _teleboyToInit7 = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase)
        {
            // swiss
            { "blueZoomde.ch", "BlueZoomD.ch" },
            { "HelvetiaOneTV.ch", "Helvetiaone.ch" },
            // german
            { "Pro7Maxx.de", "ProSiebenMaxx.de" },
            // french
            { "TV5MondeFBS.fr", "TV5MondeFranceBelgiumSwitzerlandMonaco.fr" },
            { "ArteFr.fr", "arte.fr" },
            { "M6Suisse.fr", "M6Switzerland.ch" },
            // italian
            { "RTL1025.it", "RTL1025TV.it" },
            { "DMAXItalia.it", "DMAX.it"},
            { "WarnerTVItaly.it", "WarnerTv.it" },
            // portuguese
            { "RTPi.pt", "RTPInternacional.pt" },
        };

        (DateTimeOffset, DateTimeOffset) HandleChunk_Teleboy(
            TeleboyEpgResponse epgIn,
            EpgBuilder epgOut)
        {
            var dtNow = DateTimeOffset.Now;
            var dtMin = dtNow;
            var dtMax = dtNow;

            if (!epgIn.Success || epgIn.Data == null) return (dtMin, dtMax);
            foreach (var itm in epgIn.Data.Items)
            {
                if (!itm.Begin.HasValue) continue;
                if (!itm.End.HasValue) continue;
                var station = itm.Station;
                if (station == null) continue;

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
                if (!_config.AppendOnlyMode)
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
                            Trace.TraceWarning($"Couldn't find channel \"{id}\", skipping");
                        }
                        continue;
                    }
                }


                if (itm.Begin < dtMin)
                {
                    dtMin = itm.Begin.Value;
                }
                if (itm.End > dtMax)
                {
                    dtMax = itm.End.Value;
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
                    stop = CommonConverters.ConvertDateTimeXmlTv(itm.End.Value),
                    length = new length
                    {
                        units = lengthUnits.seconds,
                        Value = (itm.End.Value - itm.Begin.Value).TotalSeconds.ToString()
                    },
                    desc = CommonConverters.ConvertSingleNullable(itm.ShortDescription, value => new desc
                    {
                        Value = value
                    })
                };
                epgOut.TryAddProgramme(itm.Begin.Value, prg);
            }

            return (dtMin, dtMax);
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

        public async Task FillEpg(EpgBuilder epgOut)
        {
            var referenceTime = DateTimeOffset.Now;
            var boundHigh = referenceTime.Add(_config.TimeSpanForward);
            var boundLow = referenceTime.Subtract(_config.TimeSpanBackwards);
            await GetTeleboyEpg(epgOut, boundLow, boundHigh);
        }

        public void Dispose()
        {
            _client.Dispose();
        }
    }
}
