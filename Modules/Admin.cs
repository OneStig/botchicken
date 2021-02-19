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
    public class Admin : ModuleBase<SocketCommandContext>
    {
        [Command("faq")]
        public async Task prfaq()
        {
            if (Context.User.Id == 265965692149432344)
            {
                var embed = new EmbedBuilder();

                var botAuth = new EmbedAuthorBuilder();
                botAuth.WithName("FAQ");
                botAuth.WithIconUrl("https://cdn.discordapp.com/avatars/286697179949694977/8304744891c425c8b75dcc44e46af5f7.png");
                embed.WithAuthor(botAuth);

                embed.AddField("Q: How do I add this bot to my own server?", "> [Click here](https://discord.com/oauth2/authorize?client_id=286697179949694977&scope=bot&permissions=268782656) to invite the bot");
                
                embed.AddField("Q: Why is my inventory value wrong/won't show up?", "> At the moment, this bot relies on CSGO Backpack for inventory values. " +
                               "This won't account for stickered items, special floats, or certain patterns (blue gems, dopplers etc.). " +
                               "If your inventory doesn't show up, click the refresh link until your profile loads and rerun the command. " +
                               "In the future this feature will completely be overhauled and neither should be an issue.");

                embed.AddField("Q: Where do I report a bug?", "> Open a ticket in <#812128604677734400>, and someone will take a look as soon as possible");

                embed.AddField("Other Useful Links", "[Top.gg page](https://top.gg/bot/286697179949694977/)\n[Skinport affiliate](https://skinport.com/r/botchicken)");
                embed.WithColor(Color.Orange);

                await ReplyAsync("", false, embed.Build());
            }
        }


    }
}
