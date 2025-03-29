using Init7.Epg.Schema;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace Init7.Epg
{
    public class EpgBuilder : XmlBuilder<tv>
    {
        private IDictionary<string, channel> _channels;
        private IList<programme> _programs;

        public EpgBuilder() : base(new tv())
        {
            _root.generatorinfoname = GetType().Namespace;
            _root.generatorinfourl = "https://github.com/smx-smx/Init7.Epg";

            _channels = new Dictionary<string, channel>();
            _programs = new List<programme>();
        }

        public void AddProgramme(programme prg)
        {
            _programs.Add(prg);
        }

        public bool TryAddChannel(channel channel)
        {
            if (_channels.ContainsKey(channel.id)) return false;
            _channels.Add(channel.id, channel);
            return true;
        }

        protected override void FinishAppending()
        {
            _root.channel = _channels.Values.ToArray();
            _root.programme = _programs.ToArray();
        }
    }
}
