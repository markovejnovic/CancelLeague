using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord;
using Discord.Net;
using Discord.WebSocket;
using System.Collections.Generic;

namespace ProcessLock
{
    /// <summary>
    /// This event is fired when time has been bought to play a game.
    /// </summary>
    /// <param name="gameName">The name of the game</param>
    /// <param name="time">The hours added to play the game.</param>
    /// <returns>The total hours left to play the game.</returns>
    public delegate float BoughtTimeNotify(string gameName, float hours);
    public delegate void OnError(Exception err);

    internal class DiscordBot
    {
        public event BoughtTimeNotify BoughtTime;
        public event OnError OnError;
        public List<string> BlacklistedUsers = new List<string>();

        private static readonly string MSG_MISUNDERSTOOD =
            "❓ Sorry. I don't understand that.\n" +
            "This bot supports the following commands:\n" +
            "```\n" +
            "!csp addtime <GAME> <HOURS>h" +
                "\tLets Siva play <GAME> for <HOURS> hours.\n" +
            "```\n" +
            "The supported games are: {0}\n" +
            "Example usage:\n" +
            "```\n" +
            "!csp buytime league 1.5h\n" +
            "```";
        private static readonly string MSG_UNKNOWNGAME =
            "❌ `{0}` is not a valid game. Valid games are: {1}";
        private static readonly string MSG_BOUGHT_TIME =
            "✅ You have bought Siva {0} hours of {1}.\n" +
            "He can now play {2} hours of {1}.";
        private static readonly string MSG_BLACKLIST =
            "🛑 You seem to be blacklisted. Refusing.";
        private static readonly string BUYTIME_COMMAND =
            @"^!csp buytime ([\w]+) ([0-9]+(?:\.[0-9]+)?)h$";
        private static readonly string[] SUPPORTED_GAMES = { "league" };

        private DiscordSocketClient client;

        public DiscordBot()
        {
            client = new DiscordSocketClient();
            client.Log += Log;
            client.MessageReceived += Client_MessageReceived;
        }

        public async Task StartAsync(string token)
        {
            await client.LoginAsync(TokenType.Bot, token);
            await client.StartAsync();
        }

        private Task ReplyUnparsable(SocketMessage arg)
        {
            try
            {
                arg.Author.SendMessageAsync(
                    String.Format(MSG_MISUNDERSTOOD,
                    String.Join(", ",
                        SUPPORTED_GAMES.Select(gm => $"`{gm}`").ToArray())));
            }
            catch (HttpException ex)
            {
                Console.WriteLine(ex.Message);
            }
            return Task.CompletedTask;
        }

        private Task ReplySuccess(SocketMessage arg, string game, float time,
                                    float totalHours)
        {
            try
            {
                arg.Author.SendMessageAsync(
                    String.Format(MSG_BOUGHT_TIME, time, game, totalHours));
            }
            catch (HttpException ex)
            {
                Console.WriteLine(ex.Message);
            }
            return Task.CompletedTask;
        }

        private Task ReplyBlacklist(SocketMessage arg)
        {
            try
            {
                arg.Author.SendMessageAsync(MSG_BLACKLIST);
            }
            catch (HttpException ex)
            {
                Console.WriteLine(ex.Message);
            }
            return Task.CompletedTask;
        }

        private Task ReplyUnknownGame(SocketMessage arg, string game)
        {
            try
            {
                arg.Author.SendMessageAsync(
                    String.Format(MSG_UNKNOWNGAME,
                    game,
                    String.Join(", ",
                        SUPPORTED_GAMES.Select(gm => $"`{gm}`").ToArray())));
            }
            catch (HttpException ex)
            {
                Console.WriteLine(ex.Message);
            }
            return Task.CompletedTask;
        }

        private Task Client_MessageReceived(SocketMessage arg)
        {
            if (arg.Type != MessageType.Default) return Task.CompletedTask;
            if (!arg.Content.StartsWith("!csp")) return Task.CompletedTask;

            Regex pattern = new Regex(BUYTIME_COMMAND);
            Match matches = pattern.Match(arg.Content);

            if (!matches.Success)
                return ReplyUnparsable(arg);

            if (BlacklistedUsers.Contains($"{arg.Author.Username}#{arg.Author.Discriminator}"))
                return ReplyBlacklist(arg);

            Console.Write($"[{arg.Author.Username}]: {arg.Content}\n");

            string game = matches.Groups[1].Value;

            if (!Array.Exists(SUPPORTED_GAMES, x => x == game))
                return ReplyUnknownGame(arg, game);

            float hours = float.Parse(matches.Groups[2].Value);

            float totalHours = BoughtTime != null
                ? BoughtTime.Invoke(game, hours)
                : hours;
            return ReplySuccess(arg, game, hours, totalHours);
        }

        private Task Log(LogMessage msg)
        {
            Console.WriteLine(msg.ToString());

            if (msg.Exception != null)
            {
                OnError?.Invoke(msg.Exception);
                return Task.CompletedTask;
            }
            
            return Task.CompletedTask;
        }
    }
}
