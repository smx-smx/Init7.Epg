using System;
using System.Collections.Generic;
using System.ComponentModel.Design.Serialization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using System.Xml;

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

        public string BuildToString()
        {
            FinishAppending();

            var serializer = new XmlSerializer(_root.GetType());
            using var stringWriter = new StringWriter();
            using var xmlWriter = XmlWriter.Create(stringWriter);
            serializer.Serialize(xmlWriter, _root);
            return stringWriter.ToString();
        }

        public void BuildToFile(string filePath)
        {
            FinishAppending();

            var serializer = new XmlSerializer(_root.GetType());
            using var stream = new FileStream(
                filePath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.ReadWrite);
            stream.SetLength(0);

            using var xmlWriter = XmlWriter.Create(stream, new XmlWriterSettings
            {
                Indent = true
            });

            serializer.Serialize(xmlWriter, _root);
        }

        public T Build()
        {
            FinishAppending();
            return _root;
        }
    }
}
