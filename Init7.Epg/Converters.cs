using Init7.Epg.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Init7.Epg
{
    public class Converters
    {
        public static string ConvertDateTime(DateTime dt)
        {
            return dt.ToString("yyyyMMddHHmmss zzz").Replace(":", "");
        }

        public static credits ConvertCredits(ICollection<EpgResultCredit> credits)
        {
            var ecb = new EpgCreditBuilder();
            foreach (var itm in credits)
            {
                switch (itm.Position)
                {
                    case "actor":
                        ecb.AddActor(new actor
                        {
                            Text = [itm.Name]
                        });
                        break;
                    case "producer":
                        ecb.AddProducer(new producer
                        {
                            Text = [itm.Name]
                        });
                        break;
                    case "director":
                        ecb.AddDirector(new director
                        {
                            Text = [itm.Name]
                        });
                        break;
                }
            }
            return ecb.Build();
        }

        public static TOut[]? ConvertSingleNullable<TIn, TOut>(TIn? value, Func<TIn, TOut> converter)
        {
            if (value == null) return null;
            var res = converter(value);
            return [res];
        }

    }
}
