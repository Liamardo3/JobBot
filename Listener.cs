using Discord;
using Discord.WebSocket;
using dotenv.net;

namespace JobBot;

public class Listener
{
    private readonly DiscordSocketClient _client;
    private readonly ulong _channelId;

    public Listener()
    {
        DotEnv.Load();

        var token     = Environment.GetEnvironmentVariable("DISCORD_TOKEN")     ?? "";
        var channelId = Environment.GetEnvironmentVariable("DISCORD_CHANNEL_ID") ?? "";

        if (string.IsNullOrWhiteSpace(token))
            throw new Exception("DISCORD_TOKEN is missing from .env");
        if (string.IsNullOrWhiteSpace(channelId))
            throw new Exception("DISCORD_CHANNEL_ID is missing from .env");

        _channelId = ulong.Parse(channelId);
        _client = new DiscordSocketClient(new DiscordSocketConfig
        {
            GatewayIntents = GatewayIntents.Guilds
                           | GatewayIntents.GuildMessages
                           | GatewayIntents.MessageContent
        });
    }

    public async Task StartAsync()
    {
        _client.MessageReceived += HandleMessageAsync;

        await _client.LoginAsync(TokenType.Bot,
            Environment.GetEnvironmentVariable("DISCORD_TOKEN"));
        await _client.StartAsync();

        Console.WriteLine("Listener connected, waiting for !search...");
    }

    private async Task HandleMessageAsync(SocketMessage message)
    {
        // ignore bots and messages outside our channel
        if (message.Author.IsBot) return;
        if (message.Channel.Id != _channelId) return;
        if (message.Content.Trim().ToLower() != "!search") return;

        Console.WriteLine($"!search triggered by {message.Author.Username}");

        await message.Channel.SendMessageAsync("🔍 Searching for new listings...");

        try
        {
            var seenJobs = new SeenJobs();
            var allJobs  = await Scraper.ScrapeLinkedInAsync();
            var newJobs  = allJobs.Where(j => !seenJobs.HasSeen(j.Url)).ToList();

            var bot = new Bot();

            if (newJobs.Count > 0)
            {
                await bot.PostJobsAsync(newJobs);
                seenJobs.MarkSeen(newJobs.Select(j => j.Url));
            }
            else
            {
                await message.Channel.SendMessageAsync(
                    "✅ Search complete — no new listings since last run.");
            }
        }
        catch (Exception ex)
        {
            await message.Channel.SendMessageAsync($"❌ Error during search: {ex.Message}");
            Console.WriteLine($"Error: {ex}");
        }
    }
}