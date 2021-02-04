using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Net;
using System.Linq;
using Newtonsoft.Json;

namespace botchicken
{
    // https://www.exchangerate-api.com/docs/c-sharp-currency-api

    public class ExchangeAPI
    {
        public string date;

        public Dictionary<string, double> rates;
    }

    class ExchangeRate
    {
        public static ExchangeAPI rateAPI = new ExchangeAPI();
        public static Dictionary<string, double> rates = new Dictionary<string, double>();

        public static Dictionary<string, string> prefsymbols = new Dictionary<string, string>()
        {
            { "ISK", "kr" },
            { "PHP", "₱" },
            { "DKK", "Kr." },
            { "HUF", "Ft" },
            { "CZK", "Kč" },
            { "RON", "L"},
            { "SEK", "kr" },
            { "IDR", "Rp" },
            { "INR", "₹" },
            { "BRL", "R$" },
            { "RUB", "₽" },
            { "HRK", "kn" },
            { "JPY", "¥" },
            { "THB", "฿" },
            { "CHF", "CHf" },
            { "PLN", "zł"},
            { "BGN", "Лв."},
            { "TRY", "₺" },
            { "CNY", "¥" },
            { "NOK", "kr" },
            { "ZAR", "R" },
            { "ILS", "₪" },
            { "GBP", "£" },
            { "KRW", "₩" },
            { "MYR", "RM" }
        };

        static ExchangeRate()
        {
            if (!Directory.Exists(ResourceLoader.configFolder))
            {
                Directory.CreateDirectory(ResourceLoader.configFolder);
            }

            if (!File.Exists(ResourceLoader.configFolder + "/exchangerates.json"))
            {
                RefreshRates();
            }
            else
            {
                string json = File.ReadAllText(ResourceLoader.configFolder + "/exchangerates.json");
                rateAPI = JsonConvert.DeserializeObject<ExchangeAPI>(json);
                rates = rateAPI.rates;
            }
        }

        public static string Convert(double usd, string key)
        {
            string pref = "$";

            usd /= rates["USD"];

            if (key == "EUR")
            {
                return "€" + Math.Round(usd, 2).ToString() + " EUR";
            }

            if (prefsymbols.ContainsKey(key))
            {
                pref = prefsymbols[key];
            }

            return pref + Math.Round(usd * rates[key], 2).ToString();
        }

        public static bool ValidCurr(string cur)
        {
            return rates.ContainsKey(cur.ToUpper()) || cur.ToUpper() == "EUR";
        }

        public static void RefreshRates()
        {
            try
            {
                string htmlu = string.Empty;
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create("https://api.exchangeratesapi.io/latest/");
                request.AutomaticDecompression = DecompressionMethods.GZip;

                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                using (Stream stream = response.GetResponseStream())
                using (StreamReader reader = new StreamReader(stream))
                {
                    htmlu = reader.ReadToEnd();
                }

                rateAPI = JsonConvert.DeserializeObject<ExchangeAPI>(htmlu);
                rates = rateAPI.rates;

                File.WriteAllText(ResourceLoader.configFolder + "/exchangerates.json", htmlu);
            }
            catch
            {
                Console.WriteLine("Failed to refresh exchange");
            }
        }
    }
}
