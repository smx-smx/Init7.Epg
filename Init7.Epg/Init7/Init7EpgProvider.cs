using Init7.Epg.Schema;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Init7.Epg.Init7
{
    public class Init7EpgProvider : IEpgProvider, IDisposable
    {
        private readonly Init7EpgClient _client;

        public Init7EpgProvider()
        {
            _client = new Init7EpgClient();
        }

        void HandleChunk_Init7(EpgResultList epgIn, EpgBuilder epgOut)
        {
            foreach (var itm in epgIn.Items)
            {
                var chan_in = itm.Channel;

                var id = chan_in.GetChannelId();
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
                    }).ToArray(),
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
            var epgIn = await _client.GetEpg(limit: 2000);
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
