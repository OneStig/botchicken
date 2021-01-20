using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json;

namespace botchicken
{
    public class ConfigLoader
    {
        private const string configFolder = "Resources";
        private const string configFile = "config.json";

        public static Config cfg;

        static ConfigLoader()
        {
            if (!Directory.Exists(configFolder))
            {
                Directory.CreateDirectory(configFolder);
            }

            if (!File.Exists(configFolder + "/" + configFile))
            {
                cfg = new Config();
                string json = JsonConvert.SerializeObject(cfg, Formatting.Indented);
                File.WriteAllText(configFolder + "/" + configFile, json);
            }
            else
            {
                string json = File.ReadAllText(configFolder + "/" + configFile);
                cfg = JsonConvert.DeserializeObject<Config>(json);
            }
        }
    }

    public struct Config
    {
        public string token;
        public string prefix;
        public string topgg;
        public string csgobackpack;
        public string steamapi;
        public string portid;
        public string portsecret;
        //public string buffsession;
    }
}
