using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System.IO;
using Newtonsoft.Json;

namespace botchicken.Modules
{
    public struct steam
    {
        public bool success;
        public string lowest_price;
        public string volume;
        public string median_price;
    }

    public class TupleList<T1, T2> : List<Tuple<T1, T2>>
    {
        public void Add(T1 item, T2 item2)
        {
            Add(new Tuple<T1, T2>(item, item2));
        }
    }

    public class Pricecheck : ModuleBase<SocketCommandContext>
    {

        [Command("price")]
        [Alias("p", "pc")]
        public async Task pcheck()
        {
            var eb = new EmbedBuilder();
            eb.WithColor(new Color(176, 195, 217));

            var botAuth = new EmbedAuthorBuilder();

            botAuth.WithName("BOT Chicken Price Check");
            botAuth.WithIconUrl("https://cdn.discordapp.com/avatars/286697179949694977/8304744891c425c8b75dcc44e46af5f7.png");

            eb.WithAuthor(botAuth);
            eb.AddField("Usage", "`-p <skin name here>`");

            await ReplyAsync("", false, eb.Build());
        }

        [Command("price")]
        [Alias("p", "pc")]
        public async Task pcheck([Remainder]string arg)
        {
            await fullsearch(Context, arg);
        }

        public async Task fullsearch(SocketCommandContext Ctx, string param)
        {

            string original = param;
            string selected = "";
            int leven = 10000;
            int sizedist = 10000;

            param = param.ToLower();

            var replacements = new TupleList<string, string>
                {
                    { "kato", "katowice" },
                    { "katow", "katowice" },
                    { "fn", "factory new" },
                    { "mw", "minimal wear" },
                    { "ft", "field tested" },
                    { "ww", "well worn" },
                    { "bs", "battle scarred" },
                    { "so", "souvenir"},
                    { "st", "stattrak"},
                    { "bfk", "butterfly knife"},
                    { "ibp", "ibuypower"},
                    { "ruby", "doppler"},
                    { "sapphire", "doppler"},
                    { "emerald", "gamma doppler" },
                    { "ak", "ak 47"},
                    { "scout", "ssg08"},
                    { "deagle", "desert eagle"},
                    { "kara", "karambit"}
                };

            param = " " + param + " ";

            foreach (var word in replacements)
            {
                param = param.Replace(" " + word.Item1 + " ", " " + word.Item2 + " ");
            }

            if (param[0] == ' ')
            {
                param = param.Substring(1);
            }

            if (param[param.Length - 1] == ' ')
            {
                param = param.Remove(param.Length - 1, 1);
            }

            await Task.Run(() =>
            {
                foreach (string item in ResourceLoader.allhashnames)
                {
                    int val = 0;

                    foreach (var piece in pstring(param).Split(' '))
                    {
                        string p = piece;

                        int similarity = 10000;

                        foreach (var apiece in pstring(item).Split(' '))
                        {
                            int test = levenschtein(p, apiece);
                            similarity = Math.Min(levenschtein(p, apiece), similarity);
                        }

                        val += similarity;
                    }

                    if (val < leven || (val == leven && Math.Abs(pstring(item).Split(' ').Length - pstring(param).Split(' ').Length) < sizedist))
                    {
                        selected = item;
                        leven = val;
                        sizedist = Math.Abs(pstring(item).Split(' ').Length - pstring(param).Split(' ').Length);
                    }
                }
            });

            if (leven > 10)
            {
                await BadEmbed(Ctx, param);
                return;
            }

            if (selected != "")
            {
                await Drawembed(Ctx, selected, "");
            }
            else
            {
                await BadEmbed(Ctx, param);
            }
        }

        public async Task BadEmbed(SocketCommandContext Ctx, string input)
        {
            var eb = new EmbedBuilder();
            eb.WithTitle("Unknown Item");
            eb.WithColor(Color.Blue);
            eb.AddField("Couldn't parse keywords", "`" + input + "`\nPlease make sure your item exists in the specified condition.");
            eb.AddField("Support", "Join our [support server](https://discord.gg/hh9v4eF) if you believe this is wrong.");
            var botAuth = new EmbedAuthorBuilder();

            botAuth.WithName("BOT Chicken Price Check");
            botAuth.WithIconUrl("https://cdn.discordapp.com/avatars/286697179949694977/8304744891c425c8b75dcc44e46af5f7.png");

            eb.WithAuthor(botAuth);

            await ReplyAsync("", false, eb.Build());
        }

