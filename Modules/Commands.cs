using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Discord.Rest;
using System.IO;
using Newtonsoft.Json;
using System.Net;
using DiscordBotsList.Api;
using DiscordBotsList;
using DiscordBotsList.Api.Objects;

namespace botchicken.Modules
{
    public struct SteamAcc
    {
        public string steamid;
        public int communityvisibilitystate;
        public int profilestate;
        public string personaname;
        public int commentpermission;
        public string profileurl;
        public string avatar;
        public string avatarmedium;
        public string avatarfull;
        public string avatarhash;
        public int lastlogoff;
        public int personastate;
        public string realname;
        public string primaryclanid;
        public int timecreated;
        public int personastateflags;
    }

    public struct SteamResponse
    {
        public SteamAcc[] players;
    }

    public struct SteamQuery
    {
        public SteamResponse response;
    }

    public struct backpackresp
    {
        public bool success;
        public string value;
        public string items;
    }

    public class Commands : ModuleBase<SocketCommandContext>
    {
        [Command("status")]
        public async Task status()
        {
            if (Context.User.Id == 265965692149432344)
            {
                var embed = new EmbedBuilder();

                var botAuth = new EmbedAuthorBuilder();
                botAuth.WithName("Status Report");
                botAuth.WithIconUrl("https://cdn.discordapp.com/avatars/286697179949694977/8304744891c425c8b75dcc44e46af5f7.png");

                
                embed.WithAuthor(botAuth);
                embed.WithDescription("Server Count: " + Context.Client.Guilds.Count().ToString());
                int mems = 0;

                foreach (SocketGuild server in Context.Client.Guilds)
                {
                    mems += server.MemberCount;
                }

                embed.AddField("Users: " + mems.ToString(), "Accounts: " + AccountHandler.accounts.Count);
               
                embed.WithColor(Color.Green);

                await ReplyAsync("", false, embed.Build());
            }
        }

        [Command("refresh")]
        public async Task refr()
        {
            if (Context.User.Id == 265965692149432344)
            {
                if (ConfigLoader.cfg.token[1] == 'j')
                {
                    AuthDiscordBotListApi DblApi = new AuthDiscordBotListApi(286697179949694977, ConfigLoader.cfg.topgg);

                    IDblSelfBot me = await DblApi.GetMeAsync();

                    int actual = Context.Client.Guilds.Count();
                    Console.WriteLine("Guild Count: " + actual.ToString());

                    await me.UpdateStatsAsync(actual);

                }

                await ReplyAsync("Refreshed stuff nerd");
            }
        }

        [Command("refprices")]
        public async Task pricerefresh()
        {
            if (Context.User.Id == 265965692149432344)
            {
                await ReplyAsync("Refreshing skinport");

                try
                {
                    string htmlu = string.Empty;
                    HttpWebRequest request = (HttpWebRequest)WebRequest.Create("https://api.skinport.com/v1/items?app_id=730");
                    request.Headers["authorization"] = "Basic " + ResourceLoader.Base64Encode(ConfigLoader.cfg.portid + ":" + ConfigLoader.cfg.portsecret);
                    request.AutomaticDecompression = DecompressionMethods.GZip;

                    using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                    using (Stream stream = response.GetResponseStream())
                    using (StreamReader reader = new StreamReader(stream))
                    {
                        htmlu = reader.ReadToEnd();
                    }

                    List<PortItem> sp = JsonConvert.DeserializeObject<List<PortItem>>(htmlu);
                    File.WriteAllText(ResourceLoader.configFolder + "/skinportdel.json", htmlu);

                    foreach (PortItem cur in sp)
                    {
                        ResourceLoader.skinport[cur.market_hash_name] = cur;
                    }

                    await ReplyAsync("Refreshed skinport");
                }
                catch
                {
                    await ReplyAsync("Refresh failed");
                }

                await ReplyAsync("Refreshing csgobackpack");

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
                    ResourceLoader.csgobackpack = JsonConvert.DeserializeObject<BackpackRoot>(htmlu);

                    File.WriteAllText(ResourceLoader.configFolder + "/csgobackpackdel.json", htmlu);

                    await ReplyAsync("Refreshed csgobackpack");
                }
                catch
                {
                    await ReplyAsync("Refresh failed");
                }

                await ReplyAsync("Refreshing traderapp");

                //try
                //{
                //    using (var client = new WebClient())
                //    {
                //        client.DownloadFile("https://prices.csgotrader.app/latest/prices_v6.json", ResourceLoader.configFolder + "/" + ResourceLoader.traderFile);
                //    }

                //    string json = File.ReadAllText(ResourceLoader.configFolder + "/" + ResourceLoader.traderFile);
                //    json = json.Replace("\"null\"", "null");
                //    ResourceLoader.trader = JsonConvert.DeserializeObject<Dictionary<string, traderitem>>(json);

                //    await ReplyAsync("Refreshed traderapp");
                //}
                //catch
                //{
                //    await ReplyAsync("Refresh failed");
                //}
             }
        }



