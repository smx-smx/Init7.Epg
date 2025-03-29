using System;
using System.Collections.Generic;
using System.ComponentModel.Design.Serialization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using System.Xml;
using System.Diagnostics.CodeAnalysis;
using Init7.Epg.Schema;

namespace Init7.Epg
{
    public abstract class XmlBuilder<T> where T : notnull
    {
        protected readonly T _root;

        public XmlBuilder(T root)
        {
            _root = root;
        }

        protected abstract void FinishAppending();

        [RequiresUnreferencedCode("XmlSerializer")]
        public string BuildToString()
        {
            FinishAppending();

            var serializer = new XmlSerializer(_root.GetType());
            using var stringWriter = new StringWriter();
            using var xmlWriter = XmlWriter.Create(stringWriter);
            serializer.Serialize(xmlWriter, _root);
            return stringWriter.ToString();
        }

        [RequiresUnreferencedCode("XmlSerializer")]
        public void BuildToStream(Stream stream)
        {
            FinishAppending();

            var serializer = new XmlSerializer(_root.GetType());

            using var xmlWriter = XmlWriter.Create(stream, new XmlWriterSettings
            {
                Indent = true
            });

            serializer.Serialize(xmlWriter, _root);
        }

        [RequiresUnreferencedCode("XmlSerializer")]
        public void BuildToFile(string filePath)
        {
            FinishAppending();

            using var stream = new FileStream(
                filePath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.ReadWrite);
            stream.SetLength(0);

            BuildToStream(stream);
        }

        public T Build()
        {
            FinishAppending();
            return _root;
        }
    }
}
