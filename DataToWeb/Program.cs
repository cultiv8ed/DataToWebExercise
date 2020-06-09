using Autofac;
using DataToWeb.Data;
using DataToWeb.Library;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using NLog;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;

namespace DataToWeb
{
    class Program
    {
        private static IConfig _ReaderConfig { get; set; }
        private static IConfig _WriterConfig { get; set; }
        private static IContainer _Container { get; set;  }
        private static ILifetimeScope _ContainerScope { get; set;}
        private static IEnumerable<IConfigurationSection> ConfigOptions { get; set; }
        private static string _OutputType { get; set; }
        private static string _InputType { get; set; }
        private static Logger _Logger {get; set; } = LogManager.GetCurrentClassLogger();
        static void Main(string[] args)
        {
            _Logger.Info("App Starting");
            InitAppConfig();
            InitAutofac();
            using (_ContainerScope = _Container.BeginLifetimeScope())
            {
                InitialiseConfig(args);
                List<Details> result = ReadDataFromSource();
                string OutputJson = GenerateOutputFromWriter(result);
                _Logger.Info("Output Created\r\n");
                _Logger.Info(OutputJson);
                _Logger.Info("\r\nPress a key to exit");
                Console.ReadKey();
            }
        }

        private static void InitAppConfig()
        {
            try
            {
                IConfigurationBuilder ConfigBuilder = new ConfigurationBuilder();
                ConfigBuilder.AddJsonFile("appconfig.json");
                IConfigurationRoot Configuration = ConfigBuilder.Build();
                ConfigOptions = Configuration.GetSection("AppSettings").GetChildren();
                _OutputType = ConfigOptions.Where(c => c.Key == "OutputType")
                                    .Select(c => c.Value)
                                    .FirstOrDefault();
                _InputType = ConfigOptions.Where(c => c.Key == "InputType")
                                    .Select(c => c.Value)
                                    .FirstOrDefault();
            }
            catch (Exception ex)
            {
                _Logger.Error($"Error reading from appconfig: {ex.Message}");
                throw;
            }
        }

        private static string GenerateOutputFromWriter(List<Details> result)
        {
            try
            {
                IDataWriter<Details> DataWriter = _ContainerScope
                                            .Resolve<IDataWriter<Details>>
                                                (new TypedParameter(typeof(IConfig), _WriterConfig));

                string OutputJson = DataWriter.OutputAsString(result);
                DataWriter.OutputAsFile(result);
                return OutputJson;
            }
            catch (Exception ex)
            {
                _Logger.Error($"Error generating output: {ex.Message}");
                throw;
            }
        }

        private static List<Details> ReadDataFromSource()
        {
            try
            {
                IDataReader<Details> DataReader = _ContainerScope
                                            .Resolve<IDataReader<Details>>
                                                (new TypedParameter(typeof(IConfig), _ReaderConfig));
                DataReader.Read();
                List<Details> result = DataReader.GetDataObjects().ToList();
                return result;
            }
            catch (Exception ex)
            {
                _Logger.Error($"Error generating output: {ex.Message}");
                throw;
            }
        }

        private static void InitialiseConfig(string[] args)
        {
            try
            {
                _ReaderConfig = _ContainerScope.Resolve<IConfig>();
                _ReaderConfig.SetConfigItem("FileName", ConfigOptions
                                                .Where(c => c.Key == "InputFileName")
                                                .Select(c => c.Value)
                                                .FirstOrDefault());
                _ReaderConfig.SetConfigItem("HasHeaderRecord", ConfigOptions
                                                .Where(c => c.Key == "CsvHasHeaderRecord")
                                                .Select(c => c.Value)
                                                .FirstOrDefault());

                _WriterConfig = _ContainerScope.Resolve<IConfig>();
                _WriterConfig.SetConfigItem("JsonFormatting", Formatting.Indented);
                _WriterConfig.SetConfigItem("RemoveQuotes", ConfigOptions
                                                .Where(c => c.Key == "JsonRemoveQuotes")
                                                .Select(c => c.Value)
                                                .FirstOrDefault());
                _WriterConfig.SetConfigItem("RemoveArrayIfSingleRecord", ConfigOptions
                                                .Where(c => c.Key == "JsonRemoveArrayIfSingleRecord")
                                                .Select(c => c.Value)
                                                .FirstOrDefault());
                _WriterConfig.SetConfigItem("FileName", ConfigOptions
                                .Where(c => c.Key == "OutputFileName")
                                .Select(c => c.Value)
                                .FirstOrDefault());
            }
            catch (Exception ex)
            {
                _Logger.Error($"Error initialising config objects: {ex.Message}");
                throw;
            }
        }

        private static void InitAutofac()
        {
            try
            {
                var Builder = new ContainerBuilder();
                Builder.RegisterType<Config>().As<IConfig>();

                switch (_OutputType)
                {
                    case "Xml":
                        Builder.RegisterType<XmlWriter<Details>>().As<IDataWriter<Details>>();
                        break;
                    case "Json":
                        Builder.RegisterType<JsonWriter<Details>>().As<IDataWriter<Details>>();
                        break;
                    default:
                        break;
                }

                switch (_InputType)
                {
                    case "Csv":
                        Builder.RegisterType<CsvInputReader<Details>>().As<IDataReader<Details>>();
                        break;
                    default:
                        break;
                }
                _Container = Builder.Build();
            }
            catch (Exception ex)
            {
                _Logger.Error($"Error initialising Autofac: {ex.Message}");
                throw;
            }
        }
    }
}