        public async Task Drawembed(SocketCommandContext Ctx, string hash_name, string color)
        {
            AccountHandler.AccExists(Context.User.Id);

            AccountHandler.accounts[Context.User.Id].uses++;
            AccountHandler.SaveFile();
            
            IDictionary<string, Color> rarities = new Dictionary<string, Color>()
            {
                {"consumer grade", new Color(176,195,217) },
                {"industrial grade", new Color(94, 152, 217) },
                {"mil-spec", new Color(75, 105, 255) },
                {"restricted", new Color(136, 71, 255) },
                {"classified", new Color(211, 46, 230) },
                {"covert", new Color(235, 75, 75) },
                {"extraordinary gloves", new Color(235, 75, 75) },
                {"contraband", new Color(255, 174, 57) },
                {"b0c3d9", new Color(176,195,217) },
                {"5e98d9", new Color(94, 152, 217) },
                {"4b69ff", new Color(75, 105, 255) },
                {"8847ff", new Color(136, 71, 255) },
                {"d32ce6", new Color(211, 46, 230) },
                {"eb4b4b", new Color(235, 75, 75) },
                {"e4ae39", new Color(255, 174, 57) },
                {"d2d2d2", new Color(210, 210, 210) }
            };

            var eb = new EmbedBuilder();
            eb.WithTitle(hash_name);
            eb.WithDescription(Ctx.User.Mention + " Currency: " + AccountHandler.accounts[Context.User.Id].currency);
            eb.WithColor(new Color(176, 195, 217));

            var botAuth = new EmbedAuthorBuilder();

            botAuth.WithName("BOT Chicken Price Check");
            botAuth.WithIconUrl("https://cdn.discordapp.com/avatars/286697179949694977/8304744891c425c8b75dcc44e46af5f7.png");

            eb.WithAuthor(botAuth);

            if (color != null && rarities.ContainsKey(color))
            {
                color = color.ToLower();
                eb.WithColor(rarities[color]);
            }

            string htmlu = string.Empty;
            string convert = @"https://www.xe.com/currencyconverter/convert/?From=USD&To=EUR&Amount=";
            string baseusd = @"http://steamcommunity.com/market/priceoverview/?currency=1&appid=730&market_hash_name=";
            string url = @"";

            url += hash_name.Replace(" ", "%20").Replace(")", "%29").Replace("(", "%28");



            string neutral = neutralhash(hash_name);
            if (ResourceLoader.trader[neutral].icon_url != null)
            {
                eb.WithThumbnailUrl(ResourceLoader.trader[neutral].icon_url);
            }

            if (ResourceLoader.trader[neutral].name_color != null && rarities.ContainsKey(ResourceLoader.trader[neutral].name_color))
            {
                eb.WithColor(rarities[ResourceLoader.trader[neutral].name_color]);
            }

            if (ResourceLoader.csgobackpack.items_list.ContainsKey(neutral) && ResourceLoader.csgobackpack.items_list[neutral] != null)
            {
                if (ResourceLoader.csgobackpack.items_list[neutral].icon_url != null)
                {
                    eb.WithThumbnailUrl(@"https://steamcommunity-a.akamaihd.net/economy/image/" + ResourceLoader.csgobackpack.items_list[neutral].icon_url);
                }

                if (ResourceLoader.csgobackpack.items_list[neutral].rarity_color != null)
                {
                    eb.WithColor(rarities[ResourceLoader.csgobackpack.items_list[neutral].rarity_color.ToLower()]);
                }
            }

            string item_page = null;

            await Task.Run(() => { 
            if (ResourceLoader.trader[hash_name].csgotrader.price != null)
            {
                string splink = "";
                if (ResourceLoader.skinport.ContainsKey(hash_name) && ResourceLoader.skinport[hash_name].item_page != null)
                {
                        item_page = ResourceLoader.skinport[hash_name].item_page;

                        if (Context.Guild.Id == 727970463325749268)
                        {
                            item_page += "?r=hade";
                        }
                        else if (Context.Guild.Id == 664104795367538690)
                        {
                            item_page += "?r=hostile";
                        }
                        else
                        {
                            item_page += "?r=botchicken";
                        }

                        splink = "\n[**Best Deals**](" + item_page + ")";
                }


                int i = 0;
                string doppler = "";

                string[] dopplers = { "Phase 1",
                                      "Phase 2",
                                      "Phase 3",
                                      "Phase 4",
                                      "Ruby",
                                      "Sapphire",
                                      "Black Pearl",
                                      "Emerald"};

                if (ResourceLoader.trader[hash_name].csgotrader.doppler != null)
                {
                    foreach (string t in dopplers)
                    {
                        if (ResourceLoader.trader[hash_name].csgotrader.doppler.ContainsKey(t) && ResourceLoader.trader[hash_name].csgotrader.doppler[t] != null)
                        {
                            if (i % 2 == 0 && i != 0)
                            {
                                doppler += "\n";
                            }
                            else if (i != 0)
                            {
                                doppler += "  ·  ";
                            }

                            doppler +=  t + ": `" + ExchangeRate.Convert(Double.Parse(ResourceLoader.trader[hash_name].csgotrader.doppler[t]), AccountHandler.accounts[Context.User.Id].currency) + "`";

                            i++;
                        }
                    }

                    eb.AddField("<:botchicken:740299794550882324>  ·  Doppler Evaluation", doppler + splink);
                }
                else
                {
                    eb.AddField("<:botchicken:740299794550882324>  ·  Bot Suggestion",
                        ExchangeRate.Convert(Double.Parse(ResourceLoader.trader[hash_name].csgotrader.price),
                        AccountHandler.accounts[Context.User.Id].currency)
                        + splink, true);
                }
            }

            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(baseusd + url);
                request.AutomaticDecompression = DecompressionMethods.GZip;

                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                using (Stream stream = response.GetResponseStream())
                using (StreamReader reader = new StreamReader(stream))
                {
                    htmlu = reader.ReadToEnd();
                }
                steam readu = JsonConvert.DeserializeObject<steam>(htmlu);

                string sdata = "";

                if (readu.lowest_price != null)
                {
                    sdata = ExchangeRate.Convert(Double.Parse(readu.lowest_price.Split('$')[1]), AccountHandler.accounts[Context.User.Id].currency) + "\n";
                }
                else if (readu.median_price != null)
                {
                    sdata = ExchangeRate.Convert(Double.Parse(readu.median_price.Split('$')[1]), AccountHandler.accounts[Context.User.Id].currency) + "\n";
                }

                if (readu.volume != null)
                {
                    sdata += "Volume: " + readu.volume + " · [(market listings)](https://steamcommunity.com/market/listings/730/" + @url + ")";
                }
                else
                {
                    sdata += "Volume: 0 · [(market listings)](https://steamcommunity.com/market/listings/730/" + @url + ")";
                }

                eb.AddField("<:steam:740300441044123669>  ·  Steam Market", sdata, true);
            }
            catch
            {
                eb.AddField("<:steam:740300441044123669>  ·  Steam Market", "Volume: 0", true);
            }

            });

            

