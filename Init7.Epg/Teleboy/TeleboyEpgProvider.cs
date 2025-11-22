using Init7.Epg.Schema;
using System.Globalization;

namespace Init7.Epg.Teleboy
{
    public class TeleboyEpgProviderConfig
    {
        public TimeSpan TimeSpanBackwards { get; set; } = TimeSpan.FromHours(6);
        public TimeSpan TimeSpanForward { get; set; } = TimeSpan.FromDays(2);
        /// <summary>
        /// Only add data for existing channels (from Init7) in the EPG map
        /// </summary>
        public bool AppendOnlyMode { get; set; }
    }

    public class TeleboyEpgProvider(TeleboyEpgProviderConfig config) : IEpgProvider, IDisposable
    {
        private readonly TeleboyEpgClient _client = new();
        private Dictionary<int, GenreItem> _genreMap = [];
        private bool disposedValue;
        //private readonly HashSet<string> _channelWarnings = [];

        /// <summary>
        /// mapping for specific channel names that are called differently
        /// </summary>
        private static readonly Dictionary<string, string> _teleboyToInit7 = new(StringComparer.InvariantCultureIgnoreCase)
        {
            // swiss
            { "blueZoomde.ch", "BlueZoomD.ch" },
            { "HelvetiaOneTV.ch", "Helvetiaone.ch" },
            {"24H.es","24Horas.es"},
            {"3+.ch","3plus.ch"},
            {"4+.ch","4plus.ch"},
            {"5+.ch","5Plus.ch"},
            {"5STAR.en","5STAR.uk"},
            {"6+.ch","6Plus.ch"},
            {"6ter.fr","6terSwitzerland.ch"},
            {"7+/NickCH.ch","7PlusNickSchweiz.ch"},
            {"ARD-alpha.de","ARDalpha.de"},
            {"BBC1.uk","BBCOne.uk"},
            {"BBC2.uk","BBCTwo.uk"},
            {"BR.de","BRFernsehenSud.de"},
            {"BoingPlus.it","Boing.it"},
            {"CNBC.uk","CNBCEurope.uk"},
            {"CanalAlphaJura.ch","CanalAlphaJU.ch"},
            {"CanalAlphaNeuchatel.ch","CanalAlphaNE.ch"},
            {"Chan4.uk","Channel4.uk"},
            {"DWen.de","DWEnglish.de"},
            {"E4Extra.en","E4.uk"},
            {"EuNews-e.uk","EuronewsEnglish.fr"},
            {"EuNews-f.fr","EuronewsFrench.fr"},
            {"EuNews-i.fr","EuronewsItalian.fr"},
            {"EuNews.de","EuronewsGerman.fr"},
            {"EuSp.de","Eurosport1.de"},
            {"FashionTV.fr","FashionTVEurope.fr"},
            {"Fr2.fr","France2.fr"},
            {"Fr24.fr","France24French.fr"},
            {"Fr3.fr","France3.fr"},
            {"Fr4.fr","France4.fr"},
            {"Fr5.fr","France5.fr"},
            {"France24[en].uk","France24English.fr"},
            {"Kabel1Doku.de","kabeleinsDoku.de"},
            {"Kabel1ch.de","kabeleinsSchweiz.ch"},
            {"Kanal7Avrupa.tr","Kanal7.tr"},
            {"MDR.de","MDRFernsehenSachsen.de"},
            {"RTL2ch.de","RTLZwei.de"},
            {"RTLch.de","RTL.de"},
            {"S-RTLch.de","RTLSuper.de"},
            {"SRF2.ch","SRFzwei.ch"},
            {"SRFi.ch","SRFinfo.ch"},
            {"SWR-BW.de","SWRFernsehenBadenWurttemberg.de"},
            {"Sat.1Gold.de","SAT1Gold.de"},
            {"Sat.1ch.de","SAT1.de"},
            {"ServusTvAustria.at","ServusTV.at"},
            {"SkyNews.uk","SkyNewsInternational.uk"},
            {"Sport1Plus.ch","Sport1.de"},
            {"SportitaliaHD.it","Sportitalia.it"},
            {"TBasel.ch","Telebasel.ch"},
            {"TBärn.ch","TeleBarn.ch"},
            {"TLCGermany.de","TLC.de"},
            {"TV5.fr","TV5MondeFranceBelgiumSwitzerlandMonaco.fr"},
            {"TZüri.ch","TeleZuri.ch"},
            {"VOXch.de","VOX.de"},
            {"W9.fr","W9Switzerland.ch"},
            {"WDR.de","WDRFernsehenKoln.de"},
            {"WetterTV.ch","WettercomTV.de"},
            {"ZDF-Info.de","ZDFinfo.de"},
            {"n-tv.de","ntv.de"},
            {"rbbBerlin.de","rbbFernsehenBerlin.de"},
            {"auftanken.tv.ch","AuftankenTV.ch"},
            {"Pro7ch.de","ProSiebenSchweiz.ch"},
            {"RaiSport1.it","RaiSportHD.it"},
            {"RTP3.pt","RTPInternacional.pt"},
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

        private void HandleChunk_Teleboy(
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
                if (!itm.End.HasValue) continue;
                if (itm.Station == null) continue;

                var id = itm.Station.GetChannelId();
                var chan_out = new channel
                {
                    displayname = [.. Constants.DISPLAYLANGS.Select(lang => new displayname
                    {
                        lang = lang,
                        Value = id
                    })],
                    icon = TeleboyConverters.ConvertLogos(itm.Station?.Logos ?? new StationLogos()),
                    id = id,
                };

                if (!config.AppendOnlyMode)
                {
                    // if we're not in append only mode, we add any channel we see
                    epgOut.TryAddChannel(chan_out);
                }
                else
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
                    }
                    else
                    {
                        //if (!_channelWarnings.Add(id))
                        //{
                        //    Console.WriteLine($"Couldn't find channel \"{id}\", skipping");
                        //}
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
                programme prg = BuldProgramme(itm, chan_out);
                epgOut.TryAddProgramme(itm.Begin.Value, prg);
            }
        }

