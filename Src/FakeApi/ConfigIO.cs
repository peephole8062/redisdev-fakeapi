﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

namespace FakeApi
{
    internal static class ConfigIO
    {
        internal static Config _config;

        public static Config GetConfig(string configSource)
        {
            if(_config != null)
            {
                return _config;
            }

            _config = LoadConfig(configSource);
            MergeApis(_config, configSource);
            _config.Validate();
            return _config;
        }

        public static Config LoadConfig(string configSource)
        {
            if (!File.Exists(configSource))
            {
                throw new FileLoadException($"File {configSource} not found");
            }

            Config config = null;

            using (StreamReader file = File.OpenText(configSource))
            {
                var serializer = new JsonSerializer();
                try
                {
                    config = (Config)serializer.Deserialize(file, typeof(Config));
                }
                catch (JsonReaderException)
                {
                    throw;
                }

                if (config == null)
                {
                    throw new FileLoadException($"An error occured when deserialized file {configSource}");
                }
            }

            return config;
        }

        public static void WriteConfig(Config config, string configFilePath)
        {
            if(config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            if(File.Exists(configFilePath))
            {
                File.Delete(configFilePath);
            }

            File.WriteAllText(configFilePath, JsonConvert.SerializeObject(config));
        }

        public static void MergeApis(Config config, string configSource)
        {
            if (config.ApisDirectories == null)
            {
                return;
            }

            foreach (var directory in config.ApisDirectories)
            {
                if (!Directory.Exists(directory))
                {
                    throw new DirectoryNotFoundException($"Directory {directory} not found");
                }

                foreach (var file in Directory.GetFiles(directory).Except(new List<string> { configSource }))
                {
                    var apis = LoadConfig(file).Apis;
                    if(apis == null)
                    {
                        continue;
                    }
                    if(config.Apis == null)
                    {
                        config.Apis = new List<ApiConfig>();
                    }
                    config.Apis = new List<ApiConfig>(config.Apis.Union(apis));
                }
            }
        }
    }
}
