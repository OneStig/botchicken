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

namespace botchicken.Modules
{
    public class Inventory : ModuleBase<SocketCommandContext>
    {
        [Command("setinvroles")]
        public async Task Setinvroles()
        {
            await ReplyAsync("Failed to set inventory roles");
        }

        [Command("setinvroles")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task Setinvroles([Remainder]string input)
        {
            try
            {
                string[] items = input.Split(';');
                string[] rolenames = new string[items.Length];
                int[] values = new int[items.Length];

                int i = 0;
                foreach (string item in items)
                {
                    rolenames[i] = item.Split(':')[0];
                    int x = 0;
                    if (Int32.TryParse(item.Split(':')[1], out x))
                    {
                        values[i] = x;
                    }
                    else
                    {
                        await ReplyAsync("Failed to set inventory roles");
                        return;
                    }

                    i++;
                }

                if (ResourceLoader.guildsettings.ContainsKey(Context.Guild.Id))
                {
                    ResourceLoader.guildsettings[Context.Guild.Id].values = values;
                    ResourceLoader.guildsettings[Context.Guild.Id].rolenames = rolenames;
                }
                else
                {
                    guildinv newguild = new guildinv();
                    newguild.values = values;
                    newguild.rolenames = rolenames;
                    ResourceLoader.guildsettings.Add(Context.Guild.Id, newguild);
                }

                ResourceLoader.SaveGuild();
                await ReplyAsync("Successfully set new roles and values");
            }
            catch
            {
                await ReplyAsync("Failed to set inventory roles");
            }
        }

        public async Task DrawValue(SocketCommandContext ctx, string steamid)
        {
            var msg = await Context.Channel.SendMessageAsync("Calculating Inventory value...");

            AccountHandler.AccExists(Context.User.Id);

            AccountHandler.accounts[Context.User.Id].uses++;
            AccountHandler.SaveFile();

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

                    //readu.response.players[0].personaname.ToLower();

                    url = @"http://csgobackpack.net/api/GetInventoryValue/?key=" + ConfigLoader.cfg.csgobackpack + "&id=" + steamid;

                    htmlu = string.Empty;

                    request = (HttpWebRequest)WebRequest.Create(url);
                    request.AutomaticDecompression = DecompressionMethods.GZip;

                    using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                    using (Stream stream = response.GetResponseStream())
                    using (StreamReader reader = new StreamReader(stream))
                    {
                        htmlu = reader.ReadToEnd();
                    }

                    backpackresp respon = JsonConvert.DeserializeObject<backpackresp>(htmlu);

                    if (respon.success)
                    {
                        var eb = new EmbedBuilder();
                        eb.WithDescription(Context.User.Mention);
                        eb.WithTitle(readu.response.players[0].personaname + "'s Inventory");
                        eb.WithColor(Color.Orange);

                        string converted = ExchangeRate.Convert(Double.Parse(respon.value), AccountHandler.accounts[Context.User.Id].currency);

                        eb.AddField("CS:GO Inventory Value", "**" + respon.items + "** items worth **" + 
                                    converted
                                    + " ** [(Refresh)](https://csgobackpack.net/?nick=" + steamid + "&ref=1)");
                        eb.WithThumbnailUrl(readu.response.players[0].avatarfull);
                        var botAuth = new EmbedAuthorBuilder();

                        botAuth.WithName("BOT Chicken Inventory Value");
                        botAuth.WithIconUrl("https://cdn.discordapp.com/avatars/286697179949694977/8304744891c425c8b75dcc44e46af5f7.png");

                        AccountHandler.AccExists(Context.User.Id);

                        if (ResourceLoader.guildsettings.ContainsKey(Context.Guild.Id) &&
                            AccountHandler.accounts[Context.User.Id].steam_id > 0 &&
                            AccountHandler.accounts[Context.User.Id].steam_id.ToString() == steamid &&
                            ResourceLoader.guildsettings[Context.Guild.Id].rolenames != null)
                        {

                            int prevsize = 0;
                            int ind = -1;

                            int i = 0;
                            foreach (int val in ResourceLoader.guildsettings[Context.Guild.Id].values)
                            {
                                if (double.Parse(respon.value) > val && val > prevsize)
                                {
                                    prevsize = val;
                                    ind = i;
                                }
                                i++;
                            }

                            if (ind != -1)
                            {
                                try
                                {
                                    var role = ctx.Guild.Roles.First(x => x.Name.ToString() == ResourceLoader.guildsettings[Context.Guild.Id].rolenames[ind]);
                                    
                                    if (!((ctx.User as IGuildUser).RoleIds.Contains(role.Id))) {
                                        if (role != null)
                                        {
                                            (ctx.User as IGuildUser).AddRoleAsync(role);
                                            eb.AddField("New Role", "You were given the role `" + ResourceLoader.guildsettings[Context.Guild.Id].rolenames[ind] + "`");
                                        }
                                        else
                                        {
                                            eb.AddField("New Role", "We tried to give you the role `" + ResourceLoader.guildsettings[Context.Guild.Id].rolenames[ind] + "` but it doesn't exist.");
                                        }
                                    }
                                }
                                catch
                                {
                                    eb.AddField("New Role", "We tried to give you the role `" + ResourceLoader.guildsettings[Context.Guild.Id].rolenames[ind] + "` but it doesn't exist.");
                                }
                            }
                        }

                        string item_page = "https://skinport.com/r/";

                        if (Context.Guild.Id == 727970463325749268)
                        {
                            item_page += "hade";
                        }
                        else if (Context.Guild.Id == 664104795367538690)
                        {
                            item_page += "hostile";
                        }
                        else
                        {
                            item_page += "botchicken";
                        }

                        eb.AddField("Skinport", "[Buy items](" + item_page + ")");

                        eb.WithAuthor(botAuth);

                        msg.ModifyAsync((a) => {
                            a.Content = "";
                            a.Embed = eb.Build();
                        });
                    }
                    else
                    {
                        var eb = new EmbedBuilder();
                        eb.WithTitle("Inventory Evaluation Failed");
                        eb.WithColor(Color.Orange);
                        eb.AddField("Failed to get inventory value", "This may be because CS:GO inventory API is down (try again later) or the specified inventory is private"
                            + "\nor [(refresh inventory)](https://csgobackpack.net/?nick=" + steamid + ") and try again");
                        eb.AddField("Support", "Join our [server](https://discord.gg/hh9v4eF) to report any bugs.");
                        var botAuth = new EmbedAuthorBuilder();

                        botAuth.WithName("BOT Chicken Inventory Value");
                        botAuth.WithIconUrl("https://cdn.discordapp.com/avatars/286697179949694977/8304744891c425c8b75dcc44e46af5f7.png");

                        eb.WithAuthor(botAuth);

                        msg.ModifyAsync((a) => {
                            a.Content = "";
                            a.Embed = eb.Build();
                        });
                    }

                }
                catch
                {
                    var eb = new EmbedBuilder();
                    eb.WithTitle("Inventory Evaluation Failed");
                    eb.WithColor(Color.Orange);
                    eb.AddField("Failed to get inventory value", "This may be because CS:GO inventory API is down (try again later) or the specified inventory is private"
                           + "\nor [(refresh inventory)](https://csgobackpack.net/?nick=" + steamid + ") and try again");
                    eb.AddField("Support", "Join our [server](https://discord.gg/hh9v4eF) to report any bugs.");
                    var botAuth = new EmbedAuthorBuilder();

                    botAuth.WithName("BOT Chicken Inventory Value");
                    botAuth.WithIconUrl("https://cdn.discordapp.com/avatars/286697179949694977/8304744891c425c8b75dcc44e46af5f7.png");

                    eb.WithAuthor(botAuth);

                    msg.ModifyAsync((a) => {
                        a.Content = "";
                        a.Embed = eb.Build();
                    });
                }

            });
        }

        [Command("inventory")]
        [Alias("inv", "i")]
        public async Task Invvalue()
        {
            AccountHandler.AccExists(Context.User.Id);

            if (AccountHandler.accounts[Context.User.Id].steam_id > 0)
            {
                await DrawValue(Context, AccountHandler.accounts[Context.User.Id].steam_id.ToString());
                return;
            }

            var eb = new EmbedBuilder();
            eb.WithColor(Color.Orange);
            eb.AddField("Please link your steam", "Link your steam with `-link` before checking your own inventory value");
            var botAuth = new EmbedAuthorBuilder();

            botAuth.WithName("BOT Chicken Inventory Value");
            botAuth.WithIconUrl("https://cdn.discordapp.com/avatars/286697179949694977/8304744891c425c8b75dcc44e46af5f7.png");

            eb.WithAuthor(botAuth);

            await ReplyAsync("", false, eb.Build());
        }

        [Command("inventory")]
        [Alias("inv", "i")]
        public async Task Invvalue(Int64 id)
        {
            await DrawValue(Context, id.ToString());
        }

        [Command("inventory")]
        [Alias("inv", "i")]
        public async Task Invvalue([Remainder]string steamid)
        {
            var eb = new EmbedBuilder();
            var botAuth = new EmbedAuthorBuilder();
            botAuth.WithName("BOT Chicken Inventory Value");
            botAuth.WithIconUrl("https://cdn.discordapp.com/avatars/286697179949694977/8304744891c425c8b75dcc44e46af5f7.png");

            try
            {
                var mentioned = Context.Message.MentionedUsers.FirstOrDefault();

                if (mentioned != null)
                {
                    AccountHandler.AccExists(mentioned.Id);
                }

                if (mentioned != null && AccountHandler.accounts[mentioned.Id].steam_id > 0)
                {
                    await DrawValue(Context, AccountHandler.accounts[mentioned.Id].steam_id.ToString());
                }
                else
                {
                    eb.WithTitle("User Not Found");
                    eb.WithColor(Color.Orange);
                    eb.AddField("Unable to evaluate", "This user does not have their steam account linked.");
                    eb.AddField("Support", "Join our [server](https://discord.gg/hh9v4eF) to report any bugs.");

                    botAuth.WithName("BOT Chicken Inventory Value");
                    botAuth.WithIconUrl("https://cdn.discordapp.com/avatars/286697179949694977/8304744891c425c8b75dcc44e46af5f7.png");

                    eb.WithAuthor(botAuth);

                    await ReplyAsync("", false, eb.Build());
                }

                return;
            }
            catch
            {

            }

            eb.WithTitle("Inventory Command Usage");
            eb.WithColor(Color.Orange);
            eb.AddField("Commands", "`-inv` - Gets own inventory if linked (use `-link`)\n`-inv <steamid64>` - Gets inventory of inputted steamid64\n`-inv @someone` - Gets inventory of another user");
            eb.AddField("Support", "Join our [server](https://discord.gg/hh9v4eF) to report any bugs.");

            eb.WithAuthor(botAuth);

            await ReplyAsync("", false, eb.Build());
        }
    }
}