        [Command("help")]
        [Alias("h")]
        public async Task help()
        {
            var embed = new EmbedBuilder();

            var botAuth = new EmbedAuthorBuilder();
            botAuth.WithName("BOT Chicken Help");
            botAuth.WithIconUrl("https://cdn.discordapp.com/avatars/286697179949694977/8304744891c425c8b75dcc44e46af5f7.png");

            embed.WithAuthor(botAuth);
            embed.WithDescription("BOT Chicken is a CS:GO item price-check bot.");
            embed.AddField("Commands", "Bot chicken will respond to the prefix `-`. Use `-help commands` to view the list of commands and `-help price` to see how to price check.");
            embed.AddField("Server Owners", "Use `-help admin` to see how you can configure your server");
            embed.AddField("Support", "If you have any questions you can join our [support server](https://discord.gg/hh9v4eF)");
            embed.AddField("Add to Discord", "Click [here](https://discord.com/oauth2/authorize?client_id=286697179949694977&scope=bot&permissions=268782656) to add BOT Chicken to your own server.");
            embed.AddField("Donate", "Developing this bot for free isn't easy. You can donate CS:GO items [here](https://steamcommunity.com/tradeoffer/new/?partner=259282001&token=U5N-upE5)");
            embed.WithFooter("Contact stig#6649 or join our support server if you discover any bugs or have feature suggestions.");
            embed.WithColor(new Color(252, 176, 12));

            await ReplyAsync("", false, embed.Build());
        }

        [Command("help")]
        [Alias("h")]
        public async Task help(string a)
        {
            a = a.ToLower();
            if (a == "price" || a == "pricecheck")
            {
                var embed = new EmbedBuilder();

                var botAuth = new EmbedAuthorBuilder();
                botAuth.WithName("BOT Chicken Price Check");
                botAuth.WithIconUrl("https://cdn.discordapp.com/avatars/286697179949694977/8304744891c425c8b75dcc44e46af5f7.png");

                embed.WithAuthor(botAuth);
                embed.WithDescription("Syntax: -price <weapon_name> <skin_name> (condition) (stattrak or souvenir)");
                embed.AddField("Price Check", "Bot chicken will respond to the prefix `-`. Use `-help commands` to view the list of commands and `-help price` to see how to price check.");
                embed.AddField("Examples", "`-p awp dlore so ft` - Souvenir AWP | Dragon Lore (Field-Tested)\n`-p ak asiimov` - AK-47 | Asiimov (Factory New)\n`-p butterfly marble fade` - ★ Butterfly Knife | Marble Fade (Factory New)");
                embed.AddField("Notes", "-Parameters and commands are not case sensitive\n-Order of condition and stattrak does not matter\n-The bot will accept nicknames like `scout`(SSG 08), `dlore` (Dragon Lore), `st`(Stattrak) or `mw`(Minimal Wear)");
                embed.AddField("Support", "If you have any questions you can join our [support server](https://discord.gg/hh9v4eF)");
                embed.WithFooter("Contact stig#6649 or join our support server if you discover any bugs or have feature suggestions.");
                embed.WithColor(new Color(252, 176, 12));

                await ReplyAsync("", false, embed.Build());
            }
            
            if (a == "commands" || a == "command" || a == "cmd")
            {
                var embed = new EmbedBuilder();

                var botAuth = new EmbedAuthorBuilder();
                botAuth.WithName("BOT Chicken Commands");
                botAuth.WithIconUrl("https://cdn.discordapp.com/avatars/286697179949694977/8304744891c425c8b75dcc44e46af5f7.png");

                embed.WithAuthor(botAuth);
                embed.WithDescription("<> are required parameters and () are optional");
                embed.AddField("Help", "`-help` - Main help menu\n`-help commands` - Lists all available commands\n`-help price` - Price check commands with usage examples\n**Aliases:** -help, -h");

                embed.AddField("General", "`-link` - Links your steam to this bot\n`-unlink` - Unlinks your steam account \n`-donate` - (donation screen)");
                embed.AddField("Price Check", "`-price <weapon_name> <skin_name> (condition) (stattrak or souvenir)` - Price checks weapon finishes and gloves\n`-price <knife_name> (skin_name) (condition) (stattrak)` - Prices knife skins");
                embed.AddField("Inventory", "`-inv` - Gets own inventory if linked (use `-link`)\n`-inv <steamid64>` - Gets inventory of inputted steamid64\n`-inv @someone` - Gets inventory of another user");
                embed.AddField("Support", "If you have any questions you can join our [support server](https://discord.gg/hh9v4eF)");
                embed.WithFooter("Contact stig#6649 or join our support server if you discover any bugs or have feature suggestions.");
                embed.WithColor(new Color(252, 176, 12));

                await ReplyAsync("", false, embed.Build());
            }

            if (a == "admin")
            {
                var embed = new EmbedBuilder();

                var botAuth = new EmbedAuthorBuilder();
                botAuth.WithName("BOT Chicken Admin");
                botAuth.WithIconUrl("https://cdn.discordapp.com/avatars/286697179949694977/8304744891c425c8b75dcc44e46af5f7.png");

                embed.WithAuthor(botAuth);
                embed.WithDescription("<> are required parameters and () are optional");
                embed.AddField("Inventory Value", "With Bot Chicken, you can configure custom roles to be assigned if someone has an inventory of a certain value.\n" +
                                "For Example: Say I want someone with a $10 inventory to be given a role called `$10+` and someone with an inventory worth $100k to be given a role called `Mega Rich`\n" +
                                "I can use the command: `-setinvroles $10:10;Mega Rich:100000`\n +" +
                                "Please contact stig#6649 on discord if you have any issues");
                embed.WithColor(new Color(252, 176, 12));

                await ReplyAsync("", false, embed.Build());
            }
        }

