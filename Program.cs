using Init7.Epg;
using Init7.Epg.Schema;
using m3uParser;
using m3uParser.Model;
using System.Data;

public class Program
{
    private static readonly string[] DISPLAY_LANGS = ["de", "fr", "it", "en"];

    void HandleChunk(EpgResultList_high epgIn, EpgBuilder epgOut)
    {
        foreach (var itm in epgIn.Items)
        {
            var chan_in = itm.Channel;

            var id = chan_in.GetChannelId();
            var chan_out = new channel
            {
                displayname = DISPLAY_LANGS.Select(lang => new displayname
                {
                    lang = lang,
                    Value = chan_in.CanonicalName
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
                subtitle = Converters.ConvertSingleNullable(itm.SubTitle, (subt) => new subtitle
                {
                    lang = itm.Country ?? null,
                    Value = subt
                }),
                desc = Converters.ConvertSingleNullable(itm.Description, (descr) => new desc
                {
                    lang = itm.Country ?? null,
                    Value = descr
                }),
                category = itm.Categories.Select(cat => new category
                {
                    lang = itm.Country ?? null,
                    Value = cat
                }).ToArray(),
                start = Converters.ConvertDateTime(itm.TimeSlot.LowerTimeIso),
                stop = Converters.ConvertDateTime(itm.TimeSlot.UpperTimeIso),
                length = new length
                {
                    units = lengthUnits.seconds,
                    Value = (itm.TimeSlot.UpperTimeIso - itm.TimeSlot.LowerTimeIso).TotalSeconds.ToString()
                },
                credits = Converters.ConvertCredits(itm.Credits),
                icon = itm.Icons.Select(ico => new icon
                {
                    src = ico
                }).ToArray(),
                country = itm.Country?.Split(',')
                    ?.Select(itm => new country
                    {
                        Value = itm.Trim()
                    })?.ToArray() ?? null,
                episodenum = Converters.ConvertSingleNullable(itm.EpisodeNumber, (ep) => new episodenum
                {
                    Value = ep,
                    system = itm.EpisodeNumberingSystem
                }),
                starrating = Converters.ConvertSingleNullable(itm.StarRating, (sr) => new starrating
                {
                    value = sr
                })
            };
            epgOut.AddProgramme(prg);
        }
    }

    async Task Run(string outFilePath)
    {
        var client = new EpgClient();
        var epgIn = await client.GetEpg(limit: 2000);

        var epgOut = new EpgBuilder();
        HandleChunk(epgIn, epgOut);
        while (epgIn.NextUri != null)
        {
            Console.WriteLine(epgIn.NextUri);
            epgIn = await client.GetNext(epgIn);
            HandleChunk(epgIn, epgOut);
        }


        epgOut.BuildToFile(outFilePath);
    }

    public static async Task Main(string[] args)
    {
        var outFilePath = args.ElementAtOrDefault(0) ?? "output.xml";
        await new Program().Run(outFilePath);
    }
}