            bool wrap = true;

            if (ResourceLoader.skinport.ContainsKey(hash_name))
            {
                string price = "";

                if (ResourceLoader.skinport[hash_name].min_price != null)
                {
                    price = ResourceLoader.skinport[hash_name].min_price;
                }
                else if (ResourceLoader.skinport[hash_name].mean_price != null)
                {
                    price = ResourceLoader.skinport[hash_name].mean_price;
                }

                

                if (price != "" && item_page != null)
                {
                    price = ExchangeRate.Convert(Double.Parse(price), AccountHandler.accounts[Context.User.Id].currency);
                    eb.AddField("Third-party Prices", "‎");
                    eb.AddField("<:skinport:747619241250783353>  ·  SKINPORT", price
                    + "\n[(market listings)](" + item_page + ")", true);
                    //wrap = !wrap;
                }
                else
                {
                    wrap = false;
                }
                
            }

            if (ResourceLoader.trader[hash_name].buff163 != null && (ResourceLoader.trader[hash_name].buff163.highest_order != null || ResourceLoader.trader[hash_name].buff163.starting_at != null))
            {
                double minpr;

                //if (ResourceLoader.trader[hash_name].buff163.highest_order != null && ResourceLoader.trader[hash_name].buff163.starting_at != null)
                //{
                //    minpr = Math.Min(
                //            Double.Parse(ResourceLoader.trader[hash_name].buff163.highest_order.price),
                //            Double.Parse(ResourceLoader.trader[hash_name].buff163.starting_at.price)
                //        );
                //}
                //else
                //{
                //    if (ResourceLoader.trader[hash_name].buff163.highest_order != null)
                //    {
                //        minpr = Double.Parse(ResourceLoader.trader[hash_name].buff163.highest_order.price);
                //    }
                //    else
                //    {
                //        minpr = Double.Parse(ResourceLoader.trader[hash_name].buff163.starting_at.price);
                //    }
                //}

                if (ResourceLoader.trader[hash_name].buff163.starting_at != null)
                {
                    minpr = Double.Parse(ResourceLoader.trader[hash_name].buff163.starting_at.price);
                    
                }
                else
                {
                    minpr = Double.Parse(ResourceLoader.trader[hash_name].buff163.highest_order.price);
                }

                eb.AddField("<:buff163:801522918776766526>  ·  buff.163",
                ExchangeRate.Convert(minpr, AccountHandler.accounts[Context.User.Id].currency), wrap);
                wrap = !wrap;
            }

