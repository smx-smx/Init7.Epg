using Init7.Epg.Schema;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Init7.Epg.Init7
{
    public class Init7EpgConfig
    {
        public int FetchLimit { get; set; } = 2000;
    }

    public class Init7EpgProvider : IEpgProvider, IDisposable
    {
        private readonly Init7EpgConfig _config;
        private readonly Init7EpgClient _client;
        private readonly HashSet<string> _channels;

        public Init7EpgProvider(Init7EpgConfig config)
        {
            _config = config;
            _client = new Init7EpgClient();
            _channels = new HashSet<string>();
        }

        private async Task DumpChannelListAsync(string filePath)
        {
            using var fh = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.Read);
            fh.SetLength(0);

            using var writer = new StreamWriter(fh, new UTF8Encoding(false));

            foreach (var ch in _channels.Order())
            {
                await writer.WriteLineAsync(ch);
            }
        }

        void HandleChunk_Init7(EpgResultList epgIn, EpgBuilder epgOut)
        {
            foreach (var itm in epgIn.Items)
            {
                var chan_in = itm.Channel;

                var id = chan_in.GetChannelId();
                _channels.Add(id);

                var chan_out = new channel
                {
                    displayname = Constants.DISPLAY_LANGS.Select(lang => new displayname
                    {
                        lang = lang,
                        Value = id
                    }).ToArray(),
                    icon = [new icon {
                        src = chan_in.LogoUri.AbsoluteUri
                    }],
                    id = id
                };

                epgOut.TryAddChannel(chan_out);

                var prg = new programme
                {
                    channel = itm.Channel.GetChannelId(),
                    title = [new title {
                        lang = itm.Country ?? null,
                        Value = itm.Title
                    }],
                    subtitle = CommonConverters.ConvertSingleNullable(itm.SubTitle, (subt) => new subtitle
                    {
                        lang = itm.Country ?? null,
                        Value = subt
                    }),
                    desc = CommonConverters.ConvertSingleNullable(itm.Description, (descr) => new desc
                    {
                        lang = itm.Country ?? null,
                        Value = descr
                    }),
                    category = itm.Categories.Select(cat => new category
                    {
                        lang = itm.Country ?? null,
                        Value = cat
                    }).ToArray(),
                    start = CommonConverters.ConvertDateTimeXmlTv(itm.TimeSlot.LowerTimeIso),
                    stop = CommonConverters.ConvertDateTimeXmlTv(itm.TimeSlot.UpperTimeIso),
                    length = new length
                    {
                        units = lengthUnits.seconds,
                        Value = (itm.TimeSlot.UpperTimeIso - itm.TimeSlot.LowerTimeIso).TotalSeconds.ToString()
                    },
                    credits = Init7Converters.ConvertCredits(itm.Credits),
                    icon = itm.Icons.Select(ico => new icon
                    {
                        src = ico
                    }).Take(1).ToArray(),
                    country = itm.Country?.Split(',')
                        ?.Select(itm => new country
                        {
                            Value = itm.Trim()
                        })?.ToArray() ?? null,
                    episodenum = CommonConverters.ConvertSingleNullable(itm.EpisodeNumber, (ep) => new episodenum
                    {
                        Value = ep,
                        system = itm.EpisodeNumberingSystem
                    }),
                    starrating = CommonConverters.ConvertSingleNullable(itm.StarRating, (sr) => new starrating
                    {
                        value = sr
                    })
                };
                epgOut.TryAddProgramme(itm.TimeSlot.LowerTimeIso, prg);
            }
        }

        public async Task FillEpg(EpgBuilder epgOut)
        {
            var epgIn = await _client.GetEpg(limit: _config.FetchLimit);
            if (epgIn == null)
            {
                return;
            }

            HandleChunk_Init7(epgIn, epgOut);
            while (epgIn.NextUri != null)
            {
                epgIn = await _client.GetNext(epgIn);
                if (epgIn == null) break;
                HandleChunk_Init7(epgIn, epgOut);
            }

            await DumpChannelListAsync("channels_init7.txt");
        }

        public Task Initialize()
        {
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _client.Dispose();
        }
    }
}
