using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Net;
using System.Linq;
using Newtonsoft.Json;

namespace botchicken
{
    public class UserAccount
    {
        public ulong disc_id;
        public long steam_id;
        public string rank;

        public int uses;
        public int points;

        public int last_eval;

        public string currency;

        public UserAccount(ulong id)
        {
            disc_id = id;
            steam_id = 0;
            rank = "none";

            uses = 0;
            points = 0;
            last_eval = 0;

            currency = "USD";
        }
    }

    class AccountHandler
    {
        public static Dictionary<ulong, UserAccount> accounts = new Dictionary<ulong, UserAccount>();

        static AccountHandler()
        {
            if (!Directory.Exists(ResourceLoader.configFolder))
            {
                Directory.CreateDirectory(ResourceLoader.configFolder);
            }

            if (File.Exists(ResourceLoader.configFolder + "/" + ResourceLoader.accountFile))
            {
                string json = File.ReadAllText(ResourceLoader.configFolder + "/" + ResourceLoader.accountFile);
                accounts = JsonConvert.DeserializeObject<Dictionary<ulong, UserAccount>>(json);
            }

            if (File.Exists(ResourceLoader.configFolder + "/steamaccounts.json"))
            {
                Dictionary<ulong, long> steamlinks = new Dictionary<ulong, long>();
                string json = File.ReadAllText(ResourceLoader.configFolder + "/steamaccounts.json");
                steamlinks = JsonConvert.DeserializeObject<Dictionary<ulong, long>>(json);

                foreach (KeyValuePair<ulong, long> e in steamlinks)
                {
                    AccExists(e.Key);
                    accounts[e.Key].steam_id = e.Value;
                }
            }
        }

        public static void AccExists(ulong id)
        {
            if (!(accounts.ContainsKey(id) && accounts[id].disc_id > 0))
            {
                accounts[id] = new UserAccount(id);
            }
        }

        public static void SaveFile()
        {
            string json = JsonConvert.SerializeObject(accounts, Formatting.Indented);
            File.WriteAllText(ResourceLoader.configFolder + "/" + ResourceLoader.accountFile, json);
        }
    }
}