        private programme BuldProgramme(ProgramItem itm, channel chan_out)
        {
            return new programme
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
                start = CommonConverters.ConvertDateTimeXmlTv(itm.Begin!.Value),
                stop = CommonConverters.ConvertDateTimeXmlTv(itm.End!.Value),
                length = new length
                {
                    units = lengthUnits.seconds,
                    Value = (itm.End.Value - itm.Begin.Value).TotalSeconds.ToString(CultureInfo.InvariantCulture)
                },
                desc = CommonConverters.ConvertSingleNullable(itm.ShortDescription, value => new desc
                {
                    Value = value
                })
            };
        }

        async Task<TeleboyEpgResponse?> FetchTeleboyEpg(
            DateTimeOffset from, DateTimeOffset to,
            int offset, int limit,
            EpgBuilder epgOut
        )
        {
            var epgIn = await _client.GetEpg(from, to, offset: offset, limit).ConfigureAwait(false);
            if (epgIn == null || !epgIn.Success)
            {
                await Console.Error.WriteLineAsync("Failed to fetch Teleboy EPG").ConfigureAwait(false);
                return null;
            }

            HandleChunk_Teleboy(epgIn, epgOut);
            return epgIn;
        }

        async Task GetTeleboyEpg(EpgBuilder epgOut, DateTimeOffset from, DateTimeOffset to)
        {
            var offset = 0;
            const int limit = 2000;

            var epgIn = await FetchTeleboyEpg(from, to, offset, limit, epgOut).ConfigureAwait(false);
            while (epgIn != null
                && epgIn.Success
                && epgIn.Data != null
                && epgIn.Data.Items.Count > 0
                && offset < epgIn.Data.Total)
            {
                offset += epgIn.Data.Items.Count;
                epgIn = await FetchTeleboyEpg(from, to, offset, limit, epgOut).ConfigureAwait(false);
            }
        }

        public async Task Initialize()
        {
            TeleboyGenreApiResponse? _genres;
            _genres = await _client.GetGenres().ConfigureAwait(false);
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
            var boundHigh = referenceTime.Add(config.TimeSpanForward);
            var boundLow = referenceTime.Subtract(config.TimeSpanBackwards);
            await GetTeleboyEpg(epgOut, boundLow, boundHigh).ConfigureAwait(false);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _client.Dispose();
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
