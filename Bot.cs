using Discord;
using Discord.WebSocket;
using dotenv.net;

namespace JobBot;

public class Bot
{
    private readonly DiscordSocketClient _client;
    private readonly ulong _channelId;

    public Bot()
    {
        DotEnv.Load();

        var token     = Environment.GetEnvironmentVariable("DISCORD_TOKEN")     ?? "";
        var channelId = Environment.GetEnvironmentVariable("DISCORD_CHANNEL_ID") ?? "";

        if (string.IsNullOrWhiteSpace(token))
            throw new Exception("DISCORD_TOKEN is missing from .env");
        if (string.IsNullOrWhiteSpace(channelId))
            throw new Exception("DISCORD_CHANNEL_ID is missing from .env");

        _channelId = ulong.Parse(channelId);
        _client    = new DiscordSocketClient(new DiscordSocketConfig
        {
            GatewayIntents = GatewayIntents.Guilds
        });
    }

    public async Task PostJobsAsync(List<JobListing> jobs)
    {
        await _client.LoginAsync(TokenType.Bot,
            Environment.GetEnvironmentVariable("DISCORD_TOKEN"));
        await _client.StartAsync();

        // wait until the client is ready
        var ready = new TaskCompletionSource<bool>();
        _client.Ready += () => { ready.SetResult(true); return Task.CompletedTask; };
        await ready.Task;

        var channel = _client.GetChannel(_channelId) as IMessageChannel;
        if (channel == null)
            throw new Exception($"Channel {_channelId} not found — check your DISCORD_CHANNEL_ID");

        // header message
        await channel.SendMessageAsync(
            $"📋 **JobBot Daily Report** — {DateTime.Now:MMMM d, yyyy}\n" +
            $"Found **{jobs.Count}** relevant listings today\n" +
            "─────────────────────────────────");

        // send jobs in batches — Discord has a 2000 char message limit
        foreach (var job in jobs)
        {
            var msg = $"**{job.Title}**\n" +
                      $"🏢 {job.Company}\n" +
                      $"🔗 {job.Url}";

            await channel.SendMessageAsync(msg);
            await Task.Delay(1000); // rate limit safety — 1 message per second
        }

        await channel.SendMessageAsync("─────────────────────────────────\n✅ End of today's listings");

        await _client.StopAsync();
    }

    public async Task PostNoNewJobsAsync()
    {
        await _client.LoginAsync(TokenType.Bot,
            Environment.GetEnvironmentVariable("DISCORD_TOKEN"));
        await _client.StartAsync();

        var ready = new TaskCompletionSource<bool>();
        _client.Ready += () => { ready.SetResult(true); return Task.CompletedTask; };
        await ready.Task;

        var channel = _client.GetChannel(_channelId) as IMessageChannel;
        if (channel == null)
            throw new Exception($"Channel {_channelId} not found — check your DISCORD_CHANNEL_ID");

        await channel.SendMessageAsync(
            $"✅ **JobBot checked in** — {DateTime.Now:MMMM d, yyyy h:mm tt}\n" +
            $"No new listings since last run.");

        await _client.StopAsync();
    }

}