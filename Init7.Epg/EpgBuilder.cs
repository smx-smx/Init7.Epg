using Init7.Epg.Schema;
using System.Diagnostics.CodeAnalysis;

namespace Init7.Epg
{
    internal record ProgramMatch(TimeSpan Distance, ProgramEntry Entry);
    internal record ProgramEntry(DateTimeOffset Start, DateTimeOffset? End, programme Program);

    internal class EpgChannel
    {
        private readonly IDictionary<DateTimeOffset, ProgramEntry> _programsByStart;

        public channel Data { get; private set; }
        public IEnumerable<programme> Programs => _programsByStart.Values.Select(e => e.Program);

        public EpgChannel(channel channel)
        {
            _programsByStart = new SortedDictionary<DateTimeOffset, ProgramEntry>();
            Data = channel;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="a">current value</param>
        /// <param name="b">incoming value</param>
        /// <returns></returns>
        private static T? SelectField<T>(T? a, T? b)
        {
            if (a == null) return b;
            if (b == null) return a;
            return a;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="a">current collection</param>
        /// <param name="b">incoming collection</param>
        /// <returns></returns>
        private static T[]? SelectCollection<T>(IEnumerable<T>? a, IEnumerable<T>? b)
        {
            if (a == null)
            {
                return (b == null) ? null : b.ToArray();
            }

            if (b == null)
            {
                return (a == null) ? null : a.ToArray();
            }
            return [.. (b.Count() > a.Count() ? b : a)];
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="a">current episode number</param>
        /// <param name="b">incoming episode number</param>
        /// <returns></returns>
        private static episodenum? SelectEpisodeNum(episodenum? a, episodenum? b)
        {
            if (a == null || b == null) return SelectField(a, b);
            // prefer the one having the total parts
            return b.Value.AsSpan().Count('/') > a.Value.AsSpan().Count('/')
                ? b : a;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="a">current credits</param>
        /// <param name="b">incoming credits</param>
        /// <returns></returns>
        private static credits? SelectCredits(credits? a, credits? b)
        {
            if (a == null || b == null) return SelectField(a, b);

            var creditsScore = (credits x) =>
            {
                return 0
                    + (x.actor?.Count() ?? 0)
                    + (x.adapter?.Count() ?? 0)
                    + (x.actor?.Count() ?? 0)
                    + (x.commentator?.Count() ?? 0)
                    + (x.composer?.Count() ?? 0)
                    + (x.director?.Count() ?? 0)
                    + (x.editor?.Count() ?? 0)
                    + (x.guest?.Count() ?? 0)
                    + (x.presenter?.Count() ?? 0)
                    + (x.producer?.Count() ?? 0)
                    + (x.writer?.Count() ?? 0);
            };
            return creditsScore(b) > creditsScore(a) ? b : a;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="a">current program</param>
        /// <param name="b">incoming program</param>
        /// <returns></returns>
        private programme MergeProgramme(programme a, programme b)
        {
            var c = new programme
            {
                title = SelectCollection(a.title, b.title),
                subtitle = SelectCollection(a.subtitle, b.subtitle),
                length = SelectField(a.length, b.length),
                audio = SelectField(a.audio, b.audio),
                category = SelectCollection(a.category, b.category),
                channel = SelectField(a.channel, b.channel),
                clumpidx = SelectField(a.clumpidx, b.clumpidx),
                country = SelectCollection(a.country, b.country),
                credits = SelectCredits(a.credits, b.credits),
                date = SelectField(a.date, b.date),
                desc = SelectCollection(a.desc, b.desc),
                episodenum = Utils.Pairs((a.episodenum ?? []).Concat(b.episodenum ?? [])).Select(x => SelectEpisodeNum(x.Item1, x.Item2)).ToArray(),
                icon = SelectCollection(a.icon, b.icon),
                image = SelectCollection(a.image, b.image),
                keyword = SelectCollection(a.keyword, b.keyword),
                language = SelectField(a.language, b.language),
                lastchance = SelectField(a.lastchance, b.lastchance),
                @new = SelectField(a.@new, b.@new),
                origlanguage = SelectField(a.origlanguage, b.origlanguage),
                pdcstart = SelectField(a.pdcstart, b.pdcstart),
                premiere = SelectField(a.premiere, b.premiere),
                previouslyshown = SelectField(a.previouslyshown, b.previouslyshown),
                rating = SelectCollection(a.rating, b.rating),
                review = SelectCollection(a.review, b.review),
                showview = SelectField(a.showview, b.showview),
                starrating = SelectCollection(a.starrating, b.starrating),
                start = SelectField(a.start, b.start),
                stop = SelectField(a.stop, b.stop),
                subtitles = SelectCollection(a.subtitles, b.subtitles),
                url = SelectCollection(a.url, b.url),
                video = SelectField(a.video, b.video),
                videoplus = SelectField(a.videoplus, b.videoplus),
                vpsstart = SelectField(a.vpsstart, b.vpsstart),
            };
            return c;

        }

        private IEnumerable<ProgramEntry> FindRange(DateTimeOffset start, DateTimeOffset end)
        {
            var keys = _programsByStart.Keys.ToList();
            if (keys.Count == 0)
            {
                yield break;
            }

            var current = start;
            while (current < end)
            {
                int index = keys.BinarySearch(current);

                if (index < 0)
                {
                    index = ~index; // first entry with Start >= start
                }

                if(index >= keys.Count)
                {
                    yield break;
                }

                var entry = _programsByStart[keys[index]];

                // program started at or during event
                if(entry.Start >= start && entry.Start < end)
                {
                    yield return entry;
                    current = entry.Start + TimeSpan.FromMinutes(1);
                    continue;
                }

                // program started before event but finishing inside
                if(entry.End >= start && entry.End < end)
                {
                    yield return entry;
                    current = entry.End.HasValue
                        ? entry.End.Value
                        : current + TimeSpan.FromMinutes(1);
                    continue;
                }

                // end of search
                break;
            }
        }

        private bool TryFindClosestEntry(DateTimeOffset start, DateTimeOffset? end, TimeSpan maxDelta,
            [MaybeNullWhen(false)]
            out ProgramMatch match)
        {
            match = null;

            if (maxDelta == TimeSpan.Zero)
            {
                if (_programsByStart.TryGetValue(start, out var existing) && existing.End == end)
                {
                    match = new ProgramMatch(TimeSpan.Zero, existing);
                    return true;
                } else
                {
                    return false;
                }
            }

            var keys = _programsByStart.Keys.ToList();
            if (keys.Count == 0)
            {
                return false;
            }

            // Binary search for insertion index of 'start'
            int index = keys.BinarySearch(start);

            if (index < 0)
            {
                index = ~index; // first entry with Start >= start
            }

            ProgramEntry? best = null;
            TimeSpan bestDistance = TimeSpan.MaxValue;

            // Check a small window of candidates around the index
            // (typical closest match is within ±1 items)
            foreach (var idx in (int[])[
                index,
                Math.Min(keys.Count, index + 1),
                Math.Max(0, index - 1)
            ])
            {
                var entry = _programsByStart[keys[idx]];

                var distStart = (entry.Start - start).Duration();
                var distEnd = TimeSpan.Zero;

                if (entry.End.HasValue && end.HasValue)
                {
                    distEnd = (entry.End.Value - end.Value).Duration();
                }

                if (distStart > maxDelta || distEnd > maxDelta)
                {
                    continue;
                }

                var dist = distStart + distEnd;
                if (dist <= bestDistance)
                {
                    best = entry;
                    bestDistance = dist;
                }
            }

            if (best != null)
            {
                match = new ProgramMatch(bestDistance, best);
                return true;
            }

            return false;
        }

        private void DeleteOverlappingPrograms(DateTimeOffset start, DateTimeOffset end, programme program)
        {
            foreach (var entry in FindRange(start, end).Where(x => x.Start != start && x.End != end && x.Program != program))
            {
                Console.WriteLine($"Delete overlap: {start},{end} --> {entry.Start},{entry.End}, {entry.Program.title.FirstOrDefault()?.Value}");
                _programsByStart.Remove(entry.Start);
            }
        }

        public (bool, string) TryAddProgramme(DateTimeOffset start, DateTimeOffset? end, programme program,
            TimeSpan? maxDelta,
            bool overwrite, bool merge, bool allowAdd)
        {
            var add = (DateTimeOffset start, DateTimeOffset? end, programme prg) =>
            {
                // we determined a program should be added. by this point, we shouldn't have any overlap
                // (the previous one should have been merged)
                if (end.HasValue)
                {
                    DeleteOverlappingPrograms(start, end.Value, program);
                }
                _programsByStart[start] = new ProgramEntry(start, end, prg);
            };

            if (overwrite)
            {
                add(start, end, program);
                return (true, string.Empty);
            }

            if (TryFindClosestEntry(start, end, maxDelta.HasValue ? maxDelta.Value : TimeSpan.Zero, out var closest))
            {
                if (!merge)
                {
                    return (false, $"[{Data.id}] Found program, but merge is disabled");
                }

                var merged = MergeProgramme(closest.Entry.Program, program);
                add(closest.Entry.Start, closest.Entry.End, merged);
                return (true, string.Empty);
            }
            
            if (allowAdd && end.HasValue)
            {
                var overlaps = FindRange(start, end.Value).ToList();
                if (!overlaps.Any())
                {
                    add(start, end, program);
                    return (true, string.Empty);
                } else
                {
                    return (false, $"[{Data.id}] Found {overlaps.Count} overlaps for {start},{end},{program.title?.FirstOrDefault()?.Value}: \n"
                        + string.Join('\n',overlaps.Select(x => $"{x.Start},{x.End},{x.Program.title?.FirstOrDefault()?.Value}").ToArray()));
                }
            }

            return (false, $"[{Data.id}] AllowAdd is disabled");
        }

        public void ClearEpg()
        {
            _programsByStart.Clear();
        }
    }

    public class EpgBuilder : XmlBuilder<tv>
    {
        private readonly IDictionary<string, EpgChannel> _channels;

        public EpgBuilder() : base(new tv())
        {
            _root.generatorinfoname = GetType().Namespace;
            _root.generatorinfourl = "https://github.com/smx-smx/Init7.Epg";
            // with case insensitive channel name lookup
            _channels = new Dictionary<string, EpgChannel>(StringComparer.InvariantCultureIgnoreCase);
        }

        public (bool,string) TryAddProgramme(
            DateTimeOffset start, DateTimeOffset? end, programme prg,
            bool overwrite = false,
            bool merge = true,
            TimeSpan? maxDelta = null,
            bool allowAdd = false)
        {
            if (!_channels.TryGetValue(prg.channel, out var channel))
            {
                return (false, $"Channel {prg.channel} not found");
            }
            return channel.TryAddProgramme(start, end, prg, maxDelta, overwrite, merge, allowAdd);
        }

        public bool ClearEpg(channel channel)
        {
            if (!_channels.TryGetValue(channel.id, out var channelEpg)) return false;
            channelEpg.ClearEpg();
            return true;
        }

        public bool TryAddChannel(channel channel)
        {
            if (_channels.ContainsKey(channel.id)) return false;
            _channels.Add(channel.id, new EpgChannel(channel));
            return true;
        }

        public bool TryGetChannel(string id, [MaybeNullWhen(false)] out channel channel)
        {
            channel = null!;
            if (!_channels.TryGetValue(id, out var channelEntry))
            {
                return false;
            }
            channel = channelEntry.Data;
            return true;
        }

        protected override void FinishAppending()
        {
            _root.channel = _channels.Values.Select(x => x.Data).ToArray();
            _root.programme = _channels.Values.SelectMany(x => x.Programs).ToArray();
        }
    }
}
