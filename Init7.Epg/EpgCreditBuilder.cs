using Init7.Epg.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Init7.Epg
{
    public class EpgCreditBuilder : XmlBuilder<credits>
    {
        private IList<actor> _actors;
        private IList<producer> _producers;
        private IList<director> _directors;

        public EpgCreditBuilder() : base(new credits())
        {
            _actors = new List<actor>();
            _producers = new List<producer>();
            _directors = new List<director>();
        }

        public void AddActor(actor actor)
        {
            _actors.Add(actor);
        }

        public void AddProducer(producer producer)
        {
            _producers.Add(producer);
        }

        public void AddDirector(director director)
        {
            _directors.Add(director);
        }

        protected override void FinishAppending()
        {
            _root.actor = _actors.ToArray();
            _root.producer = _producers.ToArray();
            _root.director = _directors.ToArray();
        }
    }
}
