using Init7.Epg.Schema;
using Init7.Epg.Swisscom.Schema;
using System.Data;
using System.Text;

namespace Init7.Epg.Swisscom
{
    public class SwisscomEpgConfig
    {
        public TimeSpan TimeSpanBackwards { get; set; } = TimeSpan.FromHours(6);
        public TimeSpan TimeSpanForward { get; set; } = TimeSpan.FromDays(2);
        public Dictionary<string, string>? ChannelMappings { get; set; }
        public bool StandaloneMode { get; set; } = false;
        public bool OnlyMapped { get; set; } = false;
        public SwisscomEpgLanguage Language { get; set; } = SwisscomEpgLanguage.English;
    }

    public class SwisscomEpgProvider : IEpgProvider, IDisposable
    {
        private static readonly Dictionary<string, string> _swisscomToInit7 = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase)
        {
        };

        private readonly SwisscomEpgClient _client;
        private readonly SwisscomEpgConfig _config;
        private readonly Dictionary<string, Channel> _channels;
        private readonly HashSet<string> _channelWarnings = new HashSet<string>();
        private readonly Dictionary<string, DescriptionInfo> _genres = new Dictionary<string, DescriptionInfo>(StringComparer.InvariantCulture);
        private readonly Dictionary<string, SeriesResponse> _seriesInfo = new Dictionary<string, SeriesResponse>(StringComparer.InvariantCulture);

        public void Dispose()
        {
            _client.Dispose();
        }

        private const int CHANNELS_PER_REQUEST = 20;
        private const string FORMAT_VERSION = "7";

        private const string SIZE_SMALL = "45";
        private const string SIZE_MEDIUM = "920";
        private const string SIZE_LARGE = "1920";

        private bool _formatCheck = false;

        private const string IMAGES_URL = "https://services.sg101.prd.sctv.ch/content/images";

        private static string BuildImageUri(string contentPath, imageSize size)
        {
            var sizeSpec = size switch
            {
                imageSize.Item1 => SIZE_SMALL,
                imageSize.Item2 => SIZE_MEDIUM,
                imageSize.Item3 => SIZE_LARGE,
                _ => SIZE_LARGE
            };
            return $"{IMAGES_URL}/{contentPath}_w{sizeSpec}.webp";
        }

        private static string? GetAudio(TvBroadcast node)
        {
            if (node.TechnicalAttributes?.DolbySurround ?? false)
            {
                return "surround";
            }
            if (node.TechnicalAttributes?.Stereo ?? false)
            {
                return "stereo";
            }
            return null;
        }

        private static credits GetCredits(TvBroadcast node)
        {
            var ecb = new EpgCreditBuilder();

            foreach (var dude in node.GetRelations<UniversalParticipant>())
            {
                var target = dude.TargetNode;
                var descr = target.Content!.Description!;

                switch (dude.Relation.Role)
                {
                    case "Director":
                        ecb.AddDirector(new director
                        {
                            Text = [descr.Fullname]
                        });
                        break;
                    case "Actor":
                        ecb.AddActor(new actor
                        {
                            Text = [descr.Fullname]
                        });
                        break;
                    case "Guest":
                        ecb.AddGuest(new guest
                        {
                            Text = [descr.Fullname]
                        });
                        break;
                }
            }

            return ecb.Build();

        }

        private static IEnumerable<image> GetImages(TvBroadcast node, imageType type)
        {
            var images = from n in node.Nodes?.GetNodes<Image>() ?? Enumerable.Empty<Image>()
                         where n.Role is not null
                            && n.ContentPath is not null
                         select n;

            if (type == imageType.poster)
            {
                return
                    from n in images
                    where n.Shape == "BannerL1"
                       || n.Shape == "BannerL2"
                    select new image
                    {
                        type = imageType.poster,
                        sizeSpecified = true,
                        size = imageSize.Item3,
                        Value = BuildImageUri(n.ContentPath!, imageSize.Item3)
                    };
            }

            if (type == imageType.backdrop)
            {
                return
                    from n in images
                    where n.Shape == "Backdrop"
                       || n.Shape == "Iconic"
                       || n.Shape == "KeyArt"
                    select new image
                    {
                        type = imageType.backdrop,
                        sizeSpecified = true,
                        size = imageSize.Item3,
                        Value = BuildImageUri(n.ContentPath!, imageSize.Item3)
                    };
            }

            if (type == imageType.character)
            {
                return
                   from n in images
                   where n.Shape == "Participant"
                   select new image
                   {
                       type = imageType.backdrop,
                       sizeSpecified = true,
                       size = imageSize.Item3,
                       Value = BuildImageUri(n.ContentPath!, imageSize.Item3)
                   };
            }

            // $TODO: fallback?
            return Enumerable.Empty<image>();
        }

