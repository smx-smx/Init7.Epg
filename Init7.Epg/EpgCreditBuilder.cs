using Init7.Epg.Schema;

namespace Init7.Epg
{
    public class EpgCreditBuilder : XmlBuilder<credits>
    {
        private readonly List<actor> _actors;
        private readonly List<producer> _producers;
        private readonly List<director> _directors;

        public EpgCreditBuilder() : base(new credits())
        {
            _actors = [];
            _producers = [];
            _directors = [];
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
            _root.actor = [.. _actors];
            _root.producer = [.. _producers];
            _root.director = [.. _directors];
        }
    }
}
