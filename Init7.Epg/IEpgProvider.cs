using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Init7.Epg
{
    public interface IEpgProvider
    {
        public Task Initialize();
        public Task FillEpg(EpgBuilder epgOut);
    }
}
