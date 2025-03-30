using System;
using System.Collections.Generic;
using System.ComponentModel.Design.Serialization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using System.Xml;
using System.Diagnostics.CodeAnalysis;
#if TARGET_AOT
using Microsoft.Xml.Serialization.GeneratedAssembly;
#endif

namespace Init7.Epg
{
    public abstract class XmlBuilder<T> where T : notnull
    {
        protected readonly T _root;

#if TARGET_AOT
        private static readonly XmlSerializerContract _serializers = new();
#endif

        public XmlBuilder(T root)
        {
            _root = root;
        }

        protected abstract void FinishAppending();

        private static XmlSerializer GetSerializer(object obj)
        {
            /**
             * in order to build AOT code dealing with XML, we must AOT-generate the XML (de)serializers.
             * to do this, Microsoft.XmlSerializer.Generator needs a .dll that it can load and reflect on.
             * this means we need to build 2 versions of the code, in order to break the cycle:
             * - the AnyCPU (host) version will use the regular JIT-based serializer.
             * - the AOT version will use the generated serializers.
             **/
#if TARGET_AOT
            return _serializers.GetSerializer(obj.GetType());
#else
            return new XmlSerializer(obj.GetType());
#endif
        }

        public string BuildToString()
        {
            FinishAppending();

            var serializer = GetSerializer(_root);
            using var stringWriter = new StringWriter();
            using var xmlWriter = XmlWriter.Create(stringWriter);
            serializer.Serialize(xmlWriter, _root);
            return stringWriter.ToString();
        }
        public void BuildToStream(Stream stream)
        {
            FinishAppending();

            var serializer = GetSerializer(_root);

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
