using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using DiscordBotsList.Api;
using DiscordBotsList;
using DiscordBotsList.Api.Objects;

namespace botchicken
{
    class Program
    {
        DiscordSocketClient client;
        Services.CommandHandler handler;

        static void Main(string[] args)
        => new Program().StartAsync().GetAwaiter().GetResult();

        public async Task StartAsync()
        {
            if (ConfigLoader.cfg.token == "" || ConfigLoader.cfg.csgobackpack == null || 
                ConfigLoader.cfg.topgg == null || ConfigLoader.cfg.steamapi == null || 
                ResourceLoader.trader == null || AccountHandler.accounts == null ||
                ExchangeRate.rateAPI == null) 
            
            { return; }
            client = new DiscordSocketClient(new DiscordSocketConfig
            {
                LogLevel = LogSeverity.Warning
            });
            client.Log += Log;

            await client.LoginAsync(TokenType.Bot, ConfigLoader.cfg.token);
            await client.StartAsync();

            handler = new Services.CommandHandler();
            await handler.InitHandling(client);
            await client.SetGameAsync("-help");

            await Task.Delay(2000);

            if (ConfigLoader.cfg.token[1] == 'j')
            {
                AuthDiscordBotListApi DblApi = new AuthDiscordBotListApi(286697179949694977, ConfigLoader.cfg.topgg);

                IDblSelfBot me = await DblApi.GetMeAsync();

                int actual = client.Guilds.Count();
                Console.WriteLine("Guild Count: " + client.Guilds.Count().ToString());

                await me.UpdateStatsAsync(actual);
            }

            await Task.Delay(-1);
        }

        private async Task Log(LogMessage arg)
        {
            //if (arg.Exception != null && arg.Severity == LogSeverity.Error)
            //{
            //    await client.StopAsync();
            //    await client.StartAsync();
            //}
            await Task.Run(() => Console.WriteLine(arg.ToString()));
        }
    }
}