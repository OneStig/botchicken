using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Net;
using System.Linq;
using Newtonsoft.Json;

namespace botchicken
{

    public class Steam
    {
        public double last_90d { get; set; }
        public double last_30d { get; set; }
        public double last_7d { get; set; }
        public double last_24h { get; set; }
    }

    public class Bitskins
    {
        public string price { get; set; }
        public string instant_sale_price { get; set; }
    }

    public class Csmoney
    {
        public string price { get; set; }
    }
    public class Csgotrader
    {
        public string price { get; set; }
        public Dictionary<string, string> doppler;
    }

    public class buffprice
    {
        public string price { get; set; }
    }

    public class buff
    {
        public buffprice starting_at;
        public buffprice highest_order;
    }

    public class traderitem
    {
        //public Steam steam { get; set; }
        public Bitskins bitskins { get; set; }
        //public string lootfarm { get; set; }
        //public string csgotm { get; set; }
        public Csmoney csmoney { get; set; }
        public Csgotrader csgotrader { get; set; }

        public buff buff163 { get; set; }

        public string icon_url { get; set; }
        public string name_color { get; set; }
        public string type { get; set; }
    }

    public class Quality
    {
        public string category { get; set; }
        public string internal_name { get; set; }
        public string localized_name { get; set; }
    }

    public class Rarity
    {
        public string category { get; set; }
        public string internal_name { get; set; }
        public string localized_name { get; set; }
    }

    public class Type
    {
        public string category { get; set; }
        public string internal_name { get; set; }
        public string localized_name { get; set; }
    }

    public class Tags
    {
        public Quality quality { get; set; }
        public Rarity rarity { get; set; }
        public Type type { get; set; }
    }

    public class Info
    {
        public Tags tags { get; set; }
    }

    public class GoodsInfo
    {
        public string icon_url { get; set; }
        public Info info { get; set; }
        public object item_id { get; set; }
        public string original_icon_url { get; set; }
        public string steam_price { get; set; }
        public string steam_price_cny { get; set; }
    }

    public class Item
    {
        //public int appid { get; set; }
        //public bool bookmarked { get; set; }
        //public string buy_max_price { get; set; }
        //public int buy_num { get; set; }
        //public object description { get; set; }
        //public string game { get; set; }
        //public GoodsInfo goods_info { get; set; }
        //public bool has_buff_price_history { get; set; }
        //public int id { get; set; }
        public string market_hash_name { get; set; }
        //public string market_min_price { get; set; }
        //public string name { get; set; }
        //public string quick_price { get; set; }
        public string sell_min_price { get; set; }
        public int sell_num { get; set; }
        //public string sell_reference_price { get; set; }
        //public string steam_market_url { get; set; }
        //public int transacted_num { get; set; }
    }

    public class Data
    {
        public List<Item> items { get; set; }
        //public int page_num { get; set; }
        //public int page_size { get; set; }
        //public int total_count { get; set; }
        //public int total_page { get; set; }
    }

    public class BackpackRoot
    {
        public bool success;
        public Dictionary<string, Backpack> items_list;
        //public object msg { get; set; }
    }

    public class Backpack
    {
        public string name;
        public string icon_url;
        public string rarity_color;
    }

    public class imgroot
    {
        public bool success;
        public List<imgitem> results;

    }

    public class AssetDescription
    {
        public string icon_url { get; set; }
        public string name_color { get; set; }
        public string type { get; set; }
    }

    public class imgitem
    {
        public string hash_name;
        public AssetDescription asset_description;
    }

    public class guildinv
    {
        public string[] rolenames;
        public int[] values;
    }

    public class PortItem
    {
        public string market_hash_name { get; set; }
        public string suggested_price { get; set; }
        public string instant_price { get; set; }
        //public double steam_price { get; set; }
        public string item_page { get; set; }
        public string market_page { get; set; }
        public string min_price { get; set; }
        public string max_price { get; set; }
        public string mean_price { get; set; }
        public int quantity { get; set; }
        //public int created_at { get; set; }
        //public int updated_at { get; set; }
    }

