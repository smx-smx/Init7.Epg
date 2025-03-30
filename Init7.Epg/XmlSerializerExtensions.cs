using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace Init7.Epg
{
    public static class XmlSerializerExtensions
    {
        [UnconditionalSuppressMessage(
            "Trimming",
            "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code",
            Justification = "using AOT-generated Serializers")]
        public static void SerializeChecked(
            this XmlSerializer @this,
            XmlWriter xmlWriter,
            object? o)
        {
            @this.Serialize(xmlWriter, o);
        }
    }
}
