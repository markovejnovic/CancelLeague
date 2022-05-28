using System;
using System.Linq;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Win32;

namespace ProcessLock
{
    internal class Program
    {
        private static DiscordBot bot;
        private static string botToken;
        private static GameDataProvider gameDataProvider;

        private static ProcessIdentifier[] targetProcesses = {
            new ProcessIdentifier("LeagueClient", new string[] { "league" })
        };

        static string GetDiscordToken()
        {
            const string REG_TOKEN_PATH = "HKEY_CURRENT_USER\\SOFTWARE\\ProcessLock\\";
            const string REG_TOKEN_NAME = "DiscordBotToken";
            string token = (string)Registry.GetValue(
                REG_TOKEN_PATH,
                REG_TOKEN_NAME,
                null
            );
            if (token != null)
                return token;

            AutoClosingMessageBox.Show(
                "Could not load the token. You have 2 minutes to solve this.\n" +
                $"Look under {REG_TOKEN_PATH}\\{REG_TOKEN_NAME}",
                "ProcessLock",
            2 * 60 * 1000);
            return (string)Registry.GetValue(
                REG_TOKEN_PATH,
                REG_TOKEN_NAME,
                null
            );
        }

        static void HandleError(Exception ex)
        {
            using (EventLog logger = new EventLog("Application"))
            {
                logger.Source = "ProcessLock";
                logger.WriteEntry($"{ex}", EventLogEntryType.Error);
            }
            Environment.Exit(-1);
        }

        static async Task AsyncMain(string[] args)
        {
            try
            {
                botToken = GetDiscordToken();
                bot = new DiscordBot();
                bot.BlacklistedUsers.Add("sokolica#6546");
                gameDataProvider = new GameDataProvider();

                foreach (var p in targetProcesses)
                    gameDataProvider.TrackProcess(p);

                bot.BoughtTime += Bot_BoughtTime;
                bot.OnError += HandleError;

                await bot.StartAsync(botToken);
                _ = TrackOffenses();
                await Task.Delay(-1);
            } catch (Exception ex)
            {
                HandleError(ex);
            }
        }

        static async Task TrackOffenses()
        {
            while (true)
            {
                // Find offending processes and immediately kill them.
                foreach (Process process in gameDataProvider.GetOffendingProcesses())
                {
                    Console.WriteLine($"Offending process {process.ProcessName}. Killing.");
                    process.Kill();
                }

                // Decrement a second and sleep.
                gameDataProvider.DecrementSecond();
                await Task.Delay(1000);
            }
        }

        static void Main(string[] args)
        {
            // Mark this process as a system, critical process. If it is
            // killed, the system crashes (BSOD -- I'm not kidding).
            ProcessProtection.Protect();

            AsyncMain(args).Wait();
        }

        private static float Bot_BoughtTime(string gameName, float hours)
        {
            var proc = targetProcesses.First(p => p.IsAlias(gameName));
            gameDataProvider.AddSeconds(proc, (int)(60 * 60 * hours));
            return (float)gameDataProvider.SecondsLeft[proc] / 60 / 60;
        }
    }
}
