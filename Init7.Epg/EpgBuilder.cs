using Init7.Epg.Schema;
using System.Diagnostics.CodeAnalysis;

namespace Init7.Epg
{
    public class EpgChannel(channel channel)
    {
        private readonly IDictionary<DateTimeOffset, programme> _programs = new SortedDictionary<DateTimeOffset, programme>();

        public channel Data { get; private set; } = channel;
        public ICollection<programme> Programs => _programs.Values;

        public bool TryAddProgramme(DateTimeOffset start, programme program)
        {
            var startUtc = start.ToUniversalTime();
            if (_programs.ContainsKey(startUtc)) return false;
            _programs.Add(start, program);
            return true;
        }
    }

    public class EpgBuilder : XmlBuilder<tv>
    {
        private readonly IDictionary<string, EpgChannel> _channels;

        public EpgBuilder() : base(new tv())
        {
            _root.generatorinfoname = GetType().Namespace;
            _root.generatorinfourl = "https://notyourbusiness.ch";
            // with case insensitive channel name lookup
            _channels = new Dictionary<string, EpgChannel>(StringComparer.InvariantCultureIgnoreCase);
        }

        public bool TryAddProgramme(DateTimeOffset start, programme prg)
        {
            ArgumentNullException.ThrowIfNull(prg);

            if (!_channels.TryGetValue(prg.channel, out var channel))
            {
                return false;
            }
            return channel.TryAddProgramme(start, prg);
        }

        public bool TryAddChannel(channel channel)
        {
            ArgumentNullException.ThrowIfNull(channel);
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
            _root.channel = [.. _channels.Values.Select(x => x.Data)];
            _root.programme = [.. _channels.Values.SelectMany(x => x.Programs)];
        }
    }
}