        async Task HandleChunk(TvResponse resp, EpgBuilder epgOut)
        {
            if (!_formatCheck)
            {
                if (resp.Status.Version != FORMAT_VERSION)
                {
                    Console.Error.WriteLine($"Warning: version {resp.Status.Version} > {FORMAT_VERSION}");
                }
                _formatCheck = true;
            }

            if (resp.Status.Status != "OK")
            {
                throw new InvalidDataException($"Status \"{resp.Status.Status}\" != \"OK\"");
            }

            foreach (var item in resp.Nodes.GetNodes<TvChannel>().Where(x => x.Identifier != null))
            {
                var origChannelId = item.Identifier!;
                var channelId = origChannelId;
                var mappingAvailable = false;

                if (!_config.StandaloneMode && !_swisscomToInit7.TryGetValue(origChannelId, out channelId))
                {
                    if (!(mappingAvailable = _channelWarnings.Contains(origChannelId)))
                    {
                        _channelWarnings.Add(origChannelId);
                        Console.Error.WriteLine($"Couldn't find channel \"{origChannelId}\", skipping");
                    }
                    continue;
                }
                if (!_channels.TryGetValue(origChannelId, out var chanData))
                {
                    Console.Error.WriteLine($"Channel data not found for {origChannelId}, skipping");
                    continue;
                }

                var channelName = item.Content?.Description?.Title;
                var channelLang = item.Content?.Description?.Language;

                var chan_out = new channel
                {
                    displayname = CommonConverters.ConvertSingleNullable(chanData.Title, v => new displayname
                    {
                        Value = v,
                    }),
                    id = channelId,
                };

                if (epgOut.TryGetChannel(channelId, out var existing))
                {
                    chan_out.id = existing.id;
                } else if (mappingAvailable)
                {
                    Console.Error.WriteLine($"incorrect/expired mapping for {item.Identifier}");
                }

                if (_config.StandaloneMode)
                {
                    epgOut.TryAddChannel(chan_out);
                }


                foreach (TvBroadcast bcast in item.Content?.Nodes?.GetNodes<TvBroadcast>() ?? Enumerable.Empty<TvBroadcast>())
                {
                    var content = bcast.Content!;
                    var availability = bcast.Availabilities?.FirstOrDefault();
                    if (availability == null
                        || availability.AvailabilityStart == null) continue;

                    var seriesId = bcast.Content?.Series?.GlobalSeriesIdentifier ?? bcast.Series?.GlobalSeriesIdentifier;
                    SeriesResponse? seriesInfo = null;
                    if (seriesId != null && !_seriesInfo.TryGetValue(seriesId, out seriesInfo))
                    {
                        seriesInfo = await _client.GetSeriesInfo(seriesId);
                        _seriesInfo[seriesId] = seriesInfo;
                    }

                    var prg = new programme
                    {
                        title = CommonConverters.ConvertSingleNullable(
                                            content.Description,
                                            value => new title
                                            {
                                                Value = value.Title,
                                                // "de", "en", etc..
                                                lang = value.Language,
                                            }
                                        ),
                        subtitle = CommonConverters.ConvertSingleNullable(
                                            content.Description,
                                            value => new subtitle
                                            {
                                                Value = value.Subtitle,
                                                lang = value.Language
                                            }
                                        ),
                        length = CommonConverters.ConvertNullable(
                            content.Description?.Duration,
                            value => new length
                            {
                                units = lengthUnits.seconds,
                                Value = value!.Value.TotalSeconds.ToString()
                            }
                        ),
                        country = CommonConverters.ConvertSingleNullable(
                            content.Description?.Country,
                            value => new country
                            {
                                Value = value
                            }
                        ),
                        language = CommonConverters.ConvertNullable(
                            content.Description?.Language,
                            value => new language
                            {
                                Value = value
                            }
                        ),
                        date = CommonConverters.ConvertNullable(
                            content.Description?.ReleaseDate,
                            value => CommonConverters.ConvertDateTimeXmlTv(value!.Value)
                        ),
                        channel = channelId,
                        desc = CommonConverters.ConvertSingleNullable(
                            content.Description?.Summary, value => new desc
                            {
                                Value = value
                            }
                        ),
                        image = [
                            ..GetImages(bcast, imageType.poster),
                            ..GetImages(bcast, imageType.backdrop)
                        ],
                        rating = CommonConverters.ConvertSingleNullable(
                            content?.Description?.AgeRestrictionRating != null
                            || content?.Description?.AgeRestrictionSystem != null,
                            value => new rating
                            {
                                value = content?.Description?.AgeRestrictionRating,
                                system = content?.Description?.AgeRestrictionSystem
                            }
                        ),

                        start = CommonConverters.ConvertDateTimeXmlTv(availability.AvailabilityStart.Value),
                        stop = CommonConverters.ConvertNullable(
                            availability.AvailabilityEnd,
                            value => CommonConverters.ConvertDateTimeXmlTv(value!.Value)),
                        category = bcast.GetRelations<TvGenre>()?.Where(x => x.Relation.TargetIdentifier != null)
                            .Select(x =>
                            {
                                if (_genres.TryGetValue(x.Relation.TargetIdentifier!, out var genre)) return genre;
                                return null;
                            }).Where(x => x != null && x.Title != null && x.Language != null).Select(x => new category
                            {
                                Value = x!.Title!,
                                lang = x!.Language!
                            }).ToArray(),
                        episodenum = CommonConverters.ConvertSingleNullable(
                            bcast.Series,
                            value =>
                            {
                                var seasonInfo = seriesInfo
                                    ?.Results
                                    ?.FirstOrDefault(series => series.Identifier == value.GlobalSeriesIdentifier)
                                    ?.Children
                                    ?.FirstOrDefault(x => x?.Series?.Season == value.Season);

                                /*
                                var episodeInfo = seasonInfo
                                    ?.Children
                                    ?.FirstOrDefault(episode => episode switch
                                    {
                                        VodEpisodeNode vodEpisode => vodEpisode?.Series?.Episode == value.Episode,
                                        TvEpisodeNode tvEpisode => tvEpisode?.Series?.Episode == value.Episode,
                                        _ => false
                                    });
                                */

                                return new episodenum
                                {
                                    // <series>.<episode>[/totalEpisodes].<part>/<totalParts>
                                    Value = ""
                                    + (value.Season ?? 0)
                                    + "." + (value.Episode ?? 0) + "/" + (seasonInfo?.Series?.PartsCount ?? 1)
                                    + "." + (value.Part ?? 0) + "/" + (value.PartsCount ?? 1)
                                };
                            }
                        ),
                        credits = GetCredits(bcast),
                        audio = CommonConverters.ConvertNullable(
                            GetAudio(bcast),
                            value => new audio
                            {
                                present = "yes",
                                stereo = value
                            }
                        )
                    };
                    epgOut.TryAddProgramme(availability.AvailabilityStart.Value, prg);

                }
            }
        }