            if (ResourceLoader.trader[hash_name].csmoney != null && ResourceLoader.trader[hash_name].csmoney.price != null && Context.Guild.Id != 727970463325749268 && Context.Guild.Id != 664104795367538690)
            {
                eb.AddField("<:csmoney:740301151303630919>  ·  CS.MONEY",
                ExchangeRate.Convert(Double.Parse(ResourceLoader.trader[hash_name].csmoney.price), AccountHandler.accounts[Context.User.Id].currency), wrap);
                wrap = !wrap;
            }

            

            //if (ResourceLoader.trader[hash_name].bitskins != null && ResourceLoader.trader[hash_name].bitskins.price != null)
            //{
            //    eb.AddField("<:bitskins:740301421290717307>  ·  Bitskins", "$" + ResourceLoader.trader[hash_name].bitskins.price
            //        + " [USD](" + convert + ResourceLoader.trader[hash_name].bitskins.price
            //        + ")\n[(market listings)](https://bitskins.com/?market_hash_name=" + @url + ")", true);
            //}

            //if (SkinDB.buffdb.ContainsKey(hash_name))
            //{
            //    if (SkinDB.buffdb[hash_name].sell_min_price != "0")
            //    {
            //        eb.AddField("buff.163:", "USD: $" + (int.Parse(SkinDB.buffdb[hash_name].sell_min_price) * 0.14f).ToString());
            //    }
            //}

            string sp_page = "https://skinport.com/r/";

            if (Context.Guild.Id == 727970463325749268)
            {
                sp_page += "hade";
            }
            else if (Context.Guild.Id == 664104795367538690)
            {
                sp_page += "hostile";
            }
            else
            {
                sp_page += "botchicken";
            }

            eb.AddField("Links", @"[Add me to Discord](https://discord.com/oauth2/authorize?client_id=286697179949694977&scope=bot&permissions=268782656) · [Join our Server](https://discord.gg/hh9v4eF) · [Skinport](" + sp_page + ")");
            eb.WithFooter("If this item is not what you entered, please join our support server and contact us.");

            await ReplyAsync("", false, eb.Build());
        }

        public static string pstring(string input)
        {
            var str = input.ToLower().Replace("|", " ").Replace("(", " ").Replace("%20", " ")
                      .Replace(")", " ").Replace("-", " ").Replace(".", " ").Replace("★", " ")
                      .Replace("_", " ").Replace("-", " ").Replace("  ", " ").Replace("%7", " ")
                      .Replace("%28", " ").Replace("%29", " ")
                      .Replace("  ", " ").Replace("  ", " ").Replace("  ", " ").Replace("  ", " ");

            if (str[0] == ' ')
            {
                str = str.Substring(1);
            }

            return str;
        }

        public static int levenschtein(string a, string b)
        {
            if (String.IsNullOrEmpty(a) && String.IsNullOrEmpty(b))
            {
                return 0;
            }
            if (String.IsNullOrEmpty(a))
            {
                return b.Length;
            }
            if (String.IsNullOrEmpty(b))
            {
                return a.Length;
            }
            int lengthA = a.Length;
            int lengthB = b.Length;
            var distances = new int[lengthA + 1, lengthB + 1];
            for (int i = 0; i <= lengthA; distances[i, 0] = i++) ;
            for (int j = 0; j <= lengthB; distances[0, j] = j++) ;

            for (int i = 1; i <= lengthA; i++)
                for (int j = 1; j <= lengthB; j++)
                {
                    int cost = b[j - 1] == a[i - 1] ? 0 : 1;
                    distances[i, j] = Math.Min
                        (
                        Math.Min(distances[i - 1, j] + 1, distances[i, j - 1] + 1),
                        distances[i - 1, j - 1] + cost
                        );
                }
            return distances[lengthA, lengthB];
        }

        public static string neutralhash(string input)
        {
            string ret = input.Replace("StatTrak™", "").Replace("Souvenir", "").Replace("  ", " ");

            if (ret[0] == ' ')
            {
                return ret.Substring(1);
            }

            return ret;
        }
    }
}
