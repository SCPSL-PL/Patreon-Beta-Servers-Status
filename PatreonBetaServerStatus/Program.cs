#pragma warning disable CS8602
#pragma warning disable CS8604
#pragma warning disable CA2211
using DSharpPlus;
using DSharpPlus.Entities;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace PatreonBetaServerStatus;

public static class Program
{
    private static readonly HttpClient HttpClient = new();

    public static PeriodicTimer Timer => _timer ??= new PeriodicTimer(TimeSpan.FromSeconds(Config.RefreshRate));
    private static PeriodicTimer? _timer;

    public static DiscordWebhookClient? Client => _client ??= new();
    private static DiscordWebhookClient? _client;

    public static DiscordWebhook? Webhook;

    public static Config Config { get; private set; } = null!;

    public static void Main() => MainAsync().GetAwaiter().GetResult();

    public static async Task MainAsync()
    {
        Log($"Patreon Beta Servers Status by Tajemniczy Typiarz", ConsoleColor.Magenta);

        if (!File.Exists(ConfigPath))
        {
            Log($"Config not Found! Creating Config...", ConsoleColor.DarkRed);
            await File.WriteAllTextAsync(ConfigPath, JsonConvert.SerializeObject(new Config(), Formatting.Indented));
            Environment.Exit(0);
            return;
        }
        Config = JsonConvert.DeserializeObject<Config>(await File.ReadAllTextAsync(ConfigPath))!;
        Log("Config Loaded!", ConsoleColor.Green);

        Webhook = await Client.AddWebhookAsync(Config.WebhookId, Config.WebhookToken);
        Log("Application started.", ConsoleColor.Green);

        await SendInfo();
        await Task.Delay(-1);
    }

    public static async Task SendInfo()
    {
        while (await Timer.WaitForNextTickAsync())
        {
            List<ServerObject> servers = new();
            
            JArray jarray = JArray.Parse(await HttpClient.GetStringAsync("http://api.scpsecretlab.pl/lobbylist/"));
            IEnumerable<JToken> tokens = from token in jarray where token["version"].Value<string>().Equals(Config.GameVersion) select token;

            foreach (JToken token in tokens)
            {
                byte[] bytes = Convert.FromBase64String(token.SelectToken("info").ToString());
                string name = Regex.Replace(Encoding.UTF8.GetString(bytes), "<[^>]*>", "");

                servers.Add(new(name, token.SelectToken("continentCode").ToString(), token.SelectToken("players").ToString()));
            }

            DiscordEmbedBuilder embed = new DiscordEmbedBuilder()
                .WithTitle("Patreon Beta Servers Status")
                .WithAuthor("Tajemniczy Typiarz", iconUrl: "https://cdn.discordapp.com/attachments/777200142569832469/1095713871525855364/NowyTajemniczy5.png")
                .WithColor(DiscordColor.DarkButNotBlack)
                .WithFooter($"Servers Version: {Config.GameVersion} • Refreshes every {Config.RefreshRate} seconds")
                .WithTimestamp(DateTime.UtcNow);


            foreach (ServerObject server in servers.OrderByDescending(x => x.Players))
            {
                embed.AddField($":flag_{ContinentCodesToFlags[server.ContinentCode]}: | {server.Name}", $"```ansi\n\u001b[1;36m{server.Players}\u001b[0m```");
            };

            await Webhook.EditMessageAsync(Config.MessageId, new DiscordWebhookBuilder().AddEmbed(embed));
            Log($"Servers Status Refreshed: {servers.Count} servers on the list.", ConsoleColor.Cyan);
        }
    }

    public static Dictionary<string, string> ContinentCodesToFlags = new()
    {
        { "EU", "eu" },
        { "NA", "us" },
        { "AS", "cn" },
        { "OC", "au" }
    };

    public static void Log(string message, ConsoleColor consoleColor)
    {
        Console.ForegroundColor = consoleColor;
        Console.WriteLine("[{0:dd/MM/yyyy HH:mm:ss}] {1}", DateTime.Now, message);
        Console.ResetColor();
    }

    private const string ConfigPath = "config.json";
}