        async Task GetSwisscomEpg(EpgBuilder epgOut, DateTimeOffset from, DateTimeOffset to)
        {
            IEnumerable<string> population = _channels.Keys;
            if (_config.OnlyMapped && _config.ChannelMappings != null)
            {
                population = population.Intersect(_config.ChannelMappings.Keys);
            }

            foreach (var idChunk in population.Chunk(CHANNELS_PER_REQUEST))
            {
                var epgIn = await _client.GetEpg(from, to, idChunk);
                await HandleChunk(epgIn, epgOut);

                // save some ram by cleaning up the seasons info cache between channel ID chunks
                _seriesInfo.Clear();
            }
        }

        public async Task FillEpg(EpgBuilder epgOut)
        {
            var referenceTime = DateTimeOffset.Now;
            var boundHigh = referenceTime.Add(_config.TimeSpanForward);
            var boundLow = referenceTime.Subtract(_config.TimeSpanBackwards);
            await GetSwisscomEpg(epgOut, boundLow, boundHigh);
        }

        private async Task DumpChannelListAsync(string filePath)
        {
            using var fh = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.Read);
            fh.SetLength(0);

            using var writer = new StreamWriter(fh, new UTF8Encoding(false));

            foreach (var ch in _channels.Values.OrderBy(x => Convert.ToInt32(x.Identifier)))
            {
                await writer.WriteLineAsync($"{ch.Identifier}: {ch.Title}");
            }
        }

        public async Task Initialize()
        {
            _channels.Clear();

            var newChannels = await _client.GetChannels();
            foreach (var channel in newChannels)
            {
                _channels[channel.Identifier] = channel;
            }
            await DumpChannelListAsync("channels_swisscom.txt");

            var newGenres = await _client.GetGenres();
            if (newGenres != null)
            {
                _genres.Clear();
                foreach (var genre in newGenres)
                {
                    _genres[genre.Key] = genre.Value;
                }
            }
        }

        public SwisscomEpgClient Client => _client;

        public SwisscomEpgProvider(SwisscomEpgConfig config)
        {
            _config = config;
            _client = new SwisscomEpgClient();
            _channels = new Dictionary<string, Channel>();
            if (config.ChannelMappings != null)
            {
                _swisscomToInit7.Clear();
                foreach (var mapping in config.ChannelMappings)
                {
                    _swisscomToInit7.Add(mapping.Key, mapping.Value);
                }
            }
        }
    }
}