        [Command("currency")]
        [Alias("c", "cur")]
        public async Task currency(string cur)
        {
            AccountHandler.AccExists(Context.User.Id);

            if (ExchangeRate.ValidCurr(cur))
            {
                AccountHandler.accounts[Context.User.Id].currency = cur.ToUpper();
                AccountHandler.SaveFile();
                await ReplyAsync("Your currency is now set to `" + cur.ToUpper() + "`");
            }
            else
            {
                await ReplyAsync("Invalid currency. Run -cur for usage");
            }
        }


        [Command("currency")]
        [Alias("c", "cur")]
        public async Task currency()
        {
            AccountHandler.AccExists(Context.User.Id);

            var eb = new EmbedBuilder();
            eb.WithTitle("Command Usage");
            eb.WithColor(Color.Orange);
            eb.AddField("Currency", "Your current currency is set to `" + AccountHandler.accounts[Context.User.Id].currency + "`");
            eb.AddField("Example", "`-cur EUR` (replace with the ISO code of your preferred currency)");
            eb.AddField("Support", "Join our [server](https://discord.gg/hh9v4eF) to report any bugs.");
            var botAuth = new EmbedAuthorBuilder();

            botAuth.WithName("BOT Chicken Currency Setting");
            botAuth.WithIconUrl("https://cdn.discordapp.com/avatars/286697179949694977/8304744891c425c8b75dcc44e46af5f7.png");

            eb.WithAuthor(botAuth);

            await ReplyAsync("", false, eb.Build());

            await ReplyAsync();

        }


        [Command("unlink")]
        public async Task unlink()
        {
            AccountHandler.AccExists(Context.User.Id);

            AccountHandler.accounts[Context.User.Id].steam_id = 0;
            AccountHandler.SaveFile();

            await Context.Channel.SendMessageAsync(Context.User.Mention + " your account has been unlinked.");
        }

        [Command("link")]
        public async Task verify()
        {
            AccountHandler.AccExists(Context.User.Id);

            if (AccountHandler.accounts[Context.User.Id].steam_id > 0)
            {
                await Context.Channel.SendMessageAsync(Context.User.Mention + " your steam account is already linked.");
                return;
            }

            var eb = new EmbedBuilder();
            eb.WithTitle("Command Usage");
            eb.WithColor(Color.Orange);
            eb.AddField("Link command ussage", "Get your steamid64 from [steamid.io](https://steamid.io/).\n `-link <steamid64>`");
            eb.AddField("Example", "`-link 76561198219547729` (replace with your steamid64)");
            eb.AddField("Support", "Join our [server](https://discord.gg/hh9v4eF) to report any bugs.");
            var botAuth = new EmbedAuthorBuilder();

            botAuth.WithName("BOT Chicken Steam Verify");
            botAuth.WithIconUrl("https://cdn.discordapp.com/avatars/286697179949694977/8304744891c425c8b75dcc44e46af5f7.png");

            eb.WithAuthor(botAuth);

            await ReplyAsync("", false, eb.Build());
        }

