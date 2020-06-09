using Autofac;
using DataToWeb.Data;
using DataToWeb.Library;
using Newtonsoft.Json;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;

namespace DataToWeb.Tests
{
    public class Tests
    {
        private IConfig _ReaderConfig { get; set; }
        private IConfig _WriterConfig { get; set; }
        private IContainer _Container { get; set; }
        private ILifetimeScope _ContainerScope { get; set; }

        private List<Details> _SampleDetails { get; set; } = new List<Details>();

        [SetUp]
        public void Setup()
        {
            var Builder = new ContainerBuilder();
            Builder.RegisterType<CsvInputReader<Details>>().As<IDataReader<Details>>();
            Builder.RegisterType<JsonWriter<Details>>().As<IDataWriter<Details>>();
            Builder.RegisterType<Config>().As<IConfig>();
            _Container = Builder.Build();

            _ContainerScope = _Container.BeginLifetimeScope();
            _ReaderConfig = _ContainerScope.Resolve<IConfig>();
            _ReaderConfig.SetConfigItem("FileName", "sample.csv");
            _ReaderConfig.SetConfigItem("HasHeaderRecord", "false");

            _WriterConfig = _ContainerScope.Resolve<IConfig>();
            _WriterConfig.SetConfigItem("JsonFormatting", Formatting.Indented);
            _WriterConfig.SetConfigItem("RemoveQuotes", "true");
            _WriterConfig.SetConfigItem("RemoveArrayIfSingleRecord", "true");

            var details = new Details()
            {
                name = "Fred",
                address = new Address()
                {
                    line1 = "10 TEST STREET",
                    line2 = "TESTINGTON"
                }
            };

            _SampleDetails.Clear();
            _SampleDetails.Add(details);
        }

        [Test]
        public void CanSetAndConfigItemString()
        {
            _ReaderConfig.SetConfigItem("FileName", "sample.csv");
            Assert.AreEqual("sample.csv", _ReaderConfig.GetConfigString("FileName"));
        }

        [Test]
        public void CanReadObjectFromConfig()
        {
            _ReaderConfig.SetConfigItem("TestObject", new List<int>());
            Assert.AreEqual(typeof(List<int>), _ReaderConfig.GetConfigObject("TestObject").GetType());
            Assert.IsNotNull(_ReaderConfig.GetConfigObject("TestObject"));
        }

        [Test]
        public void CanReadSampleCsv()
        {
            IDataReader<Details> rdr = _ContainerScope
                                        .Resolve<IDataReader<Details>>
                                         (new TypedParameter(typeof(IConfig), _ReaderConfig));
            rdr.Read();
            var result = rdr.GetDataObjects();
            Assert.AreEqual(1, result.Count());
        }

        [Test]
        public void CanGenerateStandardJson()
        {
            _WriterConfig.SetConfigItem("RemoveQuotes", "false");
            _WriterConfig.SetConfigItem("RemoveArrayIfSingleRecord", "false");

            IDataWriter<Details> DataWriter = _ContainerScope
                                    .Resolve<IDataWriter<Details>>
                                        (new TypedParameter(typeof(IConfig), _WriterConfig));

            string OutputJson = DataWriter.OutputAsString(_SampleDetails);
            Assert.AreEqual("[\r\n  {\r\n    \"name\": \"Fred\",\r\n    \"address\": {\r\n      \"line1\": \"10 TEST STREET\",\r\n      \"line2\": \"TESTINGTON\"\r\n    }\r\n  }\r\n]"
                ,OutputJson);
        }

        [Test]
        public void CanGenerateNonStandardJson()
        {
            IDataWriter<Details> DataWriter = _ContainerScope
                        .Resolve<IDataWriter<Details>>
                            (new TypedParameter(typeof(IConfig), _WriterConfig));

            string OutputJson = DataWriter.OutputAsString(_SampleDetails);
            Assert.AreEqual("{\r\n  name: Fred,\r\n  address: {\r\n    line1: 10 TEST STREET,\r\n    line2: TESTINGTON\r\n  }\r\n}",
                OutputJson);
        }

        [Test]
        public void CanGenerateXml()
        {
            var Builder = new ContainerBuilder();
            Builder.RegisterType<XmlWriter<Details>>().As<IDataWriter<Details>>();
            Builder.RegisterType<Config>().As<IConfig>();
            _Container.Dispose();
            _Container = Builder.Build();
            _ContainerScope.Dispose();
            _ContainerScope = _Container.BeginLifetimeScope();


            IDataWriter<Details> DataWriter = _ContainerScope
                        .Resolve<IDataWriter<Details>>
                            (new TypedParameter(typeof(IConfig), _WriterConfig));

            string OutputXml = DataWriter.OutputAsString(_SampleDetails);
            Assert.AreEqual("<?xml version=\"1.0\" encoding=\"utf-16\"?>\r\n<ArrayOfDetails xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\">\r\n  <Details>\r\n    <name>Fred</name>\r\n    <address>\r\n      <line1>10 TEST STREET</line1>\r\n      <line2>TESTINGTON</line2>\r\n    </address>\r\n  </Details>\r\n</ArrayOfDetails>",
                 OutputXml);
        }
    }
}