    public class PortRoot
    {
        public List<PortItem> Items { get; set; }
    }

    public class ResourceLoader
    {
        public const string configFolder = "Resources";
        public const string configFile = "skins.json";
        public const string skinsFile = "bitskins.json";
        public const string traderFile = "prices_v6.json";
        public const string accountFile = "accounts.json";

        public static BackpackRoot csgobackpack;

        public static Dictionary<string, traderitem> trader = new Dictionary<string, traderitem>();
        public static Dictionary<string, Item> buffdb = new Dictionary<string, Item>();
        public static Dictionary<ulong, guildinv> guildsettings = new Dictionary<ulong, guildinv>();
        public static Dictionary<string, PortItem> skinport = new Dictionary<string, PortItem>();

        public static string[] allhashnames;

        static ResourceLoader()
        {
            if (!Directory.Exists(configFolder))
            {
                Directory.CreateDirectory(configFolder);
            }

            if (!File.Exists(configFolder + "/" + traderFile))
            {
                return;
            }
            else
            {
                string json = File.ReadAllText(configFolder + "/" + traderFile);
                json = json.Replace("\"null\"", "null");
                trader = JsonConvert.DeserializeObject<Dictionary<string, traderitem>>(json);

                //allhashnames = trader.Keys.ToArray();
            }


            if (!File.Exists(configFolder + "/csgobackpackdel.json"))
            {
                try
                {
                    string htmlu = string.Empty;
                    HttpWebRequest request = (HttpWebRequest)WebRequest.Create("http://csgobackpack.net/api/GetItemsList/v2/");
                    request.AutomaticDecompression = DecompressionMethods.GZip;

                    using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                    using (Stream stream = response.GetResponseStream())
                    using (StreamReader reader = new StreamReader(stream))
                    {
                        htmlu = reader.ReadToEnd();
                    }
                    csgobackpack = JsonConvert.DeserializeObject<BackpackRoot>(htmlu);

                    File.WriteAllText(configFolder + "/csgobackpackdel.json", htmlu);
                }
                catch
                {
                    Console.WriteLine("Failed to fetch icons");
                }
            }
            else
            {
                string json = File.ReadAllText(configFolder + "/csgobackpackdel.json");
                csgobackpack = JsonConvert.DeserializeObject<BackpackRoot>(json);
            }

            allhashnames = csgobackpack.items_list.Keys.ToArray();

            if (!File.Exists(configFolder + "/guilds.json"))
            {
                SaveGuild();
            }
            else
            {
                string json = File.ReadAllText(configFolder + "/guilds.json");
                guildsettings = JsonConvert.DeserializeObject<Dictionary<ulong, guildinv>>(json);
            }

            List<PortItem> sp;

            if (!File.Exists(configFolder + "/skinportdel.json"))
            {
                string htmlu = string.Empty;
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create("https://api.skinport.com/v1/items?app_id=730");
                request.Headers["authorization"] = "Basic " + Base64Encode(ConfigLoader.cfg.portid + ":" + ConfigLoader.cfg.portsecret);
                request.AutomaticDecompression = DecompressionMethods.GZip;

                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                using (Stream stream = response.GetResponseStream())
                using (StreamReader reader = new StreamReader(stream))
                {
                    htmlu = reader.ReadToEnd();
                }

                sp = JsonConvert.DeserializeObject<List<PortItem>>(htmlu);
                File.WriteAllText(configFolder + "/skinportdel.json", htmlu);
            }
            else
            {
                string json = File.ReadAllText(configFolder + "/skinportdel.json");
                sp = JsonConvert.DeserializeObject<List<PortItem>>(json);
            }

            foreach (PortItem cur in sp)
            {
                skinport[cur.market_hash_name] = cur;
            }
        }

        public static void SaveGuild()
        {
            string json = JsonConvert.SerializeObject(guildsettings, Formatting.Indented);
            File.WriteAllText(configFolder + "/guilds.json", json);
        }

        public static string Base64Encode(string plainText)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return System.Convert.ToBase64String(plainTextBytes);
        }
    }
}