        [Command("masterlink")]
        public async Task verify(ulong discid, Int64 steamid)
        {
            if (Context.User.Id == 265965692149432344)
            {
                AccountHandler.AccExists(discid);
                AccountHandler.accounts[discid].steam_id = steamid;
                AccountHandler.SaveFile();

                await ReplyAsync("Manually linked account");
            }
        }

        [Command("link")]
        public async Task verify(Int64 steamid)
        {
            AccountHandler.AccExists(Context.User.Id);

            if (AccountHandler.accounts[Context.User.Id].steam_id > 0)
            {
                await Context.Channel.SendMessageAsync(Context.User.Mention + " your steam account is already linked.");
                return;
            }

            await Task.Run(() => {
                try
                {
                    string url = @"http://api.steampowered.com/ISteamUser/GetPlayerSummaries/v0002/?key=" + ConfigLoader.cfg.steamapi + "&steamids=";

                    url += steamid;

                    string htmlu = string.Empty;

                    HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                    request.AutomaticDecompression = DecompressionMethods.GZip;

                    using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                    using (Stream stream = response.GetResponseStream())
                    using (StreamReader reader = new StreamReader(stream))
                    {
                        htmlu = reader.ReadToEnd();
                    }

                    SteamQuery readu = JsonConvert.DeserializeObject<SteamQuery>(htmlu);

                    string idhash = ResourceLoader.Base64Encode(((ulong)steamid + Context.User.Id).ToString().Substring(8, 5)).Substring(1, 5).ToUpper();

                    var eb = new EmbedBuilder();
                    eb.WithColor(Color.Orange);
                    eb.WithDescription(Context.User.Mention);

                    var botAuth = new EmbedAuthorBuilder();
                    botAuth.WithName("BOT Chicken Steam Verify");
                    botAuth.WithIconUrl("https://cdn.discordapp.com/avatars/286697179949694977/8304744891c425c8b75dcc44e46af5f7.png");

                    eb.WithAuthor(botAuth);

                    if (readu.response.players[0].personaname.ToLower().EndsWith(idhash.ToLower()))
                    {
                        AccountHandler.accounts[Context.User.Id].steam_id = steamid;
                    
                        eb.AddField("Verification successful", "Please remove `" + idhash + 
                                                               "` from your name.\nYou may now use commands such as `-inv`");

                        AccountHandler.SaveFile();
                    }
                    else
                    {
                        eb.AddField("Profile: " + readu.response.players[0].personaname, "If this is your account,\n Please add `" + idhash +
                                                                                         "` to the end of your name. \n [Click here](https://steamcommunity.com/profiles/" + steamid.ToString() +
                                                                                         "/edit) to change it, then rerun the command.");
                        eb.WithThumbnailUrl(readu.response.players[0].avatarfull);
                    }

                    ReplyAsync("", false, eb.Build());
                }
                catch
                {
                    Failverify(Context);
                }
            });
        }

        [Command("link")]
        public async Task verify([Remainder]string para)
        {
            AccountHandler.AccExists(Context.User.Id);

            if (AccountHandler.accounts[Context.User.Id].steam_id > 0)
            {
                await Context.Channel.SendMessageAsync(Context.User.Mention + " your steam account is already linked.");
                return;
            }

            await Failverify(Context);
        }

        public async Task Failverify(SocketCommandContext Ctx)
        {
            var eb = new EmbedBuilder();
            eb.WithTitle("Invalid steamid64");
            eb.WithColor(Color.Orange);
            eb.AddField("Couldn't find steamid64", "Get your steamid64 from [steamid.io](https://steamid.io/).\n It should look similar to `76561198219547729`");
            eb.AddField("Example", "`-link 76561198219547729` (replace with your steamid64)");
            eb.AddField("Support", "Join our [server](https://discord.gg/hh9v4eF) to report any bugs.");
            var botAuth = new EmbedAuthorBuilder();

            botAuth.WithName("BOT Chicken Steam Verify");
            botAuth.WithIconUrl("https://cdn.discordapp.com/avatars/286697179949694977/8304744891c425c8b75dcc44e46af5f7.png");

            eb.WithAuthor(botAuth);

            await ReplyAsync("", false, eb.Build());
        }
    }